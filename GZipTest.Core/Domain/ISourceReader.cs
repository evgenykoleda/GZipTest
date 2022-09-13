namespace GZipTest.Core.Domain
{
    internal interface ISourceReader
    {
        bool TryReadBlock(out DataBlock block);
    }
}
