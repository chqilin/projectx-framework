using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ProjectX
{
    public class XTable
    {
        private Dictionary<object, object> mNodes;

        #region Constructors
        public XTable()
        {
            this.mNodes = new Dictionary<object, object>();
        }
        #endregion

        #region Properties and Indices
        public Dictionary<object, object> Nodes
        {
            get { return this.mNodes; }
            set { this.mNodes = value; }
        }
        public ICollection Keys
        {
            get { return this.mNodes.Keys; }
        }
        public ICollection Values
        {
            get { return this.mNodes.Values; }
        }
        public int Count
        {
            get { return this.mNodes.Count; }
        }
        public object this[object key]
        {
            get
            {
                object value = null;
                this.mNodes.TryGetValue(key, out value);
                return value;
            }
            set { this.mNodes[key] = value; }
        }
        #endregion

        #region Generic-Node
        public object Value(object key)
        {
            object value = null;
            this.mNodes.TryGetValue(key, out value);
            return value;
        }
        public T Value<T>(object key)
        {
            object value = null;
            this.mNodes.TryGetValue(key, out value);
            return (T)value;
        }
        public object RequiredValue(object key)
        {
            object value = null;
            this.mNodes.TryGetValue(key, out value);
            if (value == null)
                throw new Exception("The value at [" + key.ToString() + "] is not found or null.");
            return value;
        }
        public T RequiredValue<T>(object key)
        {
            object value = null;
            this.mNodes.TryGetValue(key, out value);
            if (value == null)
                throw new Exception("The value at [" + key.ToString() + "] is not found or null.");
            return (T)value;
        }
        #endregion

        #region Optional Typed Node
        public string String(object key, string defaultValue = "")
        {
            object node = this.Value(key);
            if (node == null)
                return defaultValue;
            return node.ToString();
        }
        public int Int(object key, int def = 0)
        {
            object node = this.Value(key);
            if (node == null)
                return def;
            int ret = def;
            int.TryParse(node.ToString(), out ret);
            return ret;
        }
        public long Long(object key, long def = 0)
        {
            object node = this.Value(key);
            if (node == null)
                return def;
            long ret = def;
            long.TryParse(node.ToString(), out ret);
            return ret;
        }
        public float Float(object key, float def = 0)
        {
            object node = this.Value(key);
            if (node == null)
                return def;
            float ret = def;
            float.TryParse(node.ToString(), out ret);
            return ret;
        }
        public double Double(object key, double def = 0)
        {
            object node = this.Value(key);
            if (node == null)
                return def;
            double ret = def;
            double.TryParse(node.ToString(), out ret);
            return ret;
        }
        public bool Bool(object key, bool def = true)
        {
            object node = this.Value(key);
            if (node == null)
                return def;
            string value = node.ToString();
            return value == "true" || value == "TRUE" || value == "True";
        }
        public T Enum<T>(object key, T def = default(T))
        {
            object node = this.Value(key);
            if (node == null)
                return def;
            return (T)System.Enum.Parse(typeof(T), node.ToString());
        }
        #endregion

        #region Required Typed Node
        public string RequiredString(object key)
        {
            return this.RequiredValue(key).ToString();
        }
        public int RequiredInt(object key)
        {
            return int.Parse(this.RequiredValue(key).ToString());
        }
        public long RequiredLong(object key)
        {
            return long.Parse(this.RequiredValue(key).ToString());
        }
        public float RequiredFloat(object key)
        {
            return float.Parse(this.RequiredValue(key).ToString());
        }
        public double RequiredDouble(object key)
        {
            return double.Parse(this.RequiredValue(key).ToString());
        }
        public bool RequiredBool(object key)
        {
            string value = this.RequiredValue(key).ToString();
            return value == "true" || value == "TRUE" || value == "True";
        }
        public T RequiredEnum<T>(object key)
        {
            return (T)System.Enum.Parse(typeof(T), this.RequiredValue(key).ToString());
        }
        #endregion

        #region Utility Methods
        public bool Contains(object key)
        {
            return this.mNodes.ContainsKey(key);
        }

        public void Clear()
        {
            this.mNodes.Clear();
        }

        public static XTable EnumTable<T>()
        {
            XTable table = new XTable();
            foreach (object enumValue in System.Enum.GetValues(typeof(T)))
            {
                table[enumValue.ToString()] = (int)(enumValue);
            }
            return table;
        }

        public static XTable ObjectTable(object o)
        {
            XTable table = new XTable();
            if (o == null)
                return table;

            FieldInfo[] fields = o.GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                Type t = f.FieldType;
                string n = f.Name;
                object v = f.GetValue(o);
                switch (t.FullName)
                {
                    case "System.Boolean":
                    case "System.Char":
                    case "System.Byte":
                    case "System.SByte":
                    case "System.Int16":
                    case "System.UInt16":
                    case "System.Int32":
                    case "System.UInt32":
                    case "System.Int64":
                    case "System.UInt64":
                    case "System.Single":
                    case "System.Double":
                    case "System.Decimal":
                    case "System.String":
                    case "XTable":
                        table[n] = v;
                        break;
                    default:
                        table[n] = XTable.ObjectTable(v);
                        break;
                }
            }
            return table;
        }

        public static object TableObject(XTable table, Type type)
        {
            if (table == null || table.Count == 0)
                return null;
            object o = Activator.CreateInstance(type);
            FieldInfo[] fields = o.GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                Type t = f.FieldType;
                string n = f.Name;
                object v = f.GetValue(o);
                switch (t.FullName)
                {
                    case "System.Boolean":
                    case "System.Char":
                    case "System.Byte":
                    case "System.SByte":
                    case "System.Int16":
                    case "System.UInt16":
                    case "System.Int32":
                    case "System.UInt32":
                    case "System.Int64":
                    case "System.UInt64":
                    case "System.Single":
                    case "System.Double":
                    case "System.Decimal":
                    case "System.String":
                    case "XTable":
                        v = table[n];
                        break;
                    default:
                        v = XTable.TableObject(table[n] as XTable, t);
                        break;
                }
                f.SetValue(o, v);
            }
            return o;
        }
        #endregion
    }
}

