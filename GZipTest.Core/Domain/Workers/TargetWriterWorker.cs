using GZipTest.Core.Application;
using GZipTest.Core.DataStructures;
using GZipTest.Core.Logging;

namespace GZipTest.Core.Domain.Workers
{
    internal class TargetWriterWorker : IWorker
    {
        private readonly ITargetWriter _targetWriter;
        private readonly ProducerConsumerQueue<DataBlock> _targetQueue;
        private readonly ApplicationLifecycle _lifecycle;
        private readonly ILogger _logger;
        private readonly Thread _workerThread;

        public TargetWriterWorker(
            ITargetWriter targetWriter,
            ProducerConsumerQueue<DataBlock> targetQueue,
            ApplicationLifecycle lifecycle,
            ILogger logger)
        {
            _targetWriter = targetWriter;
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
                _logger.Message($"Target writer started");

                while (_targetQueue.TryDequeue(out DataBlock block))
                {
                    _logger.Message($"Writing block {block.Index} started");
                    _targetWriter.WriteBlock(block);
                    _logger.Message($"Writing block {block.Index} completed");
                }

                _logger.Message($"Target writer finished");
            }
            catch (Exception exception)
            {
                _logger.Exception(exception, $"Target writer failed");
                _lifecycle.OnError(exception);
            }
        }
    }
}
