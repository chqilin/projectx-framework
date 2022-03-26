using System;
using System.Reflection;

namespace ProjectX
{
    public abstract class XReflector
    {
        public object Invoke(string method, bool required, params object[] args)
        {
            return XReflector.InvokeInstance(this, method, required, args);
        }

        public static object InvokeStatic<T>(string method, bool required, params object[] args)
        {
            return XReflector.InvokeStatic(typeof(T), method, required, args);
        }

        public static object InvokeStatic(string type, string method, bool required, params object[] args)
        {
            return XReflector.InvokeStatic(Type.GetType(type), method, required, args);
        }

        public static object InvokeInstance(object target, string method, bool required, params object[] args)
        {
            if (target == null)
                throw new NullReferenceException("Target object is null.");

            Type type = target.GetType();
            MethodInfo mInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (mInfo == null)
            {
                if (required)
                    throw new Exception("Method [" + type.Name + "." + method + "] not found.");
                else
                    return null;
            }

            try
            {
                return mInfo.Invoke(target, args.Length > 0 ? args : null);
            }
            catch (Exception e)
            {
                string message = "Method [" + type.Name + "." + method + "] invoke failed.";              
                throw new Exception(message, e.InnerException);
            }
        }

        public static object InvokeStatic(Type type, string method, bool required, params object[] args)
        {
            if (type == null)
                throw new NullReferenceException("Target type is null");
            
            MethodInfo mInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (mInfo == null)
            {
                if (required)
                    throw new Exception("Method [" + type.Name + "." + method + "] not found.");
                else
                    return null;
            }

            try
            {
                return mInfo.Invoke(null, args.Length > 0 ? args : null);
            }
            catch (Exception e)
            {
                string message = "Method [" + type.Name + "." + method + "] invoke failed.";
                throw new Exception(message, e.InnerException);
            }
        }
    }
}
