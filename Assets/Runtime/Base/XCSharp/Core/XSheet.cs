
using System;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public class XSheetCell
    {
        private string mValue;

        public XSheetCell()
        {
            this.mValue = "";
        }

        public string value
        {
            get { return this.mValue; }
            set { this.mValue = value; }
        }

        public static implicit operator string(XSheetCell cell)
        {
            return cell.mValue;
        }
        public static implicit operator byte(XSheetCell cell)
        {
            if (cell.mValue == "")
                return 0;
            try
            {
                return byte.Parse(cell.mValue);
            }
            catch (Exception e)
            {
                throw ParseException<byte>(e, cell);
            }
        }
        public static implicit operator short(XSheetCell cell)
        {
            if (cell.mValue == "")
                return 0;
            try
            {
                return short.Parse(cell.mValue);
            }
            catch (Exception e)
            {
                throw ParseException<short>(e, cell);
            }
        }
        public static implicit operator int(XSheetCell cell)
        {
            if (cell.mValue == "")
                return 0;
            try
            {
                return int.Parse(cell.mValue);
            }
            catch (Exception e)
            {
                throw ParseException<int>(e, cell);
            }
        }
        public static implicit operator long(XSheetCell cell)
        {
            if (cell.mValue == "")
                return 0;
            try
            {
                return long.Parse(cell.mValue);
            }
            catch (Exception e)
            {
                throw ParseException<long>(e, cell);
            }
        }
        public static implicit operator float(XSheetCell cell)
        {
            if (cell.mValue == "")
                return 0;
            try
            {
                return float.Parse(cell.mValue);
            }
            catch (Exception e)
            {
                throw ParseException<float>(e, cell);
            }
        }
        public static implicit operator double(XSheetCell cell)
        {
            if (cell.mValue == "")
                return 0;
            try
            {
                return double.Parse(cell.mValue);
            }
            catch (Exception e)
            {
                throw ParseException<double>(e, cell);
            }
        }
        public static implicit operator bool(XSheetCell cell)
        {
            return cell.mValue == "true"
                || cell.mValue == "TRUE"
                || cell.mValue == "True";
        }

        private static Exception ParseException<T>(Exception e, XSheetCell c)
        {
            return new Exception(string.Format("{0}\nA '{1}' value is expected but the cell value is '{2}'", e.Message, typeof(T).Name, c.mValue));
        }
    }

    public class XSheetRow
    {
        private Dictionary<string, XSheetCell> mCells;

        public XSheetRow(List<string> columns)
        {
            this.mCells = new Dictionary<string, XSheetCell>();
            foreach (string col in columns)
            {
                this.mCells[col] = new XSheetCell();
            }
        }

        public XSheetCell this[string column]
        {
            get
            {
                XSheetCell cell = null;
                this.mCells.TryGetValue(column, out cell);
                return cell;
            }
            set
            {
                this.mCells[column] = value;
            }
        }
        
        public void Parse(string data, ref int pos)
        {
            foreach (var c in this.mCells.Values)
            {
                if (pos > data.Length)
                    break;
                int i = pos;
                while (i < data.Length && data[i] != '|')
                    i++;
                c.value = data.Substring(pos, i - pos).Trim();
                pos = i + 1;
            }
        }
    }

    public class XSheet
    {
        private List<string> mColumns = new List<string>();
        private List<XSheetRow> mRows = new List<XSheetRow>();

        public ICollection<string> columns
        {
            get { return this.mColumns; }
        }

        public List<XSheetRow> rows
        {
            get { return this.mRows; }
        }

        public static XSheet ParseString(string data)
        {
            XSheet table = new XSheet();
            int j = 0;
            int i = 0;
            for (i = 0; i < data.Length; i++)
            {
                if (data[i] != ',' && data[i] != '\n')
                    continue;
                string column = data.Substring(j, i - j).Trim();
                table.mColumns.Add(column);
                j = i + 1;
                if (data[i] == '\n')
                {
                    break;
                }
            }
            data = data.Substring(i + 1).Trim();

            int pos = 0;
            while (pos < data.Length)
            {
                XSheetRow row = new XSheetRow(table.mColumns);
                row.Parse(data, ref pos);
                table.mRows.Add(row);
            }
            return table;
        }
    }
}

