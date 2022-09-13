
namespace GZipTest.Core.Domain.Compression
{
    internal class CompressedFileWriter : ITargetWriter, IDisposable
    {
        private readonly FileStream _fileStream;

        public CompressedFileWriter(string filePath)
        {
            _fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
        }

        public void WriteTotalBlocksCount(int blocksCount)
        {
            _fileStream.Write(BitConverter.GetBytes(blocksCount));
        }

        public void WriteBlock(DataBlock block)
        {
            _fileStream.Write(BitConverter.GetBytes(block.Index));
            _fileStream.Write(BitConverter.GetBytes(block.DataSize));
            _fileStream.Write(block.Data, 0, block.DataSize);
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }
    }
}
