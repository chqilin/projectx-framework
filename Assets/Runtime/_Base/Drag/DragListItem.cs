using UnityEngine;
using System.Collections;

namespace ProjectX
{
    [RequireComponent(typeof(InputTarget))]
    public class DragListItem : MonoBehaviour
    {
        public System.Action<InputTouch, DragListItem> onPress = null;
        public System.Action<InputTouch, DragListItem> onClick = null;
        public System.Action<DragListItem> onRadiusChanged = null;

        [SerializeField]
        private float m_Radius = 0.5f;
        [SerializeField]
        private Vector3 m_Offset = Vector3.zero;
        [SerializeField]
        private Vector3 m_Rotation = Vector3.zero;
        [SerializeField]
        private Vector3 m_Scale = Vector3.one;
        [SerializeField]
        private bool m_CanBeDragOut = true;
        [SerializeField]
        private bool m_RemoveWhenDragOut = true;

        public int index { get; set; }

        public InputTarget inputTarget { get; private set; } 
        
        public float radius
        {
            get { return m_Radius; }
            set
            {
                if (Mathf.Approximately(m_Radius, value))
                    return;
                m_Radius = value;
                XCSharp.InvokeAction(this.onRadiusChanged, this);
            }
        }

        public Vector3 offset
        {
            get { return m_Offset; }
            set { m_Offset = value; }
        }

        public Vector3 position
        {
            get { return this.transform.position - m_Offset; }
            set { this.transform.position = value + m_Offset; }
        }

        public Vector3 localPosition
        {
            get { return this.transform.localPosition - m_Offset; }
            set { this.transform.localPosition = value + m_Offset; }
        }

        public bool canBeDragOut
        {
            get { return m_CanBeDragOut; }
            set { m_CanBeDragOut = value; }
        }

        public bool removeWhenDragOut
        {
            get { return m_RemoveWhenDragOut; }
            set { m_RemoveWhenDragOut = value; }
        }

        public void SetPRS(Vector3 origin = default(Vector3))
        {
            this.transform.localPosition = origin + this.m_Offset;
            this.transform.localRotation = Quaternion.Euler(this.m_Rotation);
            this.transform.localScale = this.m_Scale;
        }

        public void ResetPRS()
        {
            //this.transform.localPosition = Vector3.zero;
            this.transform.localRotation = Quaternion.identity;
            this.transform.localScale = Vector3.one;
        } 

        void Awake()
        {
            this.inputTarget = this.GetComponent<InputTarget>();
            this.inputTarget.onPress.Attach(this.OnPress);
            this.inputTarget.onClick.Attach(this.OnClick);
        }

        void OnDestroy()
        {
            this.inputTarget.onPress.Detach(this.OnPress);
            this.inputTarget.onClick.Detach(this.OnClick);
        }

        void OnPress(InputTouch touch)
        {
            XCSharp.InvokeAction(this.onPress, touch, this);
        }

        void OnClick(InputTouch touch)
        {
            XCSharp.InvokeAction(this.onClick, touch, this);
        }

        void OnDrawGizmos()
        {
            //Gizmos.color = new Color(0.75f, 0.75f, 1.0f, 0.25f);
            //Gizmos.DrawSphere(this.transform.position - this.offset, this.radius);
        }

        void OnDrawGizmosSelected()
        {
            //Gizmos.color = new Color(0.75f, 0.75f, 1.0f, 1.0f);
            //Gizmos.DrawSphere(this.transform.position - this.offset, this.radius);
        }
    }
}
