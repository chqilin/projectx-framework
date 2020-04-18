using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectX
{
    public partial class XTableExchange
    {
        public class StringAdapter_Json : StringAdapter
        {
            public XTable StringToTable(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return null;
                return Json.Parse(str);
            }

            public string TableToString(XTable table)
            {
                if (table == null)
                    return "{}";
                return Json.Stringify(table);
            }
        }

        private static StringAdapter msJson = new StringAdapter_Json(); 
        public static StringAdapter json { get { return msJson; } }
    }
}
