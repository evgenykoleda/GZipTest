namespace GZipTest.Core.Domain.Workers
{
    internal interface IWorker
    {
        void Start();
        void Join();
    }
}
