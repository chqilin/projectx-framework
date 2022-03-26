using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectX
{
    public partial class XTableExchange
    {
        public class StringAdapter_Lua : StringAdapter
        {
            private LuaHolder mLua = new LuaHolder();

            public XTable StringToTable(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return null;
                string luaCode = string.Format("return {0};", str);
                XTable table = this.mLua.Evaluate(luaCode) as XTable;
                return table;
            }

            public string TableToString(XTable table)
            {
                if (table == null)
                    return "{}";
                StringBuilder str = new StringBuilder("{");
                foreach (var node in table.Nodes)
                {
                    string strKey = node.Key.ToString();

                    string strVal = "nil";
                    object objVal = node.Value;
                    if (objVal is XTable)
                    {
                        strVal = this.TableToString(objVal as XTable);
                    }
                    else if (objVal is string)
                    {
                        strVal = objVal.ToString();
                        strVal = "\'" + strVal + "\'";
                    }
                    else if (objVal != null)
                    {
                        strVal = objVal.ToString();
                    }
                    str.AppendFormat("{0}={1},", strKey, strVal);
                }
                str.Append("}");
                return str.ToString();
            }
        }

        private static StringAdapter msLua = new StringAdapter_Lua();
        public static StringAdapter lua { get { return msLua; } }
    }
}
