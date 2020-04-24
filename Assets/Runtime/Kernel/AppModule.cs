using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace ProjectX
{
	public abstract class AppModule : MonoBehaviour
	{
	    public XTable attribute = new XTable();

	    public virtual bool Init()
        {
            return true;
        }

	    public virtual void Quit()
        { }

        public virtual void Ready()
        { }

        public virtual void Tick(float elapse)
        { }
	}

    public class ModuleManager
    {
        private Dictionary<System.Type, AppModule> mModules = new Dictionary<System.Type, AppModule>();

        public void Install<T>() where T : AppModule, new()
        {
            T module = new T();
            mModules.Add(typeof(T), module);
        }

        public void Install(AppModule module)
        {
            if (module == null)
                return;
            mModules.Add(module.GetType(), module);
        }

        public T Select<T>() where T : AppModule
        {
            AppModule module = null;
            mModules.TryGetValue(typeof(T), out module);
            return module as T;
        }

        public bool Init()
        {
            foreach (var node in mModules)
            {
                if (!node.Value.Init())
                {
                    Debug.LogErrorFormat("Init game module [{0}] failed.", node.Key.Name);
                    return false;
                }
            }
            return true;
        }

        public void Quit()
        {
            List<AppModule> moduleList = new List<AppModule>(mModules.Values);
            moduleList.Reverse();
            foreach (var m in moduleList)
            {
                m.Quit();
            }
        }

        public void Ready()
        {
            foreach (var node in mModules)
            {
                node.Value.Ready();
            }
        }

        public void Update(float elapse)
        {
            foreach (var node in mModules)
            {
                node.Value.Tick(elapse);
            }
        }
    }
}