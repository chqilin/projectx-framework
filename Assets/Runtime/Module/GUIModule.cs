using UnityEngine;
using System.Collections.Generic;

namespace ProjectX
{
    [RequireComponent(typeof(GUIManager))]
    public class GuiModule : AppModule
    {
        private GUIManager mManager = null;

        #region Properties and Indexers
        public GameObject this[string uiName]
        {
            get { return this.mManager.FindUI(uiName); }
        }
        #endregion

        #region Life Circle
        public override bool Init()
        {
            this.mManager = this.GetComponent<GUIManager>();
            this.mManager.OnLoadPrefab = (uiName) => {
                return App.assets.LoadPrefab("UI", uiName) as GameObject;
            };

            return true;
        }

        public override void Quit()
        { }
        #endregion

        #region Public Methods
        public GameObject OpenUI(string uiName, XTable attr = null)
        {
            return this.mManager.OpenUI(uiName, attr);
        }

        public GameObject OpenUI(GUILayer layer, string uiName, XTable attr = null)
        {
            return this.mManager.OpenUI(layer, uiName, attr);
        }

        public void CloseUI(string uiName)
        {
            this.mManager.CloseUI(uiName);
        }

        public void CloseUI(GUIGroup group, string uiName)
        {
            this.mManager.CloseUI(group, uiName);
        }

        public void CloseAll(GUIGroup group = null)
        {
            this.mManager.CloseAll(group);
        }

        public bool IsOpen(string uiName)
        {
            return this.mManager.IsOpen(uiName);
        }

        public void NotifyAll(string method, params object[] param)
        {
            this.mManager.NotifyAll(method, param);
        }

        public void NotifyAll(GUIGroup group, string method, params object[] param)
        {
            this.mManager.NotifyAll(group, method, param);
        }

        public void Notify(string uiName, string method, params object[] param)
        {
            this.mManager.Notify(uiName, method, param);
        }

        public void Notify(GUIGroup group, string uiName, string method, params object[] param)
        {
            this.mManager.Notify(group, uiName, method, param);
        }
        #endregion
    }
}
