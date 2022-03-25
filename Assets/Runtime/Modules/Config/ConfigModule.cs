using System;
using UnityEngine;

namespace ProjectX
{
    public class ConfigModule : AppModule
    {
        public delegate string SheetReader(string sheetName);
        public SheetReader onReadSheet = null;
        public Action<ConfigModule> onLoadConfigs = null;
        public Action<ConfigModule> onUnloadConfigs = null;

        public override bool Init()
        {
            XCSharp.InvokeAction(this.onLoadConfigs, this);
            return true;
        }

        public override void Quit()
        {
            XCSharp.InvokeAction(this.onUnloadConfigs, this);
        }

        public string ReadSheet<T>()
        {
            return this.ReadSheet(typeof(T).Name);
        }

        public string ReadSheet(string sheetName)
        {
            if (this.onReadSheet == null)
                return "";
            return this.onReadSheet(sheetName);
        }
    }
}
