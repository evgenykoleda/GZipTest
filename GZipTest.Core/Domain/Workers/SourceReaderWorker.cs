using GZipTest.Core.Application;
using GZipTest.Core.DataStructures;
using GZipTest.Core.Logging;

namespace GZipTest.Core.Domain.Workers
{
    internal class SourceReaderWorker : IWorker
    {
        private readonly ISourceReader _sourceReader;
        private readonly ProducerConsumerQueue<DataBlock> _sourceQueue;
        private readonly ApplicationLifecycle _lifecycle;
        private readonly ILogger _logger;
        private readonly Thread _workerThread;

        public SourceReaderWorker(
            ISourceReader sourceReader,
            ProducerConsumerQueue<DataBlock> sourceQueue,
            ApplicationLifecycle lifecycle,
            ILogger logger)
        {
            _sourceReader = sourceReader;
            _sourceQueue = sourceQueue;
            _lifecycle = lifecycle;
            _logger = logger;
            _workerThread = new Thread(Work);
        }

        public void Start()
        {
            _workerThread.Start();
        }

        public void Join()
        {
            _workerThread.Join();
        }

        private void Work()
        {
            try
            {
                _logger.Message($"Source reader started"); 

                while (_sourceReader.TryReadBlock(out DataBlock block))
                {
                    _logger.Message($"Reading block {block.Index} completed");
                    _sourceQueue.Enqueue(block);
                }
                
                _sourceQueue.CompleteProducerAdding();
                _logger.Message($"Source reader finished");
            }
            catch (Exception exception)
            {
                _logger.Exception(exception, "Source reader failed");
                _lifecycle.OnError(exception);
            }
        }
    }
}
