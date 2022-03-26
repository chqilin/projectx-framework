using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectX
{
    public class GUIManager : MonoBehaviour
    {
        public delegate GameObject LoadPrefabFunc(string uiName);
        public LoadPrefabFunc OnLoadPrefab = null;

        public Canvas canvasMain = null;
        public List<Canvas> canvasList = new List<Canvas>();

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

        public GameObject OpenUI(int canvasIndex, string uiName, XTable attr = null)
        {
            if (canvasIndex < 0 || canvasIndex >= this.canvasList.Count)
                return null;

            Canvas canvas = this.canvasList[canvasIndex];
            GameObject ui = this.FindUI(canvas, uiName);
            if (ui == null)
            {
                ui = this.CreateUI(canvas, uiName);
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

        public void CloseUI(Canvas canvas, string uiName)
        {
            GameObject ui = this.FindUI(canvas, uiName);
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

        public void CloseAll(Canvas canvas = null)
        {
            this.ForeachUI(canvas, ui =>
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

        public void NotifyAll(Canvas canvas, string method, params object[] param)
        {
            this.ForeachUI(canvas, ui =>
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

        public void Notify(Canvas canvas, string uiName, string method, params object[] param)
        {
            GameObject ui = this.FindUI(canvas, uiName);
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
            if (this.canvasMain == null)
                return null;
            return this.CreateUI(this.canvasMain, uiName);
        }

        public GameObject CreateUI(Canvas canvas, string uiName)
        {
            if (canvas == null)
                return null;
            if (this.OnLoadPrefab == null)
                return null;
            GameObject prefab = this.OnLoadPrefab(uiName);
            if (prefab == null)
                return null;

            GameObject ui = this.CreateUI(canvas, prefab);
            return ui;
        }

        public GameObject CreateUI(Canvas canvas, GameObject prefab)
        {
            GameObject ui = Object.Instantiate(prefab) as GameObject;
            if (ui == null)
                return null;
            ui.name = prefab.name;
            ui.transform.SetParent(canvas.transform, false);
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

        public GameObject FindUI(Canvas canvas, string uiName)
        {
            string uiNameClone = XUtility.AddCloneMarkForName(uiName);
            GameObject result = null;
            this.ForeachUI(canvas, ui =>
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
        public void ForeachUI(Canvas canvas, ForeachHandler handler)
        {
            if (handler == null)
                return;

            if (canvas != null)
            {
                for (int p = 0; p < canvas.transform.childCount; p++)
                {
                    Transform t = canvas.transform.GetChild(p);
                    if (!handler(t.gameObject))
                        return;
                }
                return;
            }

            for (int i = 0; i < this.canvasList.Count; i++)
            {
                Transform g = this.canvasList[i].transform;
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
