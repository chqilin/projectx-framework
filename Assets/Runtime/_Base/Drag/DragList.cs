using UnityEngine;
using System.Collections.Generic;

namespace ProjectX
{
    public class DragList : MonoBehaviour
    {
        public System.Action<DragList, DragListItem> onPressItem = null;
        public System.Action<DragList, DragListItem> onClickItem = null;
        public System.Action<DragList, DragListItem> onDragOut = null;

        public Transform min = null;
        public Transform max = null;
        public Transform itemRoot = null;
        public List<Collider> colliders = new List<Collider>();
        public float dragForce = 5.0f;
        public float pullLength = 1.0f;
        public float resilence = 7.5f;
        public float inertia = 0.9f;
        public float dragOut = 0.15f;

        private Vector3 mOrigin = Vector3.zero;
        private Vector3 mDirection = Vector3.zero;
        private List<DragListItem> mItems = new List<DragListItem>();
        private Transform mHead = null;
        private Transform mTail = null;

        private InputTouch mPressTouch = null;
        private DragListItem mPressItem = null;
        private Vector3 mPressDelta = Vector3.zero;
        private Vector3 mLastPoint = Vector3.zero;
        private Vector3 mLastDelta = Vector3.zero;
        private Vector3 mDestination = Vector3.zero;

        private bool mNeedLayout = true;
        private bool mNeedUpdate = true;

        #region Properties
        public List<DragListItem> items
        {
            get { return this.mItems; }
        } 

        public InputTouch pressTouch
        {
            get { return this.mPressTouch; }
        }

        public DragListItem pressItem
        {
            get { return this.mPressItem; }
        }
        #endregion

        #region Public Methods
        public void Return(DragListItem item)
        {
            if (item == null)
                return;

            int pos = item.index;
            for (int i = 0; i < this.mItems.Count; i++)
            {
                var it = this.mItems[i];
                if (it.index >= pos)
                {
                    pos = i;
                    break;
                }
            }

            this.Insert(pos, item);
        }

        public void Add(DragListItem item)
        {
            if (item == null)
                return;

            item.transform.parent = this.itemRoot;
            item.SetPRS(item.transform.localPosition);
            item.onPress = this.OnPressItem;
            item.onClick = this.OnClickItem;
            item.onRadiusChanged = this.OnItemRadiusChanged;

            Drag drag = item.GetComponent<Drag>();
            if (drag != null)
            {
                drag.StopDragging();
                drag.enabled = false;
            }

            this.mItems.Add(item);

            this.mNeedLayout = true;
        }

        public void Insert(int index, DragListItem item)
        {
            if (item == null)
                return;

            item.transform.parent = this.itemRoot;
            item.SetPRS(item.transform.localPosition);
            item.onPress = this.OnPressItem;
            item.onClick = this.OnClickItem;
            item.onRadiusChanged = this.OnItemRadiusChanged;

            Drag drag = item.GetComponent<Drag>();
            if (drag != null)
            {
                drag.StopDragging();
                drag.enabled = false;
            }

            index = index >= 0 ? index : 0;
            index = index <= this.mItems.Count ? index : this.mItems.Count;
            this.mItems.Insert(index, item);

            this.mNeedLayout = true;
        }

        public void Remove(DragListItem item)
        {
            if (item == null)
                return;
            if (!this.mItems.Remove(item))
                return;

            item.transform.parent = null;
            item.onPress = null;
            item.onRadiusChanged = null;

            Drag drag = item.GetComponent<Drag>();
            if (drag != null)
            {
                drag.enabled = true;
                drag.StartDragging(this.mPressTouch.fingerId, this.mPressTouch.caster.camera);
            }

            this.mNeedLayout = true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= this.mItems.Count)
                return;

            var item = this.mItems[index];
            this.mItems.RemoveAt(index);

            item.transform.parent = null;
            item.ResetPRS();
            item.onPress = null;
            item.onRadiusChanged = null;

            Drag drag = item.GetComponent<Drag>();
            if (drag != null)
            {
                drag.enabled = true;
                drag.StartDragging(this.mPressTouch.fingerId, this.mPressTouch.caster.camera);
            }

            this.mNeedLayout = false;
        }

        public void RefreshItemIndex()
        {
            for (int i = 0; i < this.mItems.Count; i++)
            {
                this.mItems[i].index = i;
            }
        }
        #endregion

        #region Unity Life Circle
        void Awake()
        {
            this.itemRoot.position = this.min.position;

            this.mOrigin = this.itemRoot.localPosition;
            this.mDirection = (this.max.localPosition - this.min.localPosition).normalized;

            for (int i = 0; i < this.itemRoot.childCount; i++)
            {
                Transform t = this.itemRoot.GetChild(i);
                DragListItem item = t.GetComponent<DragListItem>();
                if (item == null || !item.enabled)
                    continue;
                this.mItems.Add(item);
            }

            for (int i = 0; i < this.mItems.Count; i++)
            {
                var item = this.mItems[i];
                item.index = i;
                item.SetPRS();
                item.onPress = this.OnPressItem;
                item.onClick = this.OnClickItem;
                item.onRadiusChanged += this.OnItemRadiusChanged;
            }

            GameObject head = new GameObject("__head__");
            head.transform.parent = this.itemRoot;
            head.transform.localPosition = Vector3.zero;
            head.transform.localRotation = Quaternion.identity;
            head.transform.localScale = Vector3.one;
            this.mHead = head.transform;

            GameObject tail = new GameObject("__tail__");
            tail.transform.parent = this.itemRoot;
            tail.transform.localPosition = Vector3.zero;
            tail.transform.localRotation = Quaternion.identity;
            tail.transform.localScale = Vector3.one;
            this.mTail = tail.transform;

            InputDetection.instance.onPressAnyWhere.Attach(this.OnPressAnyWhere);
            InputDetection.instance.onMoveAnyWhere.Attach(this.OnMoveAnyWhere);
            InputDetection.instance.onReleaseAnyWhere.Attach(this.OnReleaseAnyWhere);
        }

        void OnDestroy()
        {
            Object.Destroy(this.mHead.gameObject);
            Object.Destroy(this.mTail.gameObject);

            foreach (var item in this.mItems)
            {
                item.onPress = null;
                item.onRadiusChanged = null;
            }

            InputDetection.instance.onPressAnyWhere.Detach(this.OnPressAnyWhere);
            InputDetection.instance.onMoveAnyWhere.Detach(this.OnMoveAnyWhere);
            InputDetection.instance.onReleaseAnyWhere.Detach(this.OnReleaseAnyWhere);
        }

        void Start()
        {
            this.mDestination = this.itemRoot.localPosition;
            this.LayoutItems(true);
        }

        void Update()
        {
            this.UpdatePosition();
            if (this.mPressTouch == null)
            {
                this.AdjustPositionInRange();
            }

            this.LayoutItems(false);
        }
        #endregion

        #region Private Methods
        void OnPressItem(InputTouch touch, DragListItem item)
        {
            this.mPressItem = item;
            XCSharp.InvokeAction(this.onPressItem, this, item);
        }

        void OnClickItem(InputTouch touch, DragListItem item)
        {
            XCSharp.InvokeAction(this.onClickItem, this, item);
        }

        void OnPressAnyWhere(InputTouch touch)
        {
            RaycastHit hit;
            if (this.TouchOnColliders(touch, touch, out hit))
            {
                this.mPressTouch = touch;
                this.mPressDelta = Vector3.zero;
                this.mLastPoint = this.transform.InverseTransformPoint(hit.point);
                this.mDestination = this.itemRoot.localPosition;
            }
        }

        void OnMoveAnyWhere(InputTouch touch)
        {
            if (this.mPressTouch == null || touch.fingerId != this.mPressTouch.fingerId)
                return;

            RaycastHit hit;
            if (this.TouchOnColliders(this.mPressTouch, touch, out hit))
            {
                Vector3 point = this.transform.InverseTransformPoint(hit.point);
                Vector3 delta = point - this.mLastPoint;
                if (this.Approximately(delta, Vector3.zero))
                    return;

                this.mPressDelta += delta;
                if (this.mPressItem != null &&
                    this.mPressItem.canBeDragOut &&
                    touch.target != null &&
                    touch.target == this.mPressItem.inputTarget)
                {
                    float sqrd = this.mPressDelta.sqrMagnitude;
                    float angle = Vector3.Angle(this.mPressDelta, this.mDirection);
                    if (sqrd > this.dragOut * this.dragOut && angle > 45 && angle < 135)
                    {
                        XCSharp.InvokeAction(this.onDragOut, this, this.mPressItem);
                        if(this.mPressItem.removeWhenDragOut)
                        {
                            this.Remove(this.mPressItem);
                        }
                        this.mPressTouch = null;
                        this.mPressItem = null;
                        return;
                    }
                }

                delta = this.ShadowOnNomalized(delta, this.mDirection);
                delta = this.AdjustDeltaInRange(delta);
                
                this.mLastPoint = point;
                this.mLastDelta = delta;
                this.mDestination += delta;
                this.mNeedUpdate = true;
            }
            else
            {
                if (this.mPressItem != null &&
                    this.mPressItem.canBeDragOut)
                {
                    XCSharp.InvokeAction(this.onDragOut, this, this.mPressItem);
                    if (this.mPressItem.removeWhenDragOut)
                    {
                        this.Remove(this.mPressItem);
                    }
                    this.mPressTouch = null;
                    this.mPressItem = null;
                }
            }
        }

        void OnReleaseAnyWhere(InputTouch touch)
        {
            this.mPressTouch = null;
            this.mPressItem = null;
        }

        void OnItemRadiusChanged(DragListItem item)
        {
            this.mNeedLayout = true;
        }

        void UpdatePosition()
        {
            if (!this.mNeedUpdate)
                return;

            if (this.mPressTouch == null && !this.Approximately(this.mLastDelta, Vector3.zero))
            {
                this.mLastDelta *= this.inertia;
                this.mLastDelta = this.AdjustDeltaInRange(this.mLastDelta);
                this.mDestination += this.mLastDelta;
            }

            Vector3 force = XMath.Elastic(this.mDestination, this.itemRoot.localPosition, this.dragForce, 0, 0);
            if (Mathf.Approximately(force.sqrMagnitude, 0))
            {
                this.mNeedUpdate = false;
            }

            this.AddForce(force);
        }

        Vector3 AdjustDeltaInRange(Vector3 delta)
        {
            if (this.mItems.Count == 0)
                return Vector3.zero;

            Vector3 head = this.transform.InverseTransformPoint(this.mHead.position);
            Vector3 tail = this.transform.InverseTransformPoint(this.mTail.position);

            Vector3 min = this.transform.InverseTransformPoint(this.min.position);
            Vector3 max = this.transform.InverseTransformPoint(this.max.position);

            Vector3 min_head = head - min;
            Vector3 max_tail = tail - max;

            bool head_force =
                min_head.sqrMagnitude > 0 &&
                Vector3.Angle(min_head, this.mDirection) < 90 &&
                Vector3.Angle(delta, this.mDirection) < 90;

            bool tail_force =
                max_tail.sqrMagnitude > 0 &&
                Vector3.Angle(max_tail, this.mDirection) > 90 &&
                Vector3.Angle(delta, this.mDirection) > 90;

            if (head_force != tail_force)
            {
                if (head_force)
                {
                    float shadow = Vector3.Dot(min_head, this.mDirection);
                    float percent = Mathf.Clamp(1.0f - (shadow / this.pullLength), 0.0f, 1.0f);
                    delta *= percent;
                }
                else if (tail_force)
                {
                    float shadow = -Vector3.Dot(max_tail, this.mDirection);
                    float percent = Mathf.Clamp(1.0f - (shadow / this.pullLength), 0.0f, 1.0f);
                    delta *= percent;
                }
                else
                {
                    delta = Vector3.zero;
                }
            }

            return delta;
        }

        void AdjustPositionInRange()
        {
            if (this.mItems.Count == 0)
                return;

            Vector3 head = this.transform.InverseTransformPoint(this.mHead.position);
            Vector3 tail = this.transform.InverseTransformPoint(this.mTail.position);

            Vector3 min = this.transform.InverseTransformPoint(this.min.position);
            Vector3 max = this.transform.InverseTransformPoint(this.max.position);

            if (this.mHead == this.mTail)
            {
                this.mNeedUpdate = false;
                Vector3 force = XMath.Elastic(min, head, this.resilence, 0, 0);
                this.AddForce(force);
                return;
            }

            Vector3 min_head = head - min;
            Vector3 max_tail = tail - max;

            bool head_force = min_head.sqrMagnitude > 0.01f && Vector3.Angle(min_head, this.mDirection) <= 90;
            bool tail_force = max_tail.sqrMagnitude > 0.01f && Vector3.Angle(max_tail, this.mDirection) > 90;
            if (!head_force && !tail_force)
                return;

            this.mNeedUpdate = false;

            if (head_force != tail_force)
            {
                Vector3 min_max = max - min;
                Vector3 head_tail = tail - head;
                if (head_tail.sqrMagnitude > min_max.sqrMagnitude)
                {
                    if (head_force)
                    {
                        Vector3 force = XMath.Elastic(min, head, this.resilence, 0, 0);
                        this.AddForce(force);
                    }
                    else if (tail_force)
                    {
                        Vector3 force = XMath.Elastic(max, tail, this.resilence, 0, 0);
                        this.AddForce(force);
                    }
                }
                else
                {
                    if (head_force)
                    {
                        Vector3 force = XMath.Elastic(min, head, this.resilence, 0, 0);
                        this.AddForce(force);
                    }
                }
            }
            else
            {
                Vector3 force = XMath.Elastic(min, head, this.resilence, 0, 0);
                this.AddForce(force);
            }
        }

        void LayoutItems(bool kinematic)
        {
            if (!this.mNeedLayout)
                return;

            bool completed = true;

            Vector3 pos = Vector3.zero;
            this.mHead.transform.localPosition = pos;
            for (int i = 0; i < this.mItems.Count; i++)
            {
                var cur = this.mItems[i];
                var curpos = cur.localPosition;
                pos += this.mDirection * cur.radius;
                if (kinematic)
                {
                    cur.transform.localPosition = pos;
                }
                else if (!this.Approximately(curpos, pos))
                {
                    completed = false;
                    Vector3 force = XMath.Elastic(pos, curpos, this.resilence, 0, 0);
                    this.AddForce(cur, force);
                }
                pos += this.mDirection * cur.radius;
            }
            this.mTail.transform.localPosition = pos;

            if(completed)
            {
                this.mNeedLayout = false;
            }
        }

        void AddForce(Vector3 force)
        {
            this.itemRoot.localPosition += force * Time.deltaTime;
        }
        void AddForce(DragListItem item, Vector3 force)
        {
            item.transform.localPosition += force * Time.deltaTime;
        }

        bool TouchOnColliders(InputTouch press, InputTouch touch, out RaycastHit hit)
        {
            hit = default(RaycastHit);
            hit.distance = float.MaxValue;
            if (press == null)
                return false;

            bool collided = false;
            Ray ray = press.caster.camera.ScreenPointToRay(touch.position);
            for (int i = 0; i < this.colliders.Count; i++)
            {
                Collider collider = this.colliders[i];
                if (collider == null || !collider.enabled)
                    continue;

                RaycastHit h = default(RaycastHit);
                if (!collider.Raycast(ray, out h, float.MaxValue))
                    continue;

                if (h.distance < hit.distance)
                {
                    collided = true;
                    hit = h;
                }
            }

            return collided;
        }

        bool Approximately(Vector3 a, Vector3 b)
        {
            return XMath.FloatEqual(a.x, b.x)
                && XMath.FloatEqual(a.y, b.y)
                && XMath.FloatEqual(a.z, b.z);
        }

        Vector3 ShadowOnNomalized(Vector3 dir, Vector3 normalized)
        {
            return normalized * Vector3.Dot(dir, normalized);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.25f, 0.25f, 1.0f, 0.5f);
            Gizmos.DrawSphere(this.min.position, 0.05f);
            Gizmos.DrawSphere(this.max.position, 0.05f);
            Gizmos.DrawLine(this.min.position, this.max.position);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.25f, 0.25f, 1.0f, 1.0f);
            Gizmos.DrawSphere(this.min.position, 0.05f);
            Gizmos.DrawSphere(this.max.position, 0.05f);
            Gizmos.DrawLine(this.min.position, this.max.position);
        } 
        #endregion
    }
}
