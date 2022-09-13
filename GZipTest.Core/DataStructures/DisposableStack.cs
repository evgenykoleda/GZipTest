namespace GZipTest.Core.DataStructures
{
    internal class DisposableStack : IDisposable
    {
        private readonly Stack<IDisposable> _items;

        public DisposableStack()
        {
            _items = new Stack<IDisposable>();
        }

        public T Push<T>(T item)
            where T : IDisposable
        {
            _items.Push(item);
            return item;
        }

        public void Dispose()
        {
            while (_items.TryPop(out IDisposable item))
            {
                item.Dispose();
            }
        }
    }
}
