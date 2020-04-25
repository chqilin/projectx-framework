
using System.Collections.Generic;
using UnityEngine;

namespace ProjectX
{
    public partial class App : MonoBehaviour
    {
        #region Facades
        public static App instance
        {
            get; private set;
        }
        public static AssetManager assets
        {
            get { return instance.mAssetManager; }
        }
        public static ModuleManager modules
        {
            get { return instance.mModuleManager; }
        }
        public static FunctionManager funcs
        {
            get { return instance.mFunctionManager; }
        }
        public static FSM states
        {
            get { return instance.mStateManager; }
        }
        #endregion

        #region App Life Circle
        protected virtual void Init()
        { 
            // Application Configs.
            Application.targetFrameRate = 300; // run as fast as I can
            Screen.sleepTimeout = SleepTimeout.NeverSleep; // forbid screen sleep

            // install modules.
            foreach (var m in this.moduleList)
            {
                mModuleManager.Install(m);
            }
        }

        protected virtual void Quit()
        { }

        protected virtual void Ready()
        { }

        protected virtual void Tick(float elapse) 
        { }
        #endregion

        #region Unity Life Circle
        [SerializeField]
        private List<AppModule> moduleList = new List<AppModule>();

        private AssetManager mAssetManager = new AssetManager();
        private ModuleManager mModuleManager = new ModuleManager();
        private FunctionManager mFunctionManager = new FunctionManager();
        private FSM mStateManager = new FSM();

        void Awake()
        {
            instance = this;
            instance.Init();

            // asset manager
            if (!mAssetManager.Init())
            {
                Debug.LogError("Init asset manager failed.");
                return;
            }

            // module manager
            if (!mModuleManager.Init())
            {
                Debug.LogError("Init module manager failed.");
                return;
            }

            // function manager
            if (!mFunctionManager.Init())
            {
                Debug.LogError("Init function manager failed.");
                return;
            }

            // state manager
            mStateManager.Init();

            // debug-Console
            DebugConsole console = instance.GetComponent<DebugConsole>();
            if (console != null)
            {
                console.OnInput = text =>
                {
                    string[] segs = text.Split(new string[] { ">>" }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (segs.Length > 0)
                    {
                        string funcName = segs[0].Trim();
                        string funcParam = segs.Length > 1 ? segs[1].Trim() : "";
                        mFunctionManager.InvokeAsync(funcName, null, funcParam, null, null);
                    }
                };
            }
        }

        void OnDestroy()
        {
            mStateManager.Quit();
            mFunctionManager.Quit();
            mModuleManager.Quit();
            mAssetManager.Quit();

            // instance.gameObject may be destroyed now
            // we must check it.
            if(instance != null)
            {
                instance.Quit();
                instance = null;
            }

            Application.Quit();
        }

        void Start()
        {
            instance.Ready();
            mModuleManager.Ready();
        }

        void Update()
        {
            float elapse = Time.deltaTime;
            
            instance.Tick(elapse);

            mAssetManager.Update(elapse);
            mModuleManager.Update(elapse);
            mFunctionManager.Update(elapse);
            mStateManager.Update(elapse);
        }
        #endregion 
    }
}
