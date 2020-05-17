using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace ProjectX
{
    public class GUIController : UIBehaviour
    {
        public System.Action onOpen = null;
        public System.Action onClose = null;

        public bool destroyOnClose = true;

        public RectTransform rectTransform
        {
            get
            {
                return this.transform as RectTransform;
            }
        }

        public XTable attribute { get; set; } = new XTable();

        #region Virtual Methods
        public virtual bool IsOpen
        {
            get { return this.gameObject.activeInHierarchy; }
        }

        public virtual void Open()
        {
            XCSharp.InvokeAction(this.onOpen);
            this.gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            if (this.destroyOnClose)
            {
                Object.Destroy(this.gameObject);
            }
            else
            {
                this.gameObject.SetActive(false);
            }

            XCSharp.InvokeAction(this.onClose);
        }
        #endregion

        #region Public Methods
        public void Goto(GameObject destUi)
        {
            if (destUi == null)
                return;
            GUIController destController = destUi.GetComponent<GUIController>();
            if (destController == null)
                return;
            this.Close();
            destController.Open();
        }
        #endregion
    }
}

