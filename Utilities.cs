using Sandbox;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PluginManager.Utilities
{
    public class Log
    {
        public static void WriteLineAndConsole(string msg) => MySandboxGame.Log.WriteLineAndConsole("PluginManager: " + msg);
    }

    class AsyncLock
    {
        private readonly Task<IDisposable> _releaserTask;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IDisposable _releaser;

        public AsyncLock()
        {
            _releaser = new Releaser(_semaphore);
            _releaserTask = Task.FromResult(_releaser);
        }
        public IDisposable Lock()
        {
            _semaphore.Wait();
            return _releaser;
        }
        public Task<IDisposable> LockAsync()
        {
            var waitTask = _semaphore.WaitAsync();
            return waitTask.IsCompleted
                ? _releaserTask
                : waitTask.ContinueWith(
                    (_, releaser) => (IDisposable)releaser,
                    _releaser,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
        }
        private class Releaser : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            public Releaser(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }
            public void Dispose()
            {
                _semaphore.Release();
            }
        }
    }

    public static class HashSetExtension
    {
        public static bool Exists<T>(this System.Collections.Generic.HashSet<T> self, Predicate<T> match)
        {
            foreach (var e in self) if (match(e)) return true;

            return false;
        }
    }

}
