using System;
using System.Threading;

namespace RestServer.Helper {
    /// <summary>
    /// Helper class that blocks a process so it's not triggered again. This is helpful to prevent repeated calls to interrupt long running actions inside of the unity rendering.
    /// Helpful for animations or anything else that runs longer than one request.
    ///
    /// 
    /// </summary>
    /// <example>
    /// private SimpleInterlock _lock = new SimpleInterlock();
    ///
    /// public void StartAnimation() {
    ///     if(_lock.isRunning()) {
    ///         return; // animation is running, do not start again
    ///     }
    ///     using (var interlock = _lock.DoWork()) {
    ///         // Do animation
    ///     }
    /// } 
    /// </example>
    public sealed class SimpleInterlock {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private bool _running;

        /// <summary>
        /// Is the workload currently running and locked?
        /// </summary>
        public bool isRunning {
            get {
                _lock.EnterReadLock();
                try {
                    return _running;
                } finally {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Method to call in a using statement to lock the workload.
        /// </summary>
        public SimpleInterlockRunner DoWork() {
            _lock.EnterWriteLock();
            try {
                _running = true;
            } finally {
                _lock.ExitWriteLock();
            }

            return new SimpleInterlockRunner(this);
        }

        internal void ExitWork() {
            _lock.EnterWriteLock();
            try {
                _running = false;
            } finally {
                _lock.ExitWriteLock();
            }
        }
    }

    public sealed class SimpleInterlockRunner : IDisposable {
        private SimpleInterlock _interlock;

        public SimpleInterlockRunner(SimpleInterlock interlock) {
            _interlock = interlock;
        }

        public void Dispose() {
            _interlock.ExitWork();
        }
    }
}