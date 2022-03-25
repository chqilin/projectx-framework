using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public partial class LuaModule : AppModule
    {
        public System.Action<LuaModule> onExportLib = null;
        public System.Action<LuaModule> onImportLua = null;

        private LuaHolder mLuaHolder = new LuaHolder();

        #region Life Circle
        public override bool Init()
        {
            try
            {
                XCSharp.InvokeAction(this.onExportLib, this);
                XCSharp.InvokeAction(this.onImportLua, this);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
            return true;
        }

        public override void Quit()
        { }
        #endregion

        #region Public Methods
        public void Export<T>(bool global) where T : LuaLibrary, new()
        {
            T lib = new T();
            this.mLuaHolder.Export(lib.name, lib.LibMain(this.mLuaHolder), global);
        }

        public void Export(string lib, LuaFuncPair[] funcs, bool global)
        {
            this.mLuaHolder.Export(lib, funcs, global);
        }

        public void Import(string fileName)
        {
            this.mLuaHolder.Import(fileName);
        }

        public object Evaluate(string luastr)
        {
            return this.mLuaHolder.Evaluate(luastr);
        }

        public object Invoke(string funcName, params object[] funcArgs)
        {
            return this.mLuaHolder.Invoke(funcName, funcArgs);
        }
        #endregion
    }
}
