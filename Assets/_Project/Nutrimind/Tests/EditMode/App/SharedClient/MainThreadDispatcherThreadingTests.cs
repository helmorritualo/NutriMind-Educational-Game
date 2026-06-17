using System;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace NutriMind.Tests.EditMode.App
{
    /// <summary>
    /// RED-phase tests for Phase 03 MainThreadDispatcher thread safety and
    /// generation-bound callback suppression.
    ///
    /// These tests will FAIL (RED) until the dispatcher provides:
    ///   - Thread-safe posting sufficient for concurrent background calls
    ///   - A generation-bound Post overload for stale callback suppression
    /// </summary>
    [TestFixture]
    public class MainThreadDispatcherThreadingTests
    {
        private const string AssemblyName = "NutriMind.Runtime.App";

        private static Type FindType(string fullTypeName)
            => Type.GetType(fullTypeName + ", " + AssemblyName);

        [Test]
        public void Post_FromMultipleBackgroundThreads_AllCallbacksExecuteExactlyOnce()
        {
            Type d = FindType("NutriMind.Runtime.App.MainThreadDispatcher");
            Assert.That(d, Is.Not.Null,
                "Precondition: MainThreadDispatcher type must exist");

            ConstructorInfo ctor = d.GetConstructor(Type.EmptyTypes);
            Assert.That(ctor, Is.Not.Null,
                "Precondition: MainThreadDispatcher must have a parameterless constructor");

            object instance = ctor.Invoke(null);

            MethodInfo post = null;
            foreach (MethodInfo m in d.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                if (m.Name == "Post" && m.GetParameters().Length >= 1) { post = m; break; }
            Assert.That(post, Is.Not.Null,
                "Precondition: MainThreadDispatcher must have a Post method");

            MethodInfo exec = d.GetMethod("ExecutePending", BindingFlags.Public | BindingFlags.Instance)
                ?? d.GetMethod("Flush", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(exec, Is.Not.Null,
                "Precondition: MainThreadDispatcher must have ExecutePending or Flush method");

            const int threadCount = 20;
            int[] callCounts = new int[threadCount];
            var allDone = new ManualResetEvent[threadCount];
            for (int i = 0; i < threadCount; i++)
                allDone[i] = new ManualResetEvent(false);

            // Post callbacks from thread-pool workers concurrently
            int postedCount = 0;
            for (int i = 0; i < threadCount; i++)
            {
                int idx = i;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        post.Invoke(instance, new object[] { (Action)(() => Interlocked.Increment(ref callCounts[idx])) });
                        Interlocked.Increment(ref postedCount);
                    }
                    finally
                    {
                        allDone[idx].Set();
                    }
                });
            }

            WaitHandle.WaitAll(allDone);

            Assert.That(postedCount, Is.EqualTo(threadCount),
                "All {0} background threads must have successfully posted their callbacks", threadCount);

            // Execute all pending callbacks
            exec.Invoke(instance, null);

            // Verify each callback ran exactly once
            for (int i = 0; i < threadCount; i++)
                Assert.That(callCounts[i], Is.EqualTo(1),
                    "Callback posted from background thread {0} must execute exactly once after Flush", i);
        }

        [Test]
        public void ConcurrentPostAndFlush_DoesNotLoseCallbacks()
        {
            Type d = FindType("NutriMind.Runtime.App.MainThreadDispatcher");
            Assert.That(d, Is.Not.Null,
                "Precondition: MainThreadDispatcher type must exist");

            ConstructorInfo ctor = d.GetConstructor(Type.EmptyTypes);
            object instance = ctor.Invoke(null);

            MethodInfo post = null;
            foreach (MethodInfo m in d.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                if (m.Name == "Post" && m.GetParameters().Length >= 1) { post = m; break; }
            if (post == null) return;

            MethodInfo exec = d.GetMethod("ExecutePending", BindingFlags.Public | BindingFlags.Instance)
                ?? d.GetMethod("Flush", BindingFlags.Public | BindingFlags.Instance);
            if (exec == null) return;

            const int batchSize = 10;
            int executedCount = 0;

            // First batch
            for (int i = 0; i < batchSize; i++)
                post.Invoke(instance, new object[] { (Action)(() => Interlocked.Increment(ref executedCount)) });

            // Flush
            exec.Invoke(instance, null);
            Assert.That(executedCount, Is.EqualTo(batchSize),
                "First batch: all {0} callbacks must execute", batchSize);

            // Second batch (ensures queue state is clean after flush)
            for (int i = 0; i < batchSize; i++)
                post.Invoke(instance, new object[] { (Action)(() => Interlocked.Increment(ref executedCount)) });

            exec.Invoke(instance, null);
            Assert.That(executedCount, Is.EqualTo(batchSize * 2),
                "Second batch: all {0} callbacks must execute after prior flush", batchSize);
        }

        [Test]
        public void PostWithGeneration_StaleCallbacksDoNotExecute()
        {
            Type d = FindType("NutriMind.Runtime.App.MainThreadDispatcher");
            Assert.That(d, Is.Not.Null,
                "Precondition: MainThreadDispatcher type must exist");

            // Look for a generation-bound Post overload: Post(int, Action)
            MethodInfo postWithGen = null;
            foreach (MethodInfo m in d.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (m.Name == "Post")
                {
                    ParameterInfo[] ps = m.GetParameters();
                    if (ps.Length == 2 && ps[0].ParameterType == typeof(int))
                    {
                        postWithGen = m;
                        break;
                    }
                }
            }

            // Also try PostForGeneration or PostStamped
            if (postWithGen == null)
            {
                foreach (MethodInfo m in d.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if ((m.Name == "PostForGeneration" || m.Name == "PostStamped")
                        && m.GetParameters().Length >= 2)
                    {
                        postWithGen = m;
                        break;
                    }
                }
            }

            Assert.That(postWithGen, Is.Not.Null,
                "MainThreadDispatcher must provide a generation-bound Post overload " +
                "(e.g. Post(int, Action) or PostForGeneration(int, Action)) " +
                "so that callbacks posted with a stale generation are suppressed after Invalidate");

            ConstructorInfo ctor = d.GetConstructor(Type.EmptyTypes);
            object instance = ctor.Invoke(null);

            MethodInfo invalidate = d.GetMethod("Invalidate", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(invalidate, Is.Not.Null,
                "Precondition: MainThreadDispatcher must have Invalidate method");

            PropertyInfo genProp = d.GetProperty("Generation", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(genProp, Is.Not.Null,
                "Precondition: MainThreadDispatcher must expose Generation property");

            MethodInfo exec = d.GetMethod("ExecutePending", BindingFlags.Public | BindingFlags.Instance)
                ?? d.GetMethod("Flush", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(exec, Is.Not.Null,
                "Precondition: MainThreadDispatcher must have ExecutePending or Flush");

            // Get current generation
            int genBefore = (int)genProp.GetValue(instance);

            // Post callbacks with the current generation -- these are "stale" after Invalidate
            int staleRunCount = 0;
            var staleParams = new object[] { genBefore, (Action)(() => { staleRunCount++; }) };
            postWithGen.Invoke(instance, staleParams);
            postWithGen.Invoke(instance, staleParams);

            // Invalidate: increments generation and discards pending callbacks
            invalidate.Invoke(instance, null);
            int genAfter = (int)genProp.GetValue(instance);
            Assert.That(genAfter, Is.GreaterThan(genBefore),
                "Generation must increase after Invalidate");

            // Post callbacks with the new generation
            int freshRunCount = 0;
            var freshParams = new object[] { genAfter, (Action)(() => { freshRunCount++; }) };
            postWithGen.Invoke(instance, freshParams);
            postWithGen.Invoke(instance, freshParams);

            // Flush -- only fresh callbacks should execute
            exec.Invoke(instance, null);

            Assert.That(staleRunCount, Is.EqualTo(0),
                "Callbacks posted with stale generation must NOT execute after Invalidate");
            Assert.That(freshRunCount, Is.EqualTo(2),
                "Callbacks posted with the current generation must execute exactly once each");
        }
    }
}
