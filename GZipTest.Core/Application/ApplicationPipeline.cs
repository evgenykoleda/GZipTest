using GZipTest.Core.DataStructures;
using GZipTest.Core.Domain;
using GZipTest.Core.Domain.Compression;
using GZipTest.Core.Domain.Decompression;
using GZipTest.Core.Domain.Workers;
using GZipTest.Core.Logging;

namespace GZipTest.Core.Application
{
    public sealed class ApplicationPipeline : IDisposable
    {
        public static ApplicationPipeline CreateApplication(ApplicationSettings settings, ILogger logger)
        {
            switch (settings.ApplicationMode)
            {
                case ApplicationSettings.EApplicationMode.Compress:
                    return CreateCompressApplication(settings, logger);
                case ApplicationSettings.EApplicationMode.Decompress:
                    return CreateDecompressApplication(settings, logger);
                default:
                    throw new Exception($"Unknown application mode: '{settings.ApplicationMode}'");
            }
        }

        private static ApplicationPipeline CreateCompressApplication(ApplicationSettings settings, ILogger logger)
        {
            DisposableStack disposables = new DisposableStack();
            try
            {
                ApplicationLifecycle lifecycle = disposables.Push(new ApplicationLifecycle());

                ProducerConsumerQueue<DataBlock> sourceQueue = disposables.Push(new ProducerConsumerQueue<DataBlock>(settings.QueueMaxBlocksCount, producersCount: 1, lifecycle.CancellationToken));
                ProducerConsumerQueue<DataBlock> targetQueue = disposables.Push(new ProducerConsumerQueue<DataBlock>(settings.QueueMaxBlocksCount, settings.ProcessorsCount, lifecycle.CancellationToken));

                OriginFileReader fileReader = disposables.Push(new OriginFileReader(settings.SourceFilePath, settings.BlockSize));
                OriginBlockProcessor[] blockProcessors = new OriginBlockProcessor[settings.ProcessorsCount];
                for (int i = 0; i < settings.ProcessorsCount; i++)
                    blockProcessors[i] = new OriginBlockProcessor();
                CompressedFileWriter fileWriter = disposables.Push(new CompressedFileWriter(settings.TargetFilePath));

                return new ApplicationPipeline(disposables, lifecycle, logger, sourceQueue, targetQueue, fileReader, blockProcessors, fileWriter);
            }
            catch (Exception)
            {
                disposables.Dispose();
                throw;
            }
        }

        private static ApplicationPipeline CreateDecompressApplication(ApplicationSettings settings, ILogger logger)
        {
            DisposableStack disposables = new DisposableStack();
            try
            {
                ApplicationLifecycle lifecycle = disposables.Push(new ApplicationLifecycle());

                ProducerConsumerQueue<DataBlock> sourceQueue = disposables.Push(new ProducerConsumerQueue<DataBlock>(settings.QueueMaxBlocksCount, producersCount: 1, lifecycle.CancellationToken));
                ProducerConsumerQueue<DataBlock> targetQueue = disposables.Push(new ProducerConsumerQueue<DataBlock>(settings.QueueMaxBlocksCount, settings.ProcessorsCount, lifecycle.CancellationToken));

                CompressedFileReader fileReader = disposables.Push(new CompressedFileReader(settings.SourceFilePath));
                CompressedBlockProcessor[] blockProcessors = new CompressedBlockProcessor[settings.ProcessorsCount];
                for (int i = 0; i < settings.ProcessorsCount; i++)
                    blockProcessors[i] = new CompressedBlockProcessor();
                OriginFileWriter fileWriter = disposables.Push(new OriginFileWriter(settings.TargetFilePath));

                return new ApplicationPipeline(disposables, lifecycle, logger, sourceQueue, targetQueue, fileReader, blockProcessors, fileWriter);
            }
            catch (Exception)
            {
                disposables.Dispose();
                throw;
            }
        }

        private readonly DisposableStack _disposables;
        private readonly ApplicationLifecycle _lifecycle;
        private readonly IWorker _sourceFileWorker;
        private readonly IWorker[] _blockProcessorsWorkers;
        private readonly IWorker _targetFileWorker;

        private ApplicationPipeline(
            DisposableStack disposables,
            ApplicationLifecycle lifecycle,
            ILogger logger,
            ProducerConsumerQueue<DataBlock> sourceQueue,
            ProducerConsumerQueue<DataBlock> targetQueue,
            ISourceReader sourceReader,
            IBlockProcessor[] blockProcessors,
            ITargetWriter targetWriter)
        {
            try
            {
                _disposables = disposables;
                _lifecycle = lifecycle;
                _sourceFileWorker = new SourceReaderWorker(sourceReader, sourceQueue, lifecycle, logger);
                _blockProcessorsWorkers = new IWorker[blockProcessors.Length];
                for (int i = 0; i < blockProcessors.Length; i++)
                    _blockProcessorsWorkers[i] = new BlockProcessorWorker(blockProcessors[i], sourceQueue, targetQueue, lifecycle, logger);
                _targetFileWorker = new TargetWriterWorker(targetWriter, targetQueue, lifecycle, logger);
            }
            catch (Exception)
            {
                disposables.Dispose();
                throw;
            }
        }

        public void Start()
        {
            _sourceFileWorker.Start();
            foreach (IWorker processorWorker in _blockProcessorsWorkers)
                processorWorker.Start();
            _targetFileWorker.Start();
        }

        public void Wait()
        {
            _sourceFileWorker.Join();
            foreach (IWorker processorWorker in _blockProcessorsWorkers)
                processorWorker.Join();
            _targetFileWorker.Join();

            Exception[] errors = _lifecycle.GetErrors();
            if (errors.Length > 0)
                throw new AggregateException(errors);

            _lifecycle.CancellationToken.ThrowIfCancellationRequested();
        }

        public void Cancel()
        {
            _lifecycle.Cancel();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
