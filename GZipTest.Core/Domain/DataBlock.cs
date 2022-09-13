namespace GZipTest.Core.Domain
{
    internal class DataBlock
    {
        public int Index { get; }
        public byte[] Data { get; }
        public int DataSize { get; }

        public DataBlock(int index, byte[] data, int dataSize)
        {
            Index = index;
            Data = data;
            DataSize = dataSize;
        }
    }
}
