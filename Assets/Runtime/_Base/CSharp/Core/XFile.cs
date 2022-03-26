using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public class XFile
    {
        public static byte[] ReadBytesFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;
            FileStream stream = File.OpenRead(filePath);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            stream.Close();
            return data;
        }

        public static byte[] ReadBytesFile(string filePath, int length)
        {
            if (!File.Exists(filePath))
                return null;
            FileStream stream = File.OpenRead(filePath);
            length = length >= 0 ? length : 0;
            length = (int)Math.Min(stream.Length, length);
            byte[] data = new byte[length];
            stream.Read(data, 0, data.Length);
            stream.Close();
            return data;
        }

        public static void WriteBytesFile(string filePath, byte[] data, int offset, int count)
        {
            XCSharp.MakeDir(Path.GetDirectoryName(filePath));
            FileStream stream = File.OpenWrite(filePath);
            stream.Write(data, offset, count);
            stream.Flush();
            stream.Close();
        }

        public static string ReadTextFile(string filePath)
        {
            if (!File.Exists(filePath))
                return "";
            StreamReader reader = File.OpenText(filePath);
            string data = reader.ReadToEnd();
            reader.Close();
            return data;
        }

        public static void WriteTextFile(string filePath, string data)
        {
            XCSharp.MakeDir(Path.GetDirectoryName(filePath));
            TextWriter writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.Write(data);
            writer.Flush();
            writer.Close();
        }
    }
}
