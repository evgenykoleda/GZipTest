using System.IO.Compression;

namespace GZipTest.Core.Domain.Compression
{
    internal class OriginBlockProcessor : IBlockProcessor
    {
        public DataBlock ProcessBlock(DataBlock originBlock)
        {
            using (MemoryStream compressedStream = new MemoryStream())
            using (GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            using (MemoryStream decompressedStream = new MemoryStream(originBlock.Data, 0, originBlock.DataSize))
            {
                decompressedStream.CopyTo(zipStream);
                zipStream.Flush();
                byte[] compressedData = compressedStream.ToArray();
                return new DataBlock(originBlock.Index, compressedData, compressedData.Length);
            }            
        }
    }
}
