using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public partial class FunctionManager
    {
        public delegate void Handler(object result, object state);
        public class AsyncInvoking
        {
            public string funcName;
            public object context;
            public object param;           
            public object state;
            public Handler handler;
        }

        public System.Action onInstallFunctions = null;

        private Queue<AsyncInvoking> mAsyncInvokings = new Queue<AsyncInvoking>();
        private DelegateSet mDelegates = new DelegateSet();

        #region Life Circle
        public bool Init()
        {
            try
            {
                XCSharp.InvokeAction(this.onInstallFunctions);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        public void Quit()
        {
            this.mAsyncInvokings.Clear();
            this.mDelegates.Clear();
        }

        public void Update(float elapse)
        {
            while (this.mAsyncInvokings.Count > 0)
            {
                AsyncInvoking invoking = this.mAsyncInvokings.Dequeue();
                object result = this.Invoke(invoking.funcName, invoking.context, invoking.param);
                if(invoking.handler != null)
                {
                    invoking.handler(result, invoking.state);
                }
                break;
            }
        } 
        #endregion

        #region Public Methods
        public void Attach(string funcName, object invokee, string methodName)
        {
            this.mDelegates.Attach(funcName, invokee, methodName);
        }

        public void Attach(string funcName, DelegateSet.Handler funcHandler)
        {
            this.mDelegates.Attach(funcName, funcHandler);
        }

        public object Invoke(string funcName, object context, object param)
        {
            try
            {
                return this.mDelegates.Invoke(funcName, context, param);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public LuaHolder mLua = new LuaHolder();
        public bool InvokeCond(string funcName, object context, string param, string oper, string desval)
        {
            try
            {
                object result = this.mDelegates.Invoke(funcName, context, param);
                string frag = "";
                if (result is bool)
                    frag = ((bool)(result)) ? "true" : "false";
                else
                    frag = result.ToString();

                string expr = string.Format("return {0}{1}{2};", frag, oper, desval);
                result = this.mLua.Evaluate(expr);
                if (result is bool)
                {
                    return (bool)result;
                }

                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public void InvokeAsync(string funcName, object context, object param, object state, Handler handler)
        {
            App.instance.StartCoroutine(this.CreateAsyncInvoking(funcName, context, param, state, handler));
        }
        private IEnumerator CreateAsyncInvoking(string funcName, object context, object param, object state, Handler handler)
        {
            yield return null;
            AsyncInvoking invoking = new AsyncInvoking();
            invoking.funcName = funcName;
            invoking.context = context;
            invoking.param = param;
            invoking.state = state;
            invoking.handler = handler;           
            this.mAsyncInvokings.Enqueue(invoking);
        } 
        #endregion
    }
}
