namespace GZipTest.Core.Application
{
    public class ApplicationSettings
    {
        public static ApplicationSettings CreateDefault(
            EApplicationMode applicationMode,
            string sourceFilePath,
            string targetFilePath)
        {
            return new ApplicationSettings(
                applicationMode,
                sourceFilePath,
                targetFilePath,
                blockSize: 1024 * 1024 * 1,
                processorsCount: Environment.ProcessorCount,
                queueMaxBlocksCount: 100);
        }


        public EApplicationMode ApplicationMode { get; }
        public string SourceFilePath { get; }
        public string TargetFilePath { get; }
        public int BlockSize { get; }
        public int ProcessorsCount { get; }
        public int QueueMaxBlocksCount { get; }

        public ApplicationSettings(
            EApplicationMode applicationMode,
            string sourceFilePath,
            string targetFilePath,
            int blockSize,
            int processorsCount,
            int queueMaxBlocksCount)
        {
            ApplicationMode = applicationMode;
            SourceFilePath = sourceFilePath;
            TargetFilePath = targetFilePath;
            BlockSize = blockSize;
            ProcessorsCount = processorsCount;
            QueueMaxBlocksCount = queueMaxBlocksCount;
        }

        public enum EApplicationMode
        {
            Compress,
            Decompress
        }
    }
}
