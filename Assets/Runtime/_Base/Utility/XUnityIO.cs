using UnityEngine;
using System.Collections;
using System.IO;

namespace ProjectX
{
    public class UnityReader : BinaryReader
    {
        public UnityReader(Stream stream)
            : base(stream)
        { }

        public void Read(Transform value)
        {
            Vector3 p = Vector3.zero;
            Quaternion r = Quaternion.identity;
            Vector3 s = Vector3.one;
            this.Read(ref p);
            this.Read(ref r);
            this.Read(ref s);
            value.position = p;
            value.rotation = r;
            value.localScale = s;
        }

        #region Basic Reader with ref-params
        public void Read(ref sbyte value)
        {
            value = this.ReadSByte();
        }
        public void Read(ref byte value)
        {
            value = this.ReadByte();
        }
        public void Read(ref short value)
        {
            value = this.ReadInt16();
        }
        public void Read(ref ushort value)
        {
            value = this.ReadUInt16();
        }
        public void Read(ref int value)
        {
            value = this.ReadInt32();
        }
        public void Read(ref uint value)
        {
            value = this.ReadUInt32();
        }
        public void Read(ref long value)
        {
            value = this.ReadInt64();
        }
        public void Read(ref ulong value)
        {
            value = this.ReadUInt64();
        }
        public void Read(ref float value)
        {
            value = this.ReadSingle();
        }
        public void Read(ref double value)
        {
            value = this.ReadDouble();
        }
        #endregion

        #region Unity Reader with ref-params
        public void Read(ref Vector2 value)
        {
            this.Read(ref value.x);
            this.Read(ref value.y);
        }

        public void Read(ref Vector3 value)
        {
            this.Read(ref value.x);
            this.Read(ref value.y);
            this.Read(ref value.z);
        }

        public void Read(ref Vector4 value)
        {
            this.Read(ref value.x);
            this.Read(ref value.y);
            this.Read(ref value.z);
            this.Read(ref value.w);
        }

        public void Read(ref Quaternion value)
        {
            this.Read(ref value.x);
            this.Read(ref value.y);
            this.Read(ref value.z);
            this.Read(ref value.w);
        }

        public void Read(ref Color value)
        {
            this.Read(ref value.r);
            this.Read(ref value.g);
            this.Read(ref value.b);
            this.Read(ref value.a);
        }

        public void Read(ref Color32 value)
        {
            this.Read(ref value.r);
            this.Read(ref value.g);
            this.Read(ref value.b);
            this.Read(ref value.a);
        }
        #endregion

        #region Unity Reader with ret-value
        public Vector2 ReadVector2()
        {
            Vector2 value = Vector2.zero;
            this.Read(ref value);
            return value;
        }

        public Vector3 ReadVector3()
        {
            Vector3 value = Vector3.zero;
            this.Read(ref value);
            return value;
        }

        public Vector4 ReadVector4()
        {
            Vector4 value = Vector4.zero;
            this.Read(ref value);
            return value;
        }

        public Quaternion ReadQuaternion()
        {
            Quaternion value = Quaternion.identity;
            this.Read(ref value);
            return value;
        }

        public Color ReadColor()
        {
            Color value = Color.black;
            this.Read(ref value);
            return value;
        }

        public Color32 ReadColor32()
        {
            Color32 value = new Color32(0, 0, 0, 0);
            this.Read(ref value);
            return value;
        }
        #endregion
    }

    public class UnityWriter : BinaryWriter
    {
        public UnityWriter(Stream stream)
            : base(stream)
        { }

        #region Unity Writer
        public void Write(Vector2 value)
        {
            this.Write(value.x);
            this.Write(value.y);
        }

        public void Write(Vector3 value)
        {
            this.Write(value.x);
            this.Write(value.y);
            this.Write(value.z);
        }

        public void Write(Vector4 value)
        {
            this.Write(value.x);
            this.Write(value.y);
            this.Write(value.z);
            this.Write(value.w);
        }

        public void Write(Quaternion value)
        {
            this.Write(value.x);
            this.Write(value.y);
            this.Write(value.z);
            this.Write(value.w);
        }

        public void Write(Color value)
        {
            this.Write(value.r);
            this.Write(value.g);
            this.Write(value.b);
            this.Write(value.a);
        }

        public void Write(Color32 value)
        {
            this.Write(value.r);
            this.Write(value.g);
            this.Write(value.b);
            this.Write(value.a);
        }

        public void Write(Transform value)
        {
            this.Write(value.position);
            this.Write(value.rotation);
            this.Write(value.localScale);
        }
        #endregion
    }
}

