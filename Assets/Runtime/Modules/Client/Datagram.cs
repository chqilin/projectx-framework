using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ProjectX
{
    public class DatagramWriter
    {
        private BinaryWriter mWriter;

        #region Constructor
        public DatagramWriter(Stream outputStream)
        {
            this.mWriter = new BinaryWriter(outputStream);
        }
        #endregion

        #region Public Methods
        public void Flush()
        {
            this.mWriter.Flush();
        }
        #endregion

        #region Writing Methods
        public void Write(byte[] data)
        {
            this.mWriter.Write(data, 0, data.Length);
        }
        public void Write(byte data)
        {
            this.mWriter.Write(data);
        }
        public void Write(sbyte data)
        {
            this.mWriter.Write(data);
        }
        public void Write(short data)
        {
            this.mWriter.Write(IPAddress.HostToNetworkOrder(data));
        }
        public void Write(ushort data)
        {
            this.mWriter.Write(IPAddress.HostToNetworkOrder((short)data));
        }
        public void Write(int data)
        {
            this.mWriter.Write(IPAddress.HostToNetworkOrder(data));
        }
        public void Write(uint data)
        {
            this.mWriter.Write(IPAddress.HostToNetworkOrder((int)data));
        }
        public void Write(long data)
        {
            this.mWriter.Write(IPAddress.HostToNetworkOrder(data));
        }
        public void Write(ulong data)
        {
            this.mWriter.Write(IPAddress.HostToNetworkOrder((long)data));
        }
        public void Write(float data)
        {
            this.Write(data.ToString());
        }
        public void Write(double data)
        {
            this.Write(data.ToString());
        }
        public void Write(bool data)
        {
            byte val = (byte)(data ? 1 : 0);
            this.Write(val);
        }
        public void Write(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                ushort count = (ushort)bytes.Length;
                this.Write(count);
                this.mWriter.Write(bytes, 0, count);
            }
            else
            {
                ushort count = 0;
                this.Write(count);
            }
        }
        public void Write(int[] list)
        {
            if (list != null && list.Length > 0)
            {
                ushort count = (ushort)list.Length;
                this.Write(count);
                foreach (var val in list)
                {
                    this.Write(val);
                }
            }
            else
            {
                ushort count = 0;
                this.Write(count);
            }
        }
        public void Write(List<int> list)
        {
            if (list != null && list.Count > 0)
            {
                ushort count = (ushort)list.Count;
                this.Write(count);
                foreach (var val in list)
                {
                    this.Write(val);
                }
            }
            else
            {
                ushort count = 0;
                this.Write(count);
            }
        }
        public void Write(string[] list)
        {
            if (list != null && list.Length > 0)
            {
                ushort count = (ushort)list.Length;
                this.Write(count);
                foreach (var val in list)
                {
                    this.Write(val);
                }
            }
            else
            {
                ushort count = 0;
                this.Write(count);
            }
        }
        public void Write(List<string> list)
        {
            if (list != null && list.Count > 0)
            {
                ushort count = (ushort)list.Count;
                this.Write(count);
                foreach (var val in list)
                {
                    this.Write(val);
                }
            }
            else
            {
                ushort count = 0;
                this.Write(count);
            }
        }
        public void Write<T>(T[] list) where T : DatagramMessage
        {
            if (list != null && list.Length > 0)
            {
                ushort count = (ushort)list.Length;
                this.Write(count);
                foreach (var m in list)
                {
                    m.Serialize(this);
                }
            }
            else
            {
                ushort count = 0;
                this.Write(count);
            }
        }
        public void Write<T>(List<T> list) where T : DatagramMessage
        {
            if (list != null && list.Count > 0)
            {
                ushort count = (ushort)list.Count;
                this.Write(count);
                foreach (var m in list)
                {
                    m.Serialize(this);
                }
            }
            else
            {
                ushort count = 0;
                this.Write(count);
            }
        }
        #endregion
    }

    public class DatagramReader
    {
        private BinaryReader mReader;

        #region Constructor
        public DatagramReader(Stream inputStream)
        {
            this.mReader = new BinaryReader(inputStream);
        }
        #endregion

        #region Public Methods
        public int Read(byte[] data, int offset, int count)
        {
            return this.mReader.Read(data, offset, count);
        }
        #endregion

        #region Read Methods with return-value
        public byte[] ReadBytes(int count)
        {
            return this.mReader.ReadBytes(count);
        }
        public byte ReadByte()
        {
            return this.mReader.ReadByte();
        }
        public sbyte ReadSByte()
        {
            return this.mReader.ReadSByte();
        }
        public short ReadShort()
        {
            return IPAddress.NetworkToHostOrder(this.mReader.ReadInt16());
        }
        public ushort ReadUShort()
        {
            return (ushort)(IPAddress.NetworkToHostOrder(this.mReader.ReadInt16()));
        }
        public int ReadInt()
        {
            return IPAddress.NetworkToHostOrder(this.mReader.ReadInt32());
        }
        public uint ReadUInt()
        {
            return (uint)(IPAddress.NetworkToHostOrder(this.mReader.ReadInt32()));
        }
        public long ReadLong()
        {
            return IPAddress.NetworkToHostOrder(this.mReader.ReadInt64());
        }
        public ulong ReadULong()
        {
            return (ulong)(IPAddress.NetworkToHostOrder(this.mReader.ReadInt64()));
        }
        public float ReadFloat()
        {
            string str = this.ReadString();
            return float.Parse(str);
        }
        public double ReadDouble()
        {
            string str = this.ReadString();
            return double.Parse(str);
        }
        public bool ReadBool()
        {
            byte val = this.ReadByte();
            return val != 0;
        }
        public string ReadString()
        {
            ushort count = this.ReadUShort();
            if (count == 0)
                return "";
            byte[] bytes = this.mReader.ReadBytes(count);
            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
        public int[] ReadIntArray()
        {
            ushort count = this.ReadUShort();
            int[] list = new int[count];
            for (ushort i = 0; i < count; i++)
            {
                list[i] = this.ReadInt();
            }
            return list;
        }
        public List<int> ReadIntList()
        {
            ushort count = this.ReadUShort();
            List<int> list = new List<int>();
            for (ushort i = 0; i < count; i++)
            {
                int val = this.ReadInt();
                list.Add(val);
            }
            return list;
        }
        public string[] ReadStringArray()
        {
            ushort count = this.ReadUShort();
            string[] list = new string[count];
            for (ushort i = 0; i < count; i++)
            {
                list[i] = this.ReadString();
            }
            return list;
        }
        public List<string> ReadStringList()
        {
            ushort count = this.ReadUShort();
            List<string> list = new List<string>();
            for (ushort i = 0; i < count; i++)
            {
                string val = this.ReadString();
                list.Add(val);
            }
            return list;
        }
        public T[] ReadDatagramArray<T>() where T : DatagramMessage, new()
        {
            ushort count = this.ReadUShort();
            T[] list = new T[count];
            for (ushort i = 0; i < count; i++)
            {
                T m = new T();
                m.Deserialize(this);
                list[i] = m;
            }
            return list;
        }

        public List<T> ReadDatagramList<T>() where T : DatagramMessage, new()
        {
            ushort count = this.ReadUShort();
            List<T> list = new List<T>();
            for (ushort i = 0; i < count; i++)
            {
                T m = new T();
                m.Deserialize(this);
                list.Add(m);
            }
            return list;
        }
        #endregion

        #region Read Methods with out-param
        public void Read(out byte data)
        {
            data = this.ReadByte();
        }
        public void Read(out sbyte data)
        {
            data = this.ReadSByte();
        }
        public void Read(out short data)
        {
            data = this.ReadShort();
        }
        public void Read(out ushort data)
        {
            data = this.ReadUShort();
        }
        public void Read(out int data)
        {
            data = this.ReadInt();
        }
        public void Read(out uint data)
        {
            data = this.ReadUInt();
        }
        public void Read(out long data)
        {
            data = this.ReadLong();
        }
        public void Read(out ulong data)
        {
            data = this.ReadULong();
        }
        public void Read(out float data)
        {
            data = this.ReadFloat();
        }
        public void Read(out double data)
        {
            data = this.ReadDouble();
        }
        public void Read(out bool data)
        {
            data = this.ReadBool();
        }
        public void Read(out string data)
        {
            data = this.ReadString();
        }
        public void Read(out int[] list)
        {
            list = this.ReadIntArray();
        }
        public void Read(out List<int> list)
        {
            list = this.ReadIntList();
        }
        public void Read(out string[] list)
        {
            list = this.ReadStringArray();
        }
        public void Read(out List<string> list)
        {
            list = this.ReadStringList();
        }
        public void Read<T>(out T[] list) where T : DatagramMessage, new()
        {
            list = this.ReadDatagramArray<T>();
        }
        public void Read<T>(out List<T> list) where T : DatagramMessage, new()
        {
            list = this.ReadDatagramList<T>();
        }
        #endregion
    }

    public interface DatagramMessage
    {
        void Serialize(DatagramWriter w);
        void Deserialize(DatagramReader r);
    }

    public class DatagramManager
    {
        // message type--id mapping
        // in order to maximize query-efficiency, we use two dictionary.
        private Dictionary<System.Type, ushort> mMessagesTypeId = new Dictionary<System.Type, ushort>();
        private Dictionary<ushort, System.Type> mMessagesIdType = new Dictionary<ushort, System.Type>();

        #region Public Methods
        public void RegisterMessage(System.Type type, ushort dmid)
        {
            if (this.mMessagesTypeId.ContainsKey(type) || this.mMessagesIdType.ContainsKey(dmid))
            {
                throw new Exception("A same message has registered. type=" + type.Name + ", dmid=" + dmid);
            }
            this.mMessagesTypeId[type] = dmid;
            this.mMessagesIdType[dmid] = type;
        }
        public void UnregisterMessage(System.Type type)
        {
            ushort dmid = 0;
            if (this.mMessagesTypeId.TryGetValue(type, out dmid))
            {
                this.mMessagesTypeId.Remove(type);
                this.mMessagesIdType.Remove(dmid);
            }
        }
        public void ClearMessages()
        {
            mMessagesTypeId.Clear();
            mMessagesIdType.Clear();
        }

        public ushort SelectMessageId(System.Type type)
        {
            ushort dmid = 0;
            this.mMessagesTypeId.TryGetValue(type, out dmid);
            return dmid;
        }
        public System.Type SelectMessageType(ushort dmid)
        {
            System.Type type = null;
            this.mMessagesIdType.TryGetValue(dmid, out type);
            return type;
        }

        // serialize datagram-message to byte-array .
        public byte[] SerializeMessage(DatagramMessage message)
        {
            MemoryStream m = new MemoryStream();
            DatagramWriter w = new DatagramWriter(m);
            message.Serialize(w);
            w.Flush();
            byte[] data = m.ToArray();
            return data;
        }

        // deserialize a datagram-message via a specific type and byte-array data
        public DatagramMessage DeserializeMessage(Type type, byte[] data)
        {
            DatagramMessage message = Activator.CreateInstance(type) as DatagramMessage;
            if (message == null)
                return null;
            MemoryStream m = new MemoryStream(data);
            DatagramReader r = new DatagramReader(m);
            message.Deserialize(r);
            return message;
        }

        public byte[] AnalyzeMessage(MemoryStream data)
        {
            if (data.Length - data.Position < 4)
                return null;

            long origin = data.Position;
            DatagramReader r = new DatagramReader(data);

            ushort dmid = r.ReadUShort();
            Type type = this.SelectMessageType(dmid);
            if (type == null)
            {
                // clear stream
                data.SetLength(0);
                data.Position = 0;
                return null;
            }

            ushort size = r.ReadUShort();
            if (data.Length - data.Position < size)
            {
                // roll back
                data.Position = origin;
                return null;
            }

            byte[] bytes = new byte[4 + size];
            data.Position = origin;
            if (data.Read(bytes, 0, bytes.Length) < bytes.Length)
            {
                // roll back
                data.Position = origin;
                return null;
            }

            return bytes;
        }

        // serialize datagram-message to byte-array with meta-data .
        public byte[] EnpackMessage(DatagramMessage message)
        {
            byte[] data = this.SerializeMessage(message);
            ushort dgid = (ushort)this.SelectMessageId(message.GetType());
            ushort size = (ushort)data.Length;

            MemoryStream m = new MemoryStream();
            DatagramWriter w = new DatagramWriter(m);
            w.Write(dgid);
            w.Write(size);
            w.Write(data);
            w.Flush();
            return m.ToArray();
        }

        // deserialize datagram-message via data which contains 4-byte meta-data
        public DatagramMessage DepackMessage(byte[] data)
        {
            if (data == null || data.Length < 4)
            {
                throw new Exception("Message data is null or data.Length is too small.");
            }

            MemoryStream m = new MemoryStream(data);
            DatagramReader r = new DatagramReader(m);
            ushort dmid = r.ReadUShort();
            ushort size = r.ReadUShort();
            byte[] bytes = r.ReadBytes(size);

            Type type = this.SelectMessageType(dmid);
            if (type == null)
            {
                throw new Exception("The type of message is not found for dmid=" + dmid);
            }
            DatagramMessage message = this.DeserializeMessage(type, bytes);
            return message;
        }
        #endregion
    }
}
