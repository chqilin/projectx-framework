using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectX
{
    public class GUIManager : MonoBehaviour
    {
        public delegate GameObject LoadPrefabFunc(string uiName);
        public LoadPrefabFunc OnLoadPrefab = null;

        public GUIGroup mainGroup = null;
        public List<GUIGroup> groups = new List<GUIGroup>();

        #region Properties and Indexers
        public GameObject this[string uiName]
        {
            get { return this.FindUI(uiName); }
        }
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

        public GameObject OpenUI(GUILayer layer, string uiName, XTable attr = null)
        {
            int layerIndex = (int)layer;
            if (layerIndex < 0 || layerIndex >= this.groups.Count)
                return null;

            GUIGroup group = this.groups[layerIndex];
            GameObject ui = this.FindUI(group, uiName);
            if (ui == null)
            {
                ui = this.CreateUI(group, uiName);
            }

            return this.OpenUI(ui, attr);
        }

        public GameObject OpenUI(GameObject ui, XTable attr = null)
        {
            if (ui == null)
                return null;

            GUIBehavior behavior = ui.GetComponent<GUIBehavior>();
            if (behavior != null)
            {
                if (attr != null)
                {
                    behavior.attribute = attr;
                }
                behavior.Open();
            }

            return ui;
        }

        public void CloseUI(string uiName)
        {
            GameObject ui = this.FindUI(uiName);
            this.CloseUI(ui);
        }

        public void CloseUI(GUIGroup group, string uiName)
        {
            GameObject ui = this.FindUI(group, uiName);
            this.CloseUI(ui);
        }

        public void CloseUI(GameObject ui)
        {
            if (ui == null)
                return;
            var behavior = ui.GetComponent<GUIBehavior>();
            if (behavior != null && behavior.IsOpen)
            {
                behavior.Close();
            }
        }

        public void CloseAll(GUIGroup group = null)
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
            var behavior = ui.GetComponent<GUIBehavior>();
            return behavior != null && behavior.IsOpen;
        }

        public void NotifyAll(string method, params object[] param)
        {
            this.ForeachUI(null, ui =>
            {
                this.Notify(ui, method, param);
                return true;
            });
        }

        public void NotifyAll(GUIGroup group, string method, params object[] param)
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

        public void Notify(GUIGroup group, string uiName, string method, params object[] param)
        {
            GameObject ui = this.FindUI(group, uiName);
            this.Notify(ui, method, param);
        }

        public void Notify(GameObject ui, string method, params object[] param)
        {
            if (ui == null)
                return;
            var behavior = ui.GetComponent<GUIBehavior>();
            if (behavior == null)
                return;
            XReflector.InvokeInstance(behavior, method, true, param);
        }

        public GameObject CreateUI(string uiName)
        {
            if (this.groups.Count == 0)
                return null;
            return this.CreateUI(this.mainGroup, uiName);
        }

        public GameObject CreateUI(GUIGroup group, string uiName)
        {
            if (group == null)
                return null;
            if (this.OnLoadPrefab == null)
                return null;
            GameObject prefab = this.OnLoadPrefab(uiName);
            if (prefab == null)
                return null;

            GameObject ui = this.CreateUI(group, prefab);
            return ui;
        }

        public GameObject CreateUI(GUIGroup group, GameObject prefab)
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

        public GameObject FindUI(GUIGroup group, string uiName)
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

        public delegate bool ForeachHandler(GameObject ui);
        public void ForeachUI(GUIGroup group, ForeachHandler handler)
        {
            if (handler == null)
                return;

            if (group != null)
            {
                for (int p = 0; p < group.transform.childCount; p++)
                {
                    Transform t = group.transform.GetChild(p);
                    if (!handler(t.gameObject))
                        return;
                }
                return;
            }

            for (int i = 0; i < this.groups.Count; i++)
            {
                Transform g = this.groups[i].transform;
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
