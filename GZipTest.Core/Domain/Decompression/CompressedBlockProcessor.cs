using System.IO.Compression;

namespace GZipTest.Core.Domain.Decompression
{
    internal class CompressedBlockProcessor : IBlockProcessor
    {
        public DataBlock ProcessBlock(DataBlock compressedBlock)
        {
            using (MemoryStream compressedStream = new MemoryStream(compressedBlock.Data, 0, compressedBlock.DataSize))
            using (GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (MemoryStream decompressedStream  = new MemoryStream())
            {
                zipStream.CopyTo(decompressedStream);
                byte[] originData = decompressedStream.ToArray();
                return new DataBlock(compressedBlock.Index, originData, originData.Length);
            }
        }
    }
}
