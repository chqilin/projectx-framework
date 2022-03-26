using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectX
{
    public partial class XTableExchange
    {
        public interface StringAdapter
        {
            XTable StringToTable(string str);
            string TableToString(XTable table);
        }

        #region XTable <--> Unity Types
        public static XTable ValueToTable(Transform value)
        {
            XTable table = new XTable();
            table["P"] = ValueToTable(value.position);
            table["R"] = ValueToTable(value.rotation);
            table["S"] = ValueToTable(value.localScale);
            return table;
        }

        public static XTable ValueToTable(Vector2 value)
        {
            XTable table = new XTable();
            table["x"] = value.x;
            table["y"] = value.y;
            return table;
        }

        public static XTable ValueToTable(Vector3 value)
        {
            XTable table = new XTable();
            table["x"] = value.x;
            table["y"] = value.y;
            table["z"] = value.z;
            return table;
        }

        public static XTable ValueToTable(Vector4 value)
        {
            XTable table = new XTable();
            table["x"] = value.x;
            table["y"] = value.y;
            table["z"] = value.z;
            table["w"] = value.w;
            return table;
        }

        public static XTable ValueToTable(Quaternion value)
        {
            XTable table = new XTable();
            table["x"] = value.x;
            table["y"] = value.y;
            table["z"] = value.z;
            table["w"] = value.w;
            return table;
        }

        public static XTable ValueToTable(Color value)
        {
            XTable table = new XTable();
            table["r"] = value.r;
            table["g"] = value.g;
            table["b"] = value.b;
            table["a"] = value.a;
            return table;
        }

        public static XTable ValueToTable(Color32 value)
        {
            XTable table = new XTable();
            table["r"] = value.r;
            table["g"] = value.g;
            table["b"] = value.b;
            table["a"] = value.a;
            return table;
        }

        public static void TableToValue(XTable table, ref Transform value)
        {
            XTable pt = table.Value<XTable>("P");
            XTable rt = table.Value<XTable>("R");
            XTable st = table.Value<XTable>("S");

            Vector3 pv = Vector3.zero;
            Vector3 rv = Vector3.zero;
            Vector3 sv = Vector3.one;

            if (pt != null) TableToValue(pt, ref pv);
            if (rt != null) TableToValue(pt, ref rv);
            if (st != null) TableToValue(pt, ref sv);

            value.position = pv;
            value.rotation = Quaternion.Euler(rv);
            value.localScale = sv;
        }

        public static void TableToValue(XTable table, ref Vector2 value)
        {
            value.x = table.Float("x");
            value.y = table.Float("y");
        }

        public static void TableToValue(XTable table, ref Vector3 value)
        {
            value.x = table.Float("x");
            value.y = table.Float("y");
            value.z = table.Float("z");
        }

        public static void TableToValue(XTable table, ref Vector4 value)
        {
            value.x = table.Float("x");
            value.y = table.Float("y");
            value.z = table.Float("z");
            value.w = table.Float("w");
        }

        public static void TableToValue(XTable table, ref Quaternion value)
        {
            value.x = table.Float("x");
            value.y = table.Float("y");
            value.z = table.Float("z");
            value.w = table.Float("w");
        }

        public static void TableToValue(XTable table, ref Color value)
        {
            value.r = table.Float("r");
            value.g = table.Float("g");
            value.b = table.Float("b");
            value.a = table.Float("a");
        }

        public static void TableToValue(XTable table, ref Color32 value)
        {
            value.r = (byte)table.Int("r");
            value.g = (byte)table.Int("g");
            value.b = (byte)table.Int("b");
            value.a = (byte)table.Int("a");
        }
        #endregion
    }
}
