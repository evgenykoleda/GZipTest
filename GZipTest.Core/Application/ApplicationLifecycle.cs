namespace GZipTest.Core.Application
{
    internal class ApplicationLifecycle : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<Exception> _exceptions;
        private readonly object _sync;

        public ApplicationLifecycle()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _exceptions = new List<Exception>();
            _sync = new object();
        }

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public void OnError(Exception exception)
        {
            lock (_sync)
            {
                _exceptions.Add(exception);
            }

            _cancellationTokenSource.Cancel();
        }

        public Exception[] GetErrors()
        {
            lock (_sync)
            {
                return _exceptions.ToArray();
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }
}
