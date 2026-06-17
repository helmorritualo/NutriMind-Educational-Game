using System;
using NutriMind.Runtime.App.Http;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Singleton composition root for the NutriMind application.
    /// <see cref="CreateForMode(DataProviderMode)"/> always configures the
    /// same canonical singleton exposed by <see cref="Instance"/> /
    /// <see cref="GetInstance()"/>; callers never receive separate service graphs.
    /// Holds one shared instance of each core service for the app lifetime.
    ///
    /// Default mode selection:
    /// <c>LocalDemoJson</c> when <c>UNITY_EDITOR || DEVELOPMENT_BUILD</c>, otherwise
    /// <c>Http</c>.  Explicit <c>CreateForMode(LocalDemoJson)</c> in release builds
    /// throws <see cref="InvalidOperationException"/> — no production silent fallback
    /// to fake/local data.
    /// </summary>
    public class CompositionRoot : IDisposable
    {
        /// <summary>
        /// The single canonical composition root instance.
        /// Created lazily on first access and can be replaced by
        /// <see cref="CreateForMode(DataProviderMode)"/> with a different mode.
        /// </summary>
        private static CompositionRoot s_instance;

        private static readonly object s_lock = new();

        private bool _disposed;

        /// <summary>
        /// Optional hidden <see cref="UnityEngine.GameObject"/> that pumps
        /// <see cref="MainThreadDispatcher"/> callbacks each frame.
        /// Only created in <see cref="DataProviderMode.Http"/> mode.
        /// </summary>
        private UnityEngine.GameObject _pumpGameObject;

        /// <summary>
        /// Returns the appropriate default data-provider mode for the
        /// current build configuration:
        /// <c>LocalDemoJson</c> in editor and development builds,
        /// <c>Http</c> in release/player builds.
        /// </summary>
        private static DataProviderMode GetDefaultMode()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return DataProviderMode.LocalDemoJson;
#else
            return DataProviderMode.Http;
#endif
        }

        /// <summary>
        /// The canonical singleton composition root instance.
        /// When nothing has been configured yet, lazily creates a root
        /// using <see cref="GetDefaultMode"/>.
        /// After <see cref="CreateForMode(DataProviderMode)"/> has been called,
        /// returns the same configured canonical root.
        /// </summary>
        public static CompositionRoot Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                        {
                            s_instance = new CompositionRoot(GetDefaultMode());
                        }
                    }
                }
                return s_instance;
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Shared services
        // ──────────────────────────────────────────────────────────────

        /// <summary>Application state machine for canonical flow transitions.</summary>
        public AppStateMachine StateMachine { get; }

        /// <summary>Current session scope (mode + session-owned state).</summary>
        public SessionScope Session { get; }

        /// <summary>The active game-data provider (local demo or HTTP).</summary>
        public IGameDataProvider DataProvider { get; }

        /// <summary>HTTP configuration for the HTTP provider mode.</summary>
        public HttpProviderConfig HttpConfig { get; }

        /// <summary>Current authentication session state.</summary>
        public AuthSessionState AuthSession { get; }

        /// <summary>Revision-based sync polling service (HTTP mode only).</summary>
        public SyncPollingService? SyncPolling { get; }

        /// <summary>Scene key-to-path registry.</summary>
        public SceneRegistry SceneRegistry { get; }

        /// <summary>Scene-navigation service backed by the scene registry.</summary>
        public NavigationService NavigationService { get; }

        /// <summary>Asset catalog integrity validator.</summary>
        public AssetCatalogValidator AssetCatalog { get; }

        /// <summary>Attempt lifecycle coordinator.</summary>
        public AttemptCoordinator AttemptCoordinator { get; }

        /// <summary>Main-thread callback dispatcher.</summary>
        public MainThreadDispatcher Dispatcher { get; }

        /// <summary>
        /// Configures the canonical composition root for the requested mode.
        /// This method always returns the same canonical singleton that
        /// <see cref="Instance"/> and <see cref="GetInstance()"/> expose,
        /// so callers never obtain separate service graphs.
        ///
        /// If an existing canonical root already has the requested mode, it is
        /// returned unchanged.  If it has a different mode, the previous root is
        /// disposed and replaced with a new one configured for the requested mode.
        /// </summary>
        /// <param name="mode">The desired data-provider mode.</param>
        /// <returns>The canonical composition root configured for <paramref name="mode"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown in release builds (<c>!UNITY_EDITOR &amp;&amp; !DEVELOPMENT_BUILD</c>)
        /// when <paramref name="mode"/> is <see cref="DataProviderMode.LocalDemoJson"/>.
        /// No production silent fallback to fake/local data is permitted.
        /// </exception>
        public static CompositionRoot CreateForMode(DataProviderMode mode)
        {
            lock (s_lock)
            {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                if (mode == DataProviderMode.LocalDemoJson)
                {
                    throw new InvalidOperationException(
                        "LocalDemoJson provider cannot be used in release builds. " +
                        "Use DataProviderMode.Http for production deployment.");
                }
#endif

                // Canonical root already matches and is healthy — return it unchanged.
                // A disposed root must be replaced so callers receive a functional composition.
                if (s_instance != null && s_instance.Session.Mode == mode && !s_instance._disposed)
                    return s_instance;

                // Dispose the old root before replacing so that
                // HttpProvider, SyncPollingService, and dispatcher pump
                // are cleaned up safely.
                s_instance?.Dispose();

                // Create the new canonical root for the requested mode.
                s_instance = new CompositionRoot(mode);
                return s_instance;
            }
        }

        /// <summary>
        /// Initializes the composition root and creates all shared services.
        /// The data provider is chosen based on <paramref name="mode"/>.
        /// </summary>
        private CompositionRoot(DataProviderMode mode)
        {
            StateMachine = new AppStateMachine();
            Session = new SessionScope();
            Session.Mode = mode;
            AuthSession = new AuthSessionState();
            Session.AuthSessionState = AuthSession;
            HttpConfig = new HttpProviderConfig();
            SceneRegistry = new SceneRegistry();
            NavigationService = new NavigationService(SceneRegistry);
            AssetCatalog = new AssetCatalogValidator();
            AttemptCoordinator = new AttemptCoordinator();
            Dispatcher = new MainThreadDispatcher();

            // Choose the data provider based on the requested mode.
            if (mode == DataProviderMode.Http)
            {
                DataProvider = new HttpProvider(HttpConfig, AuthSession);
                SyncPolling = new SyncPollingService(DataProvider, HttpConfig, AuthSession, Dispatcher);
                CreateDispatcherPump();
            }
            else
            {
                DataProvider = new LocalDemoJsonProvider();
                SyncPolling = null;
            }
        }

        /// <summary>
        /// Creates a hidden GameObject with <see cref="DispatcherPumpBehaviour"/>
        /// that calls <see cref="MainThreadDispatcher.ExecutePending"/> each frame.
        /// The pump is safe to destroy and recreate across root replacements.
        /// </summary>
        private void CreateDispatcherPump()
        {
            _pumpGameObject = new UnityEngine.GameObject("NutriMind-DispatcherPump");
            _pumpGameObject.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            if (UnityEngine.Application.isPlaying)
                UnityEngine.Object.DontDestroyOnLoad(_pumpGameObject);

            var pump = _pumpGameObject.AddComponent<DispatcherPumpBehaviour>();
            pump.SetDispatcher(Dispatcher);
        }

        /// <summary>
        /// Gets the singleton composition root instance.
        /// Provided for test compatibility with <c>GetInstance()</c> lookup.
        /// </summary>
        public static CompositionRoot GetInstance() => Instance;

        /// <summary>
        /// Initializes the composition root.  Currently a no-op — all services
        /// are ready after construction.  Reserved for future async startup
        /// (e.g. data provider warm-up).
        /// </summary>
        public void Initialize()
        {
            // Phase 03: all services are constructed and ready.
        }

        /// <summary>
        /// Asynchronous initialization placeholder.
        /// </summary>
        public void InitializeAsync()
        {
            Initialize();
        }

        // ──────────────────────────────────────────────────────────────
        //  IDisposable
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Disposes managed resources owned by this composition root:
        /// stops and disposes <see cref="SyncPollingService"/>,
        /// disposes the <see cref="DataProvider"/> when it implements
        /// <see cref="IDisposable"/>, invalidates the <see cref="Dispatcher"/>,
        /// and destroys the dispatcher pump GameObject.
        /// Safe to call multiple times.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // Stop background polling first — no new callbacks after this.
            SyncPolling?.Dispose();

            // Discard any callbacks already queued to the old dispatcher.
            Dispatcher?.Invalidate();

            // Dispose the data provider (HttpProvider owns transport).
            (DataProvider as IDisposable)?.Dispose();

            // Destroy the hidden pump GameObject.
            if (_pumpGameObject != null)
            {
                if (UnityEngine.Application.isPlaying)
                    UnityEngine.Object.Destroy(_pumpGameObject);
                else
                    UnityEngine.Object.DestroyImmediate(_pumpGameObject);

                _pumpGameObject = null;
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Dispatcher pump (inner MonoBehaviour)
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Hidden MonoBehaviour installed on a runtime-created GameObject
        /// that flushes <see cref="MainThreadDispatcher"/> queued callbacks
        /// each frame via <c>Update()</c>.  Created only in HTTP mode by
        /// <see cref="CreateDispatcherPump"/>.
        /// </summary>
        private class DispatcherPumpBehaviour : UnityEngine.MonoBehaviour
        {
            private MainThreadDispatcher _dispatcher;

            public void SetDispatcher(MainThreadDispatcher dispatcher)
            {
                _dispatcher = dispatcher;
            }

            private void Update()
            {
                _dispatcher?.ExecutePending();
            }
        }
    }
}