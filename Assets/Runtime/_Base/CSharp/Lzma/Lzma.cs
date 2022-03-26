using System.IO;

namespace ProjectX
{
    public class Lzma
    {
        #region Compress & Decompress
        public static byte[] Compress(byte[] data)
        {
            MemoryStream instream = new MemoryStream(data);
            MemoryStream outstream = Lzma.Compress(instream);
            return outstream.ToArray();
        }

        public static byte[] Uncompress(byte[] data)
        {
            MemoryStream instream = new MemoryStream(data);
            MemoryStream outstream = Lzma.Uncompress(instream);
            return outstream.ToArray();
        }

        public static MemoryStream Compress(MemoryStream instream)
        {
            MemoryStream outstream = new MemoryStream();
            Lzma.Compress(instream, outstream);
            return outstream;
        }

        public static MemoryStream Uncompress(MemoryStream instream)
        {
            MemoryStream outstream = new MemoryStream();
            Lzma.Uncompress(instream, outstream);
            return outstream;
        }

        public static void Compress(Stream instream, Stream outstream)
        {
            SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
            encoder.WriteCoderProperties(outstream);

            BinaryWriter writer = new BinaryWriter(outstream);
            writer.Write(instream.Length);

            encoder.Code(instream, outstream, instream.Length, -1, null);
        }

        public static void Uncompress(Stream instream, Stream outstream)
        {
            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            byte[] properties = new byte[5];
            instream.Read(properties, 0, 5);
            decoder.SetDecoderProperties(properties);

            BinaryReader reader = new BinaryReader(instream);
            long outsize = reader.ReadInt64();

            decoder.Code(instream, outstream, instream.Length, outsize, null);
        }
        #endregion
    }
}
