using System.IO;

namespace ProjectX
{
    public class Autarky
    {
        public struct Signature
        {
            public int version;
            public bool enableEncrypt;
            public byte encryptKey;
        }

        public const byte AutarkyVersion = 3;
        public const int SignatureLength = 32;

        private static string msError = "";
        public static string error
        {
            get { return msError; }
        }

        public static bool DecodeSignature(string filePath, out Signature signature)
        {
            signature = default(Signature);

            if (!File.Exists(filePath))
                return Error("file not found.");

            FileStream stream = File.OpenRead(filePath);
            if (stream.Length < SignatureLength)
            {
                stream.Close();
                return Error("file is too small.");
            }

            byte[] data = new byte[SignatureLength];
            stream.Seek(-SignatureLength, SeekOrigin.End);
            stream.Read(data, 0, SignatureLength);
            stream.Close();

            return DecodeSignature(data, out signature);
        }

        public static bool DecodeSignature(byte[] data, out Signature signature)
        {
            signature = default(Signature);
            if (data == null || data.Length < SignatureLength)
                return Error("data is null or too small.");

            int pos = data.Length - SignatureLength;
            if (data[pos + 0] != 'a' || data[pos + 1] != 't' || data[pos + 2] != 'k' || data[pos + 3] != AutarkyVersion)
                return Error("data format is invalid.");

            pos += 4;
            signature.version = data[pos] << 8 | data[pos + 1];

            pos += 2;
            signature.enableEncrypt = data[pos] == 1;
            signature.encryptKey = data[pos + 1];

            return true;
        }

        public static bool Load(ref byte[] data, out Signature sig)
        {
            if (!DecodeSignature(data, out sig))
                return false;
            if (sig.enableEncrypt)
            {
                Decipher(data, 0, data.Length - SignatureLength, sig.encryptKey);
            }
            return true;
        }

        private static void Decipher(byte[] data, int offset, int count, byte key)
        {
            for (int pos = 0; pos < count; pos++)
            {
                int index = offset + pos;
                byte value = data[index];
                value ^= key;
                value = (byte)(((value & 0x0F) << 4) | ((value & 0xF0) >> 4));
                data[index] = value;
            }
        }

        private static bool Error(string message)
        {
            msError = message;
            return false;
        }
    }
}

