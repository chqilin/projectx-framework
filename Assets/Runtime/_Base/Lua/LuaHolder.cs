using System;
using System.Collections;
using UniLua;

namespace ProjectX
{
    public delegate int XLuaFunc(LuaHolder lua);
    public struct LuaFuncPair
    {
        public string name;
        public XLuaFunc func;
        public LuaFuncPair(string name, XLuaFunc func)
        {
            this.name = name;
            this.func = func;
        }
        public static implicit operator NameFuncPair(LuaFuncPair pair)
        {
            return new NameFuncPair(pair.name, lua => pair.func(new LuaHolder(lua)));
        }
    }

    public class LuaLibrary
    {
        public virtual string name
        {
            get { return ""; }
        }

        public virtual LuaFuncPair[] LibMain(LuaHolder xlua)
        {
            return null;
        }
    }

    public class LuaHolder
    {
        private ILuaState mLuaState;

        #region Constructors
        public LuaHolder()
        {
            mLuaState = LuaAPI.NewState();
            mLuaState.L_OpenLibs();
        }
        public LuaHolder(ILuaState luaState)
        {
            this.mLuaState = luaState;
        }
        #endregion

        #region Library and Functions
        public void Export(string libName, LuaFuncPair[] funcs, bool global)
        {
            CSharpFunctionDelegate newlib = lua =>
            {
                NameFuncPair[] define = new NameFuncPair[funcs.Length];
                for (int i = 0; i < funcs.Length; i++)
                {
                    define[i] = funcs[i];
                }
                lua.L_NewLib(define);
                return 1;
            };
            mLuaState.L_RequireF(libName, newlib, global);
        }

        public void Import(string fileName)
        {
            UniLua.ThreadStatus status = mLuaState.L_DoFile(fileName);
            if (status != UniLua.ThreadStatus.LUA_OK)
            {
                string errmsg = mLuaState.ToString(-1);
                mLuaState.Pop(1);

                System.Text.StringBuilder stsb = new System.Text.StringBuilder();
                stsb.Append("XLuaHolder can not import. \n")
                    .AppendFormat("Error: {0} \n", status)
                    .AppendFormat("Message: {0} \n", errmsg);
                string message = stsb.ToString();
                throw new Exception(message);
            }
        }

        public object Evaluate(string str)
        {
            UniLua.ThreadStatus status = mLuaState.L_DoString(str);
            if (status != UniLua.ThreadStatus.LUA_OK)
            {
                string errmsg = mLuaState.ToString(-1);
                mLuaState.Pop(1);

                System.Text.StringBuilder stsb = new System.Text.StringBuilder();
                stsb.Append("XLuaHolder can not evaluate. \n")
                    .AppendFormat("Error: {0} \n", status)
                    .AppendFormat("Message: {0} \n", errmsg);
                string message = stsb.ToString();
                throw new Exception(message);
            }

            object result = this.PopRet();
            return result;
        }

        public object Execute(string objName)
        {
            mLuaState.GetGlobal(objName);
            if (!mLuaState.IsNoneOrNil(-1) && !mLuaState.IsFunction(-1))
            {
                object result = this.GetValue(-1);
                mLuaState.Pop(1);
                return result;
            }
            return null;
        }

        public object Invoke(string funcName, params object[] args)
        {
            mLuaState.GetGlobal(funcName);
            int numArgs = this.PushArgs(args);
            mLuaState.Call(numArgs, 1);
            object result = this.PopRet();
            return result;
        }

        /*
        public int Register(string name)
        {
            mLuaState.GetField(-1, name);
            if (!mLuaState.IsFunction(-1))
            {
                throw new Exception(
                    string.Format("Function:{0} Not Found.", name)
                );
            }
            return msLuaState.L_Ref(UniLua.LuaDef.LUA_REGISTRYINDEX);
        }
         */

        public object Invoke(int funcId, params object[] args)
        {
            mLuaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, funcId);
            int numArgs = this.PushArgs(args);
            mLuaState.Call(numArgs, 1);
            object result = mLuaState.ToObject(-1);
            mLuaState.Pop(1);
            return result;
        }

        private object SafeInvoke(int funcId, params object[] args)
        {
            mLuaState.PushCSharpFunction(Traceback);
            int traceback = mLuaState.GetTop();

            mLuaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, funcId);
            int numArgs = this.PushArgs(args);
            UniLua.ThreadStatus status = mLuaState.PCall(numArgs, 1, traceback);
            if (status != UniLua.ThreadStatus.LUA_OK)
            {
                throw new Exception(mLuaState.ToString(-1));
            }

            object result = mLuaState.ToObject(-1);
            mLuaState.Pop(1);
            mLuaState.Remove(traceback);
            return result;
        }
        #endregion

        #region Stack Operations
        public int PushValue(object o)
        {
            if (o == null)
            {
                mLuaState.PushNil();
                return 1;
            }

            Type t = o.GetType();
            switch (t.FullName)
            {
                case "System.Boolean":
                    {
                        mLuaState.PushBoolean((bool)o);
                        return 1;
                    }
                case "System.Char":
                    {
                        mLuaState.PushString(((char)o).ToString());
                        return 1;
                    }
                case "System.Byte":
                    {
                        mLuaState.PushNumber((byte)o);
                        return 1;
                    }
                case "System.SByte":
                    {
                        mLuaState.PushNumber((sbyte)o);
                        return 1;
                    }
                case "System.Int16":
                    {
                        mLuaState.PushNumber((short)o);
                        return 1;
                    }
                case "System.UInt16":
                    {
                        mLuaState.PushNumber((ushort)o);
                        return 1;
                    }
                case "System.Int32":
                    {
                        mLuaState.PushNumber((int)o);
                        return 1;
                    }
                case "System.UInt32":
                    {
                        mLuaState.PushNumber((uint)o);
                        return 1;
                    }
                case "System.Int64":
                    {
                        mLuaState.PushUInt64((ulong)o);
                        return 1;
                    }
                case "System.UInt64":
                    {
                        mLuaState.PushUInt64((ulong)o);
                        return 1;
                    }
                case "System.Single":
                    {
                        mLuaState.PushNumber((float)o);
                        return 1;
                    }
                case "System.Double":
                    {
                        mLuaState.PushNumber((double)o);
                        return 1;
                    }
                case "System.Decimal":
                    {
                        mLuaState.PushLightUserData((decimal)o);
                        return 1;
                    }
                case "System.String":
                    {
                        string utf8 = XCSharp.EncodeUTF8(o as string);
                        mLuaState.PushString(utf8);
                        return 1;
                    }
                case "System.Object":
                    {
                        mLuaState.PushLightUserData((object)o);
                        return 1;
                    }
                case "XTable":
                    {
                        mLuaState.NewTable();
                        XTable table = o as XTable;
                        foreach (string key in table.Nodes.Keys)
                        {
                            object val = table.Nodes[key];
                            this.PushValue(key);
                            this.PushValue(val);

                            mLuaState.RawSet(-3);
                        }
                        mLuaState.RawSet(-3);

                        return 1;
                    }
                default:
                    {
                        mLuaState.PushLightUserData((object)o);
                        return 1;
                    }
            }
        }

        public object GetValue(int index)
        {
            string type = mLuaState.L_TypeName(index);
            switch (type)
            {
                case "number":
                    return mLuaState.ToNumber(index);
                case "boolean":
                    return mLuaState.ToBoolean(index);
                case "string":
                    string utf8 = mLuaState.ToString(index);
                    return XCSharp.DecodeUTF8(utf8);
                case "function":
                    return mLuaState.L_Ref(UniLua.LuaDef.LUA_REGISTRYINDEX);
                case "table":
                    XTable table = new XTable();
                    {
                        mLuaState.PushValue(index);
                        int t = mLuaState.GetTop();

                        mLuaState.PushNil();
                        while (mLuaState.Next(t))
                        {
                            object key = this.GetValue(-2);
                            object val = this.GetValue(-1);
                            table[key] = val;

                            mLuaState.Pop(1);
                        }
                        mLuaState.Pop(1);
                    }
                    return table;
                default:
                    return mLuaState.ToUserData(index);
            }
        }
        #endregion

        #region Private Methods
        private int PushArgs(params object[] args)
        {
            foreach (object arg in args)
            {
                this.PushValue(arg);
            }
            return args.Length;
        }

        private object PopRet()
        {
            object result = this.GetValue(-1);
            mLuaState.Pop(1);
            return result;
        }

        private int Traceback(UniLua.ILuaState luaState)
        {
            if (luaState.IsNoneOrNil(1))
            {
                if (!luaState.L_CallMeta(1, "__tostring"))
                {
                    luaState.PushString("(no error message)");
                }
            }
            else
            {
                string message = luaState.ToString(1);
                if (message != null)
                {
                    luaState.L_Traceback(luaState, message, 1);
                }
            }
            return 1;
        }
        #endregion
    }
}

