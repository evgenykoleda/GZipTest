namespace GZipTest.Core.Domain
{
    internal interface IBlockProcessor
    {
        DataBlock ProcessBlock(DataBlock sourceBlock);
    }
}
