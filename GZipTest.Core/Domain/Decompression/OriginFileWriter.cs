namespace GZipTest.Core.Domain.Decompression
{
    internal class OriginFileWriter : ITargetWriter, IDisposable
    {
        private int _currentBlockIndex;
        private readonly Dictionary<int, DataBlock> _pendingBlocks;
        private readonly FileStream _fileStream;

        public OriginFileWriter(string filePath)
        {
            _currentBlockIndex = 0;
            _pendingBlocks = new Dictionary<int, DataBlock>();
            _fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
        }

        public void WriteBlock(DataBlock block)
        {
            _pendingBlocks.Add(block.Index, block);

            while (_pendingBlocks.TryGetValue(_currentBlockIndex, out DataBlock currentBlock))
            {
                _fileStream.Write(currentBlock.Data, 0, currentBlock.DataSize);
                _pendingBlocks.Remove(_currentBlockIndex);
                _currentBlockIndex++;
            }
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }
    }
}
