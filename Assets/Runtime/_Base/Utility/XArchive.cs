using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class XArchive
    {
        private string mFilePath;
        private XTable mContents;

        public XArchive()
        {
            this.mContents = new XTable();
        }

        #region Properties
        public string Path
        {
            get { return this.mFilePath; }
        }
        public XTable Contents
        {
            get { return this.mContents; }
        }
        #endregion

        public bool Load(string filePath)
        {
            this.mFilePath = filePath;
            string content = XFile.ReadTextFile(this.mFilePath);
            this.mContents = XTableExchange.lua.StringToTable(XCSharp.EncodeUTF8(content));
            if (this.mContents == null)
            {
                this.mContents = new XTable();
            }
            return true;
        }

        public void Save()
        {
            string content = XTableExchange.lua.TableToString(this.mContents);
            XFile.WriteTextFile(this.mFilePath, content);
        }
    }
}

