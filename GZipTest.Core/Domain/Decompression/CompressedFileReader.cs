namespace GZipTest.Core.Domain.Decompression
{
    internal class CompressedFileReader : ISourceReader, IDisposable
    {
        private readonly FileStream _fileStream;

        public CompressedFileReader(string filePath)
        {
            _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public bool TryReadBlock(out DataBlock block)
        {
            if (_fileStream.Position < _fileStream.Length)
            {
                int index = ReadIntInternal();
                int dataSize = ReadIntInternal();
                byte[] data = new byte[dataSize];
                int readDataSize = _fileStream.Read(data);
                if (readDataSize < dataSize)
                    throw new Exception($"Failed to read compressed block {index}. Expected block size: {dataSize}, actual block size: {readDataSize}");

                block = new DataBlock(index, data, dataSize);
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

        private int ReadIntInternal()
        {
            int size = sizeof(int);
            byte[] buffer = new byte[size];
            int readSize = _fileStream.Read(buffer, 0, size);
            if (readSize < size)
                throw new Exception($"Expected to read {size} bytes, actual read {readSize} bytes. Position: {_fileStream.Position}");

            int value = BitConverter.ToInt32(buffer);
            return value;
        }
    }
}
