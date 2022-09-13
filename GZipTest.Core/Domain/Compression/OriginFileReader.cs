
namespace GZipTest.Core.Domain.Compression
{
    internal class OriginFileReader : ISourceReader, IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly int _blockSize;
        private int _currentBlockIndex;

        public OriginFileReader(string filePath, int blockSize)
        {
            _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            _blockSize = blockSize;
            _currentBlockIndex = 0;
        }

        public bool TryReadBlock(out DataBlock block)
        {
            byte[] data = new byte[_blockSize];
            int readDataSize = _fileStream.Read(data);
            if (readDataSize > 0)
            {
                int blockIndex = _currentBlockIndex++;
                block = new DataBlock(blockIndex, data, readDataSize);
                return true;
            }
            else
            {
                block = default;
                return false;
            }
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }
    }
}
