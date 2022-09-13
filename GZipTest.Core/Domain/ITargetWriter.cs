namespace GZipTest.Core.Domain
{
    internal interface ITargetWriter
    {
        void WriteBlock(DataBlock block);
    }
}
