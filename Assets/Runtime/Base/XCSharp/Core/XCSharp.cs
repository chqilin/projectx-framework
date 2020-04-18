using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;

namespace ProjectX
{
    public class XCSharp
    {
        public class CommentAttribute : Attribute
        {
            public CommentAttribute(string content)
            { }
        }

        #region Delegate
        public static void InvokeAction(Action action)
        {
            if (action == null)
                return;
            action.Invoke();
        }
        public static void InvokeAction<A>(Action<A> action, A a = default(A))
        {
            if (action == null)
                return;
            action.Invoke(a);
        }
        public static void InvokeAction<A1, A2>(Action<A1, A2> action, A1 a1 = default(A1), A2 a2 = default(A2))
        {
            if (action == null)
                return;
            action.Invoke(a1, a2);
        }
        public static void InvokeAction<A1, A2, A3>(Action<A1, A2, A3> action, A1 a1 = default(A1), A2 a2 = default(A2), A3 a3 = default(A3))
        {
            if (action == null)
                return;
            action.Invoke(a1, a2, a3);
        }
        public static void InvokeAction<A1, A2, A3, A4>(Action<A1, A2, A3, A4> action, A1 a1 = default(A1), A2 a2 = default(A2), A3 a3 = default(A3), A4 a4 = default(A4))
        {
            if (action == null)
                return;
            action.Invoke(a1, a2, a3, a4);
        }
        #endregion

        #region Collections
        public static bool IsNullOrEmpty(string value)
        {
            return value == null || value.Length == 0;
        }

        public static bool IsNullOrEmpty(ICollection value)
        {
            return value == null || value.Count == 0;
        }
        #endregion

        #region File, Path & URI
        public static string PathToURI(string protocol, string path)
        {
            if (path.StartsWith(protocol))
                return path;
            return protocol + path;
        }

        public static string FileURI(string path)
        {
            return XCSharp.PathToURI("file://", path);
        }

        public static string JarFileURI(string path)
        {
            return XCSharp.PathToURI("jar:file://", path);
        }

        public static string HttpURI(string path)
        {
            return XCSharp.PathToURI("http://", path);
        }

        public static void MakeDir(string path)
        {
            if (Directory.Exists(path))
                return;
            Directory.CreateDirectory(path);
        }
        #endregion

        #region String Codec
        public static string EncodeUTF8(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            var sb = new StringBuilder();
            for (var i = 0; i < bytes.Length; ++i)
            {
                sb.Append((char)bytes[i]);
            }
            return sb.ToString();
        }

        public static string DecodeUTF8(string utf8)
        {
            var bytes = new byte[utf8.Length];
            for (int i = 0; i < utf8.Length; ++i)
            {
                bytes[i] = (byte)utf8[i];
            }
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
        #endregion

        #region Enum Functions
        public static T ParseEnum<T>(string enumStr)
        {
            return (T)Enum.Parse(typeof(T), enumStr);
        }

        public static string[] EnumNames<T>()
        {
            return Enum.GetNames(typeof(T));
        }

        public static Array EnumValues<T>()
        {
            return Enum.GetValues(typeof(T));
        }

        public static void TraverseEnum<T>(Action<T> onTraverse)
        {
            if (onTraverse == null)
                return;
            foreach (T enumValue in Enum.GetValues(typeof(T)))
            {
                onTraverse(enumValue);
            }
        }
        #endregion

        #region Serialize
        public static void Serialize(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            FieldInfo[] fields = value.GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                Type t = f.FieldType;
                object v = f.GetValue(value);
                switch (t.FullName)
                {
                    case "System.Boolean":
                        writer.Write((bool)v);
                        break;
                    case "System.Char":
                        writer.Write((char)v);
                        break;
                    case "System.Byte":
                        writer.Write((byte)v);
                        break;
                    case "System.SByte":
                        writer.Write((sbyte)v);
                        break;
                    case "System.Int16":
                        writer.Write((short)v);
                        break;
                    case "System.UInt16":
                        writer.Write((ushort)v);
                        break;
                    case "System.Int32":
                        writer.Write((int)v);
                        break;
                    case "System.UInt32":
                        writer.Write((uint)v);
                        break;
                    case "System.Int64":
                        writer.Write((long)v);
                        break;
                    case "System.UInt64":
                        writer.Write((ulong)v);
                        break;
                    case "System.Single":
                        writer.Write((float)v);
                        break;
                    case "System.Double":
                        writer.Write((double)v);
                        break;
                    case "System.Decimal":
                        writer.Write((decimal)v);
                        break;
                    case "System.String":
                        writer.Write((string)v);
                        break;
                    default:
                        XCSharp.Serialize(stream, v);
                        break;
                }
            }
        }

        public static object Deserialize(MemoryStream stream, Type type)
        {
            BinaryReader reader = new BinaryReader(stream);
            object o = Activator.CreateInstance(type);
            FieldInfo[] fields = o.GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                Type t = f.FieldType;
                object v = null;
                switch (t.FullName)
                {
                    case "System.Boolean":
                        v = reader.ReadBoolean();
                        break;
                    case "System.Char":
                        v = reader.ReadChar();
                        break;
                    case "System.Byte":
                        v = reader.ReadByte();
                        break;
                    case "System.SByte":
                        v = reader.ReadSByte();
                        break;
                    case "System.Int16":
                        v = reader.ReadInt16();
                        break;
                    case "System.UInt16":
                        v = reader.ReadUInt16();
                        break;
                    case "System.Int32":
                        v = reader.ReadInt32();
                        break;
                    case "System.UInt32":
                        v = reader.ReadUInt32();
                        break;
                    case "System.Int64":
                        v = reader.ReadInt64();
                        break;
                    case "System.UInt64":
                        v = reader.ReadUInt64();
                        break;
                    case "System.Single":
                        v = reader.ReadSingle();
                        break;
                    case "System.Double":
                        v = reader.ReadDouble();
                        break;
                    case "System.Decimal":
                        v = reader.ReadDecimal();
                        break;
                    case "System.String":
                        v = reader.ReadString();
                        break;
                    default:
                        v = XCSharp.Deserialize(stream, t);
                        break;
                }
                f.SetValue(o, v);
            }
            return o;
        }
        #endregion

        #region Encrypt & Decipher
        public static string EncodeBase64(string data)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return "";
            }
        }

        public static string DecodeBase64(string data)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(data);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "";
            }
        }

        public static void Encrypt(byte[] data, int offset, int count, byte key)
        {
            for (int pos = 0; pos < count; pos++)
            {
                int index = offset + pos;
                byte value = data[index];
                value = (byte)(((value & 0x0F) << 4) | ((value & 0xF0) >> 4));
                value ^= key;
                data[index] = value;
            }
        }

        public static void Decipher(byte[] data, int offset, int count, byte key)
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

        public static string MD5(string str, bool upper = false)
        {
            byte[] rawdata = System.Text.Encoding.UTF8.GetBytes(str.Trim());
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] crydata = md5.ComputeHash(rawdata);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string fmt = upper ? "X2" : "x2";
            for (int i = 0; i < crydata.Length; i++)
            {
                sb.Append(crydata[i].ToString(fmt));
            }
            return sb.ToString();
        }
        #endregion
    }
}

