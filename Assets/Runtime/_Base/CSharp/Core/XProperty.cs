using System;
using System.Collections.Generic;
using System.Reflection;

namespace ProjectX
{
    public struct XPropertyValue
    {
        private byte[] mData;

        public XPropertyValue(float value)
        {
            this = value;
        }

        public static implicit operator float(XPropertyValue value)
        {
            return XPropertyValue.Decode(value.mData);
        }

        public static implicit operator XPropertyValue(float value)
        {
            XPropertyValue result = new XPropertyValue();
            result.mData = XPropertyValue.Encode(value);
            return result;
        }

        public static XPropertyValue operator +(XPropertyValue a, XPropertyValue b)
        {
            XPropertyValue result = new XPropertyValue();
            result.mData = XPropertyValue.Add(a.mData, b.mData);
            return result;
        }

        public static XPropertyValue operator -(XPropertyValue a, XPropertyValue b)
        {
            XPropertyValue result = new XPropertyValue();
            result.mData = XPropertyValue.Sub(a.mData, b.mData);
            return result;
        }

        public static XPropertyValue operator *(XPropertyValue a, XPropertyValue b)
        {
            XPropertyValue result = new XPropertyValue();
            result.mData = XPropertyValue.Mul(a.mData, b.mData);
            return result;
        }

        public static byte[] Add(byte[] aData, byte[] bData)
        {
            float aValue = XPropertyValue.Decode(aData);
            float bValue = XPropertyValue.Decode(bData);
            return XPropertyValue.Encode(aValue + bValue);
        }

        public static byte[] Sub(byte[] aData, byte[] bData)
        {
            float aValue = XPropertyValue.Decode(aData);
            float bValue = XPropertyValue.Decode(bData);
            return XPropertyValue.Encode(aValue - bValue);
        }

        public static byte[] Mul(byte[] aData, byte[] bData)
        {
            float aValue = XPropertyValue.Decode(aData);
            float bValue = XPropertyValue.Decode(bData);
            return XPropertyValue.Encode(aValue * bValue);
        }

        public static byte[] Encode(float value)
        {
            byte[] data = BitConverter.GetBytes(value);
            XCSharp.Encrypt(data, 0, 3, data[3]);
            return data;
        }

        public static float Decode(byte[] data)
        {
            byte[] temp = new byte[data.Length];
            Array.Copy(data, temp, data.Length);
            XCSharp.Decipher(temp, 0, 3, temp[3]);
            float value = BitConverter.ToSingle(temp, 0);
            return value;
        }
    }

    public class XPropertyIncrement
    {
        public enum AffectMode
        {
            Value,
            OriginalPercentage,
            AffectedPercentage
        }

        private AffectMode mMode = AffectMode.Value;
        private float mValue = 0.0f;
        private object mState = null;

        public XPropertyIncrement(AffectMode mode, float value, object state)
        {
            this.mMode = mode;
            this.mValue = value;
            this.mState = state;
        }

        public AffectMode mode
        {
            get { return this.mMode; }
        }

        public float value
        {
            get { return this.mValue; }
        }

        public object state
        {
            get { return this.mState; }
        }
    }

    public class XPropertyObject
    {
        public delegate float PropertyModifier(float value);

        private string mName = "";
        private float mOriginalValue = 0;
        private float mAffectedValue = 0;
        private float mInstanceValue = 0;

        private List<XPropertyIncrement> mIncrements = new List<XPropertyIncrement>();
        private PropertyModifier mModifier = null;

        private List<XPropertyIncrement> mAffectedPercentIncrements = new List<XPropertyIncrement>();

        public XPropertyObject(string name)
        {
            this.mName = name;
        }

        #region Properties
        public string name
        {
            get { return this.mName; }
        }

        public float originalValue
        {
            get { return this.mOriginalValue; }
            set
            {
                this.mOriginalValue = value;
                this.RefreshValues();
            }
        }

        public PropertyModifier modifier
        {
            get { return this.mModifier; }
            set { this.mModifier = value; }
        }

        public float affectedValue
        {
            get { return this.mAffectedValue; }
        }

        public float currentValue
        {
            get
            {
                float result = this.affectedValue;
                if (this.mModifier != null)
                {
                    result = this.mModifier(result);
                }
                return result;
            }
        }

        public float instanceValue
        {
            get
            {
                float curValue = this.currentValue;
                float insValue = this.mInstanceValue;
                if (insValue > curValue)
                {
                    insValue = curValue;
                    this.mInstanceValue = curValue;
                }
                return insValue;
            }
            set
            {
                float curValue = this.currentValue;
                float insValue = value;
                insValue = insValue < curValue ? insValue : curValue;
                this.mInstanceValue = insValue;
            }
        }

        public float incrementValue
        {
            get { return this.mAffectedValue - this.mOriginalValue; }
        }

        public int positiveIncrementCount
        {
            get
            {
                int count = 0;
                foreach (XPropertyIncrement increment in this.mIncrements)
                {
                    if (increment.value > 0)
                        count += 1;
                }
                return count;
            }
        }

        public int negativeIncrementCount
        {
            get
            {
                int count = 0;
                foreach (XPropertyIncrement increment in this.mIncrements)
                {
                    if (increment.value < 0)
                        count += 1;
                }
                return count;
            }
        }
        #endregion

        #region Public Methods
        public void AddIncrement(XPropertyIncrement increment)
        {
            this.mIncrements.Add(increment);
            this.RefreshValues();
        }

        public void RemoveIncrement(XPropertyIncrement increment)
        {
            this.mIncrements.Remove(increment);
            this.RefreshValues();
        }

        public void RemoveIncrements(Predicate<XPropertyIncrement> predicate)
        {
            this.mIncrements.RemoveAll(predicate);
            this.RefreshValues();
        }

        public void ClearIncreaments()
        {
            this.mIncrements.Clear();
            this.RefreshValues();
        }

        public void RefreshValues()
        {
            float result = this.mOriginalValue;

            this.mAffectedPercentIncrements.Clear();
            foreach (XPropertyIncrement increment in this.mIncrements)
            {
                if (increment.mode == XPropertyIncrement.AffectMode.Value)
                {
                    result += increment.value;
                }
                else if (increment.mode == XPropertyIncrement.AffectMode.OriginalPercentage)
                {
                    result += (float)this.mOriginalValue * increment.value;
                }
                else if (increment.mode == XPropertyIncrement.AffectMode.AffectedPercentage)
                {
                    this.mAffectedPercentIncrements.Add(increment);
                }
            }
            foreach (XPropertyIncrement increment in this.mAffectedPercentIncrements)
            {
                result += result * increment.value;
            }

            this.mAffectedValue = result;
        }
        #endregion
    }

    public abstract class XAbstractPropertySet<SlotType, ValueType> where ValueType : class
    {
        protected Dictionary<SlotType, ValueType> mNodes;

        public XAbstractPropertySet()
        {
            this.mNodes = new Dictionary<SlotType, ValueType>();
        }

        public Dictionary<SlotType, ValueType> nodes
        {
            get { return this.mNodes; }
            set { this.mNodes = value; }
        }

        public ICollection<SlotType> slots
        {
            get { return this.mNodes.Keys; }
        }

        public ICollection<ValueType> values
        {
            get { return this.mNodes.Values; }
        }

        public ValueType this[SlotType slot]
        {
            get
            {
                ValueType value = null;
                this.mNodes.TryGetValue(slot, out value);
                return value;
            }
            set
            {
                this.mNodes[slot] = value;
            }
        }

        public bool Contains(SlotType slot)
        {
            return this.mNodes.ContainsKey(slot);
        }
    }

    public class XPropertyIncrementSet<SlotType> : XAbstractPropertySet<SlotType, XPropertyIncrement>
    {
        public XPropertyIncrementSet()
        {
            this.mNodes.Clear();
        }
    }

    public class XPropertyObjectSet<SlotType> : XAbstractPropertySet<SlotType, XPropertyObject>
    {
        public XPropertyObjectSet()
        {
            Array slots = Enum.GetValues(typeof(SlotType));
            foreach (SlotType slot in slots)
            {
                this.mNodes[slot] = new XPropertyObject(slot.ToString());
            }
        }

        public void SetOrignalValues(object pset)
        {
            Type ptype = pset.GetType();
            foreach (var node in this.mNodes)
            {
                SlotType pslot = node.Key;
                string pname = pslot.ToString();
                XPropertyObject pobj = node.Value;

                FieldInfo f = ptype.GetField(pname, BindingFlags.Instance | BindingFlags.Public);
                if (f != null)
                {
                    float v = (float)f.GetValue(pset);
                    pobj.originalValue = v;
                    pobj.instanceValue = float.MaxValue;
                    continue;
                }
                PropertyInfo p = ptype.GetProperty(pname, BindingFlags.Instance | BindingFlags.Public);
                if (p != null)
                {
                    float v = (float)p.GetValue(pset, null);
                    pobj.originalValue = v;
                    pobj.instanceValue = float.MaxValue;
                    continue;
                }
            }
        }

        public void AddIncrements(XPropertyIncrementSet<SlotType> increments)
        {
            if (increments == null)
                return;
            foreach (var node in increments.nodes)
            {
                this[node.Key].AddIncrement(node.Value);
            }
        }

        public void RemoveIncrements(Predicate<XPropertyIncrement> predicate)
        {
            if (predicate == null)
                return;
            foreach (var property in this.values)
            {
                property.RemoveIncrements(predicate);
            }
        }
    }
}
