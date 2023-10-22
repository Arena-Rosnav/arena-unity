using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RestServer.Helper;

namespace RestServer {
    /// <summary>
    /// Allows you to (a)synchronously interact with unity inside of web requests.
    /// </summary>
    public class ThreadingHelper {
        public static readonly ThreadingHelper Instance = new ThreadingHelper();

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private readonly Queue<Workload> _workloads = new Queue<Workload>();

        private Thread _mainThreadReference;

        /// <summary>
        /// Reference to the main thread; used to determine if a workload needs to be enqueued. If this is incorrect
        /// a dead lock can occur.
        /// </summary>
        public Thread MainThreadReference {
            get => _mainThreadReference;
            set {
                if (value != null) {
                    _mainThreadReference = value;
                }
            }
        }

        /// <summary>How long to wait until synchronous workload has been executed inside unity.</summary>
        public int ThreadingMillisecondsTimeout = 1000;

        /// <summary>
        /// Execute a coroutine async inside the unity thread. Thread-safe.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="profilerMarkerText">Name to assign this workload to find it more easily in the unity profiler.</param>
        public void ExecuteAsyncCoroutine(Func<IEnumerator> coroutine, string profilerMarkerText = "ExecuteAsyncCoroutine") {
            var w = new Workload {
                HandlerCoroutine = coroutine,
                ProfileMarkerText = profilerMarkerText
            };
            EnqueueWorkload(w);
        }

        /// <summary>
        /// Execute an anonymous function inside the unity thread. Thread-safe.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="profilerMarkerText">Name to assign this workload to find it more easily in the unity profiler.</param>
        public void ExecuteAsync(Action action, string profilerMarkerText = "ExecuteAsync") {
            var w = new Workload {
                HandlerAction = () => {
                    action.Invoke();
                    return null;
                },
                ProfileMarkerText = profilerMarkerText
            };
            EnqueueWorkload(w);
        }

        /// <summary>
        /// Execute an anonymous function inside the unity thread synchronously. Thread-safe.
        ///
        /// Please do not return unity objects for further manipulation with this method, as unity methods are not
        /// thread-safe.
        /// </summary>
        /// <param name="action">The function to execute</param>
        /// <param name="profilerMarkerText">Name to assign this workload to find it more easily in the unity profiler.</param>
        /// <returns>The return value of the function.</returns>
        public T ExecuteSync<T>(Func<T> action, string profilerMarkerText = "ExecuteSync") {
            var w = new Workload {
                HandlerAction = () => action.Invoke(), // "cast"
                WaitHandle = new AutoResetEvent(false),
                ProfileMarkerText = profilerMarkerText
            };

            EnqueueWorkload(w);

            // Wait for unity thread to signal execution completion.
            if (!w.WaitHandle.WaitOne(ThreadingMillisecondsTimeout))
                throw new TimeoutException("Execution couldn't be finished on main-thread.");

            if (w._Exception != null) {
                // The execution has resulted in an unhandled exception, rethrow in this thread.
                throw w._Exception;
            }

            return (T)w._ReturnValue;
        }

        /// <summary>
        /// Enqueue a workload directly. Please try to use ExecuteSync and ExecuteAsync. Thread-safe.
        ///
        /// Does <i>not</i> enqueue a workload and throws an exception if the current thread is the main thread.
        /// </summary>
        /// <param name="workload"></param>
        public void EnqueueWorkload(Workload workload) {
            if (object.ReferenceEquals(Thread.CurrentThread, _mainThreadReference)) {
                // Enqueuing workloads from the main thread does result in a dead lock. Since you are already on the main thread, you can just execute the 
                // unity code
                throw new InvalidOperationException("Can't enqueue workloads from the main thread, execute directly instead.");
            }

            _lock.EnterWriteLock();
            try {
#if ENABLE_PROFILER && RESTSERVER_PROFILING_CORE
                RestServerProfilerCounters.ThreadingHelperCallsCount.Value += 1;
#endif
                _workloads.Enqueue(workload);
            }
            finally {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// True if the ThreadingHelper has workload to be processed; false otherwise. Thread-safe.
        /// </summary>
        /// <returns>True if the ThreadingHelper has workload to be processed; false otherwise.</returns>
        public bool HasWorkload() {
            _lock.EnterReadLock();
            try {
                return _workloads.Count > 0;
            }
            finally {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Removes workload from the queue for processing. Thread-safe.
        /// </summary>
        /// <returns>Workload for processing.</returns>
        public Workload DequeueWork() {
            _lock.EnterWriteLock();
            try {
                return _workloads.Dequeue();
            }
            finally {
                _lock.ExitWriteLock();
            }
        }
        
        /// <summary>
        /// The length of the workload backlog. Thread-safe.
        /// </summary>
        public int WorkloadBacklogLength {
            get {
                _lock.EnterReadLock();
                try {
                    return _workloads.Count;
                }
                finally {
                    _lock.ExitReadLock();
                }
            }
        }
    }

    /// <summary>
    /// Workload DTO that is used for Sync and Async execution.
    /// The executor (RestServer) doesn't know exactly which is sync and which is async, if the wait handle is existing it's used for signaling. That can be used to implement
    /// a sync behaviour.
    /// </summary>
    public class Workload {
        /// <summary>
        /// Text to assign this workload to easily identify it in the profiler.
        /// </summary>
        public string ProfileMarkerText;

        /// <summary>
        /// Co-Routine to execute on the main thread. Either this or HandlerAction can be used.
        /// </summary>
        public Func<IEnumerator> HandlerCoroutine;

        /// <summary>
        /// Action to execute on the main thread. If WaitHandle is != null, then the action will be executed synchronously
        /// and the return value is handed back to the caller. Either this or HandlerCoroutine can be used. 
        /// </summary>
        public Func<object> HandlerAction;

        /// <summary>
        /// Wait handle for multi-threaded signaling. Only used when HandlerAction should be executed synchronously.
        /// </summary>
        public AutoResetEvent WaitHandle;

        /// <summary>
        /// In case the HandlerAction is executed synchronously, this stores the return value to be handed back.
        /// </summary>
        public object _ReturnValue;

        /// <summary>
        /// In case the synchronous execution throws an exception, the exception is passed back to the calling thread. 
        /// </summary>
        public Exception _Exception;
    }
}