using System;
using System.Threading;
using System.Threading.Tasks;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Runtime.App.Http
{
    /// <summary>
    /// Polls <see cref="IGameDataProvider.GetSyncStatusAsync"/> and surfaces
    /// change notifications when opaque revisions differ from the last known
    /// values.  The service is cancellation-aware and stops polling when the
    /// token is cancelled or the session token is cleared.
    /// </summary>
    /// <remarks>
    /// Polling is the fallback when realtime (WebSocket) events are unavailable.
    /// All authoritative data refreshes still happen over HTTPS REST.
    /// <para>
    /// Event invocations (<see cref="Changed"/> and <see cref="Polled"/>) are
    /// dispatched to the main thread via <see cref="MainThreadDispatcher"/> when
    /// one is provided.  If no dispatcher is available events fire from the
    /// background polling task (acceptable in test environments).
    /// </para>
    /// </remarks>
    public class SyncPollingService : IDisposable
    {
        private readonly IGameDataProvider _provider;
        private readonly HttpProviderConfig _config;
        private readonly AuthSessionState _session;
        private readonly MainThreadDispatcher _dispatcher;
        private readonly object _startStopLock = new();
        private CancellationTokenSource _cts;
        private SyncStatusDto _lastKnown;
        private bool _disposed;

        /// <summary>
        /// Raised when any opaque revision changes.  Consumers should refresh
        /// the corresponding authoritative data over HTTP.
        /// </summary>
        public event Action<SyncStatusDto> Changed;

        /// <summary>
        /// Raised on every successful poll, even when nothing changed.
        /// </summary>
        public event Action<SyncStatusDto> Polled;

        /// <summary>
        /// Creates a new polling service bound to the given provider and session.
        /// </summary>
        /// <param name="provider">The data provider to poll.</param>
        /// <param name="config">HTTP configuration for polling intervals.</param>
        /// <param name="session">Authentication session (tracks token state).</param>
        /// <param name="dispatcher">
        /// Optional main-thread dispatcher.  When provided, events are raised
        /// on the main thread; otherwise they fire from the background task.
        /// </param>
        public SyncPollingService(
            IGameDataProvider provider,
            HttpProviderConfig config,
            AuthSessionState session,
            MainThreadDispatcher dispatcher = null)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// Starts the background polling loop.  Safe to call multiple times;
        /// only the first call after construction (or after <see cref="Stop"/>)
        /// has effect.  After <c>Stop()</c>, calling <c>Start()</c> again
        /// creates a fresh cancellation scope.
        /// </summary>
        public void Start()
        {
            if (_disposed) return;

            CancellationTokenSource cts;
            lock (_startStopLock)
            {
                if (_cts != null)
                    return; // Already running.

                cts = new CancellationTokenSource();
                _cts = cts;
            }

            _ = PollLoopAsync(cts.Token);
        }

        /// <summary>
        /// Requests that polling stop.  Ongoing requests are cancelled.
        /// Safe to call when already stopped or disposed.
        /// </summary>
        public void Stop()
        {
            if (_disposed) return;

            CancellationTokenSource cts;
            lock (_startStopLock)
            {
                cts = _cts;
                _cts = null;
            }

            if (cts != null)
            {
                try { cts.Cancel(); }
                catch (ObjectDisposedException) { }
                cts.Dispose();
            }
        }

        private async Task PollLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && !_disposed)
            {
                // Stop if the session has been cleared.
                if (_session == null || string.IsNullOrEmpty(_session.Token))
                {
                    await WaitAsync(_config.DefaultPollIntervalSeconds, ct).ConfigureAwait(false);
                    continue;
                }

                DataResult<SyncStatusDto> result;
                try
                {
                    result = await _provider.GetSyncStatusAsync(ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    SafeDiagnostics.LogDiagnostic(
                        $"SyncPollingService poll failed: {SafeDiagnostics.SanitizeDiagnostics(ex.Message)}");
                    await WaitAsync(_config.DefaultPollIntervalSeconds, ct).ConfigureAwait(false);
                    continue;
                }

                if (result.Success && result.Data != null)
                {
                    var data = result.Data;

                    // Marshal events to main thread when dispatcher is available.
                    if (_dispatcher != null)
                    {
                        var gen = _dispatcher.Generation;
                        _dispatcher.Post(gen, () => Polled?.Invoke(data));

                        if (HasChanged(data))
                        {
                            _lastKnown = data;
                            _dispatcher.Post(gen, () => Changed?.Invoke(data));
                        }
                    }
                    else
                    {
                        Polled?.Invoke(data);

                        if (HasChanged(data))
                        {
                            _lastKnown = data;
                            Changed?.Invoke(data);
                        }
                    }

                    int interval = data.NextPollAfterSeconds ?? _config.DefaultPollIntervalSeconds;
                    await WaitAsync(interval, ct).ConfigureAwait(false);
                }
                else
                {
                    await WaitAsync(_config.DefaultPollIntervalSeconds, ct).ConfigureAwait(false);
                }
            }
        }

        private bool HasChanged(SyncStatusDto current)
        {
            if (_lastKnown == null)
                return true;

            return _lastKnown.StudentProgressRevision != current.StudentProgressRevision
                || _lastKnown.StudentSettingsRevision != current.StudentSettingsRevision
                || _lastKnown.QuizRevision != current.QuizRevision
                || _lastKnown.PublishedContentRevision != current.PublishedContentRevision
                || _lastKnown.RewardWalletRevision != current.RewardWalletRevision;
        }

        private static async Task WaitAsync(int seconds, CancellationToken ct)
        {
            seconds = Math.Max(0, seconds);
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(seconds), ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected on stop.
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Stop();
            _cts?.Dispose();
        }
    }
}
