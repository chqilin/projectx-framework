using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public class GuiModule : AppModule
    {
        public Transform mainGroup = null;
        public List<Transform> groups = new List<Transform>();

        #region Properties and Indexers
        public GameObject this[string uiName]
        {
            get { return this.FindUI(uiName); }
        }
        #endregion

        #region Life Circle
        public override bool Init()
        {
            return true;
        }

        public override void Quit()
        { }
        #endregion

        #region Public Methods
        public GameObject OpenUI(string uiName, XTable attr = null)
        {
            GameObject ui = this.FindUI(uiName);
            if (ui == null)
            {
                ui = this.CreateUI(uiName);
            }
            return this.OpenUI(ui, attr);
        }

        public GameObject OpenUI(Transform group, string uiName, XTable attr = null)
        {
            GameObject ui = this.FindUI(group, uiName);
            if(ui == null)
            {
                ui = this.CreateUI(group, uiName);
            }
            return this.OpenUI(ui, attr);
        }

        public GameObject OpenUI(GameObject ui, XTable attr = null)
        {
            if (ui == null)
                return null;

            GUIController controller = ui.GetComponent<GUIController>();
            if (controller != null)
            {
                if (attr != null)
                {
                    controller.attribute = attr;
                }
                controller.Open();
            }
            
            return ui;
        }

        public void CloseUI(string uiName)
        {
            GameObject ui = this.FindUI(uiName);
            this.CloseUI(ui);
        }

        public void CloseUI(Transform group, string uiName)
        {
            GameObject ui = this.FindUI(group, uiName);
            this.CloseUI(ui);
        }

        public void CloseUI(GameObject ui)
        {
            if (ui == null)
                return;
            GUIController controller = ui.GetComponent<GUIController>();
            if (controller != null && controller.IsOpen)
            {
                controller.Close();
            }
        }

        public void CloseAll(Transform group = null)
        {
            this.ForeachUI(group, ui => 
            {
                this.CloseUI(ui);
                return true;
            });
        }

        public bool IsOpen(string uiName)
        {
            GameObject ui = this.FindUI(uiName);
            return this.IsOpen(ui);
        }

        public bool IsOpen(GameObject ui)
        {
            if (ui == null)
                return false;
            GUIController controller = ui.GetComponent<GUIController>();
            return controller != null && controller.IsOpen;
        }

        public void NotifyAll(string method, params object[] param)
        {
            this.ForeachUI(null, ui =>
            {
                this.Notify(ui, method, param);
                return true;
            });
        }

        public void NotifyAll(Transform group, string method, params object[] param)
        {
            this.ForeachUI(group, ui =>
            {
                this.Notify(ui, method, param);
                return true;
            });
        }

        public void Notify(string uiName, string method, params object[] param)
        {
            GameObject ui = this.FindUI(uiName);
            this.Notify(ui, method, param);
        }

        public void Notify(Transform group, string uiName, string method, params object[] param)
        {
            GameObject ui = this.FindUI(group, uiName);
            this.Notify(ui, method, param);
        }

        public void Notify(GameObject ui, string method, params object[] param)
        {
            if (ui == null)
                return;
            GUIController controller = ui.GetComponent<GUIController>();
            if (controller == null)
                return;
            XReflector.InvokeInstance(controller, method, true, param);
        }

        public GameObject CreateUI(string uiName)
        {
            if (this.groups.Count == 0)
                return null;
            return this.CreateUI(this.mainGroup, uiName);
        }

        public GameObject CreateUI(Transform group, string uiName)
        {
            if (group == null)
                return null;
            GameObject prefab = App.assets.LoadPrefab("Gui", uiName) as GameObject;
            if (prefab == null)
                return null;
            GameObject ui = this.CreateUI(group, prefab);
            return ui;
        }

        public GameObject CreateUI(Transform group, GameObject prefab)
        {
            GameObject ui = Object.Instantiate(prefab) as GameObject;
            if (ui == null)
                return null;
            ui.name = prefab.name;
            ui.transform.SetParent(group.transform, false);
            return ui;
        }

        public GameObject FindUI(string uiName)
        {
            string uiNameClone = XUtility.AddCloneMarkForName(uiName);
            GameObject result = null;
            this.ForeachUI(null, ui =>
            {
                if (ui.name == uiName || ui.name == uiNameClone)
                {
                    result = ui;
                    return false;
                }
                return true;
            });
            return result;
        }

        public GameObject FindUI(Transform group, string uiName)
        {
            string uiNameClone = XUtility.AddCloneMarkForName(uiName);
            GameObject result = null;
            this.ForeachUI(group, ui =>
            {
                if (ui.name == uiName || ui.name == uiNameClone)
                {
                    result = ui;
                    return false;
                }
                return true;
            });
            return result;
        }

        public Texture2D LoadTexture(string subPath)
        {
            return App.assets.LoadAsset<Texture2D>("UI", "Textures/" + subPath);
        }
        #endregion

        #region Private Methods
        private delegate bool ForeachHandler(GameObject ui);
        private void ForeachUI(Transform group, ForeachHandler handler)
        {
            if (handler == null)
                return;

            if (group != null)
            {
                for (int p = 0; p < group.childCount; p++)
                {
                    Transform t = group.GetChild(p);
                    if (!handler(t.gameObject))
                        return;
                }
                return;
            }

            for (int i = 0; i < this.groups.Count; i++)
            {
                Transform g = this.groups[i];
                for (int p = 0; p < g.childCount; p++)
                {
                    Transform t = g.GetChild(p);
                    if (!handler(t.gameObject))
                        return;
                }
            }
        } 
        #endregion
    }
}
