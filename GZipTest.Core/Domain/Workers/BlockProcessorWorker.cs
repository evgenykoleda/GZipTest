using GZipTest.Core.Application;
using GZipTest.Core.DataStructures;
using GZipTest.Core.Logging;

namespace GZipTest.Core.Domain.Workers
{
    internal class BlockProcessorWorker : IWorker
    {
        private readonly IBlockProcessor _blockProcessor;
        private readonly ProducerConsumerQueue<DataBlock> _sourceQueue;
        private readonly ProducerConsumerQueue<DataBlock> _targetQueue;
        private readonly ApplicationLifecycle _lifecycle;
        private readonly ILogger _logger;
        private readonly Thread _workerThread;

        public BlockProcessorWorker(
            IBlockProcessor blockProcessor,
            ProducerConsumerQueue<DataBlock> sourceQueue,
            ProducerConsumerQueue<DataBlock> targetQueue,
            ApplicationLifecycle lifecycle,
            ILogger logger)
        {
            _blockProcessor = blockProcessor;
            _sourceQueue = sourceQueue;
            _targetQueue = targetQueue;
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
                _logger.Message($"Block processor started");

                while (_sourceQueue.TryDequeue(out DataBlock sourceBlock))
                {
                    _logger.Message($"Processing block {sourceBlock.Index} started");
                    DataBlock processedBlock = _blockProcessor.ProcessBlock(sourceBlock);
                    _logger.Message($"Processing block {sourceBlock.Index} completed");
                    
                    _targetQueue.Enqueue(processedBlock);
                }

                _targetQueue.CompleteProducerAdding();
                _logger.Message($"Block processor finished");
            }
            catch (Exception exception)
            {
                _logger.Exception(exception, "Block processor failed");
                _lifecycle.OnError(exception);
            }
        }
    }
}
