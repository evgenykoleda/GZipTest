using System.Collections.Concurrent;

namespace GZipTest.Core.DataStructures
{
    internal class ProducerConsumerQueue<T> : IDisposable
    {
        private readonly ConcurrentQueue<T> _items;
        private int _currentItemsCount;
        private readonly int _maxItemsCount;
        private readonly ManualResetEventSlim _ableToEnqueueEvent;
        private readonly ManualResetEventSlim _ableToDequeueEvent;
        private readonly int _producersCount;
        private int _completedProducersCount;
        private readonly ManualResetEventSlim _addingCompletedEvent;
        private readonly CancellationToken _cancellationToken;

        public ProducerConsumerQueue(int maxItemsCount, int producersCount, CancellationToken cancellationToken)
        {
            if (maxItemsCount <= 0)
                throw new ArgumentException($"Invalid maxItemsCount: {maxItemsCount}");
            if (producersCount <= 0)
                throw new ArgumentException($"Invalid producersCount: {producersCount}");

            _items = new ConcurrentQueue<T>();
            _currentItemsCount = 0;
            _maxItemsCount = maxItemsCount;
            _ableToEnqueueEvent = new ManualResetEventSlim(initialState: true);
            _ableToDequeueEvent = new ManualResetEventSlim(initialState: false);            
            _producersCount = producersCount;
            _completedProducersCount = 0;
            _addingCompletedEvent = new ManualResetEventSlim(initialState: false);
            _cancellationToken = cancellationToken;
        }

        public void Enqueue(T item)
        {
            while (true)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                if (_addingCompletedEvent.IsSet)
                    throw new InvalidOperationException("Unable to enqueue an item after adding was completed");

                int currentItemsCount = _items.Count;
                if (currentItemsCount < _maxItemsCount)
                {
                    int previousItemsCount = Interlocked.CompareExchange(ref _currentItemsCount, currentItemsCount + 1, currentItemsCount);
                    if (previousItemsCount == currentItemsCount)
                    {
                        _items.Enqueue(item);

                        if (previousItemsCount == 0)
                            _ableToDequeueEvent.Set();
                        else if (previousItemsCount == _maxItemsCount - 1)
                            _ableToEnqueueEvent.Reset();

                        return;
                    }
                }
                else
                {
                    WaitHandle.WaitAny(new[] { _ableToEnqueueEvent.WaitHandle, _addingCompletedEvent.WaitHandle, _cancellationToken.WaitHandle });
                }
            }
        }

        public bool TryDequeue(out T item)
        {
            while (true)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                if (!_items.IsEmpty)
                {
                    if (_items.TryDequeue(out item))
                    {
                        int newItemsCount = Interlocked.Decrement(ref _currentItemsCount);

                        if (newItemsCount == 0)
                            _ableToDequeueEvent.Reset();
                        else if (newItemsCount == _maxItemsCount - 1)
                            _ableToEnqueueEvent.Set();

                        return true;
                    }
                }
                else
                {                    
                    if (_addingCompletedEvent.IsSet && _items.IsEmpty)
                    {
                        item = default;
                        return false;
                    }
                    
                    WaitHandle.WaitAny(new[] { _ableToDequeueEvent.WaitHandle, _addingCompletedEvent.WaitHandle, _cancellationToken.WaitHandle });
                }
            }
        }

        public void CompleteProducerAdding()
        {
            int completedProducersCount = Interlocked.Increment(ref _completedProducersCount);
            if (completedProducersCount == _producersCount)
                _addingCompletedEvent.Set();
        }

        public void Dispose()
        {
            _ableToEnqueueEvent.Dispose();
            _ableToDequeueEvent.Dispose();
            _addingCompletedEvent.Dispose();
        }
    }
}
