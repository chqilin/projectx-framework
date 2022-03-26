using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ProjectX
{
    /// <summary>
    /// Delegate Set.
    /// A delegate container which provides attach and invoke methods.
    /// </summary>
    public class DelegateSet
    {
        /// <summary>
        /// Delegate func.
        /// All func-implementation method must use this declaration.
        /// </summary>
        public delegate object Handler(object context, object param);

        private Dictionary<string, Handler> mHandlers;

        public DelegateSet()
        {
            this.mHandlers = new Dictionary<string, Handler>();
        }

        public void Attach(string name, Handler func)
        {
            if (string.IsNullOrEmpty(name) || func == null)
                throw new System.ArgumentNullException();
            Handler handler = null;
            this.mHandlers.TryGetValue(name, out handler);
            if (handler == null)
            {
                this.mHandlers[name] = func;
            }
            else
            {
                handler += func;
            }
        }

        public void Attach(string name, object invokee, string method)
        {
            try
            {
                MethodInfo methodInfo = invokee.GetType().GetMethod(method);
                Handler func = System.Delegate.CreateDelegate(typeof(Handler), invokee, methodInfo, true) as Handler;
                this.Attach(name, func);
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        public void Detach(string funcName)
        {
            if (this.mHandlers.ContainsKey(funcName))
            {
                this.mHandlers.Remove(funcName);
            }
        }

        public void Clear()
        {
            this.mHandlers.Clear();
        }

        public object Invoke(string name, object context, object param)
        {
            Handler handler = null;
            this.mHandlers.TryGetValue(name, out handler);
            if (handler != null)
            {
                return handler(context, param);
            }
            return null;
        }
    }
}

