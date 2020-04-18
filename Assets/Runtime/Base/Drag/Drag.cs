using System.Collections.Generic;
using UnityEngine;

namespace ProjectX
{
    [RequireComponent(typeof(InputTarget))]
    /// <summary>
    /// Component to allow for easy and flexible implementation of dragging objects. It uses colliders as surfaces to drag the object over. If there is a rigidbody on the object physics will be respected.
    /// </summary>
    public class Drag : MonoBehaviour
    {
        /// <summary>
        /// How a collider should be chosen to move over
        /// </summary>
        public enum MoveMode
        {
            /// <summary>
            /// When dragging the object will move over the closest (if any) collider in the colliders list
            /// </summary>
            CollidersPreferClosest,
            /// <summary>
            /// When dragging the object will move over the collider (if any) that has the lowest index in the colliders list
            /// </summary>
            CollidersPreferIndex
        }

        /// <summary>
        /// Dragging was started
        /// </summary>
        public event System.Action<Drag> onDragStarted = null;

        /// <summary>
        /// Dragging was stopped
        /// </summary>    
        public event System.Action<Drag> onDragStopped = null;

        /// <summary>
        /// Object is being dragged, also dispatched if dragged distance is 0
        /// </summary>
        public event System.Action<Drag> onDrag = null;

        /// <summary>
        /// Object is being dragged and updated on FixedUpdate, will happen if a RigidBody is attached to the object
        /// </summary>
        public event System.Action<Drag> onFixedDrag = null;

        [Header("Basic Settings:")]

        /// <summary>
        /// If starting and stopping of dragging should be handled by this component itself. If not StartDragging and StopDragging should be used.
        /// </summary>
        public bool autoStartAndStop = true;

        [Header("Raycast Settings:")]

        /// <summary>
        /// How a collider should be chosen to move over
        /// </summary>
        public MoveMode moveMode;

        /// <summary>
        /// If mask should be used in raycast
        /// </summary>    
        public bool useRaycastMask = false;

        /// <summary>
        /// Mask to use for raycasting of the colliders (so not for input)
        /// </summary>    
        public LayerMask raycastMask;

        /// <summary>
        /// Maximum distance raycasts will be done for input, can affect performance if there are a lot of distant colliders that have the same mask
        /// </summary>
        public float maxRaycastDistance = float.MaxValue;

        /// <summary>
        /// Colliders this object should move over while dragging
        /// </summary>
        public List<Collider> colliders = null;

        [Header("Screen Offset Settings:")]

        /// <summary>
        /// If the object should stay the same distance from the finger as it was when it was first touched
        /// </summary>
        public bool useScreenOffset = true;

        /// <summary>
        /// The offset on screen.
        /// </summary>
        public Vector2 offsetOnScreen = Vector2.zero;

        [Header("Dragging Settings:")]

        /// <summary>
        /// If useScreenOffset is false, the time it takes for the object to move towards the finger. Note that this basically works like reducing the offset and not like easing the following.
        /// </summary>
        public float moveToFingerTime = 0.4f;

        /// <summary>
        /// If the object should be prevented from going off the screen, the camera the object was touched on first will be used as reference.
        /// </summary>
        public bool offscreenPrevention = true;

        /// <summary>
        /// If the object should align with the surface of the collider it is being dragged over.
        /// </summary>
        public bool alignWithTargetNormal = false;

        /// <summary>
        /// Maximum speed at which the object is allowed to move by dragging. Can be used to prevent quick dragging through objects
        /// </summary>    
        public float maxDragVelocity = 222f;
        /// <summary>
        /// Maximum speed at which the object is allowed to move after release, is never higher than maxDragVelocity
        /// </summary>    
        public float maxReleaseVelocity = 10f;

        /// <summary>
        /// Time in seconds touch movement is recorded to determine the release velocity. Wrong values will result in seemingly wrong directions after release.
        /// </summary>    
        public float dragStatisticTime = 0.33f;

        /// <summary>
        /// Maximum speed at which the object is allowed to move after release, is never higher than maxDragVelocity
        /// </summary>    
        public float idleDeceleration = 10f;

        /// <summary>
        /// Velocity used after releasing, is being decelerated using idleDeceleration every Update
        /// </summary>    
        public Vector3 idleVelocity;

        /// <summary>
        /// The move to target factor. Whorks when using physics drag.
        /// </summary>
        public float moveToTargetFactor = 0.18f;

        /// <summary>
        /// The move to speed factor. Whorks when using physics drag.
        /// </summary>
        public float moveToSpeedFactor = 0.25f;

        private Camera cameraToUse = null;
        private int fingerId = -1;
        private Vector3 offset = Vector3.zero;
        private bool adjustedOffestOnScreen = false;
        private float moveToFingerSpeed = 0f;
        private List<GameObject> listeners = new List<GameObject>();
        private DragStatistics statistics = null;
        private Collider currentlyOverCollider = null;
        private InputTarget inputTarget = null;

        private bool doFixedUpdate = false;
        private Vector3 fixedUpdatePosition = Vector3.zero;

        public void StartDragging(int _fingerId, Camera _cameraToUse)
        {
            fingerId = _fingerId;
            cameraToUse = _cameraToUse;

            Vector3 inputpos = this.GetInputPosition(this.fingerId);
            Vector3 screenpos = cameraToUse.WorldToScreenPoint(transform.position);

            offset = inputpos - screenpos;
            offset.x -= offsetOnScreen.x;
            offset.y -= offsetOnScreen.y;

            moveToFingerSpeed = offset.magnitude / moveToFingerTime;

            if (onDragStarted != null)
            {
                onDragStarted(this);
            }
        }

        public void StopDragging()
        {
            if (fingerId == -1)
                return;

            fingerId = -1;

            Vector3 newVelocity = statistics.GetResult();

            statistics.Reset();

            Rigidbody rigidbody = this.GetComponent<Rigidbody>();
            if (rigidbody != null && !rigidbody.isKinematic)
            {
                doFixedUpdate = false;
            }
            else
            {
                newVelocity = newVelocity.normalized * Mathf.Clamp(newVelocity.magnitude, 0, maxDragVelocity);
                idleVelocity = newVelocity;
            }

            if (onDragStopped != null)
            {
                onDragStopped(this);
            }
        }

        public void StopDraggingImmediately()
        {
            this.fingerId = -1;

            this.idleVelocity = Vector3.zero;

            Rigidbody rigidbody = this.GetComponent<Rigidbody>();
            if (rigidbody != null && !rigidbody.isKinematic)
            {
                doFixedUpdate = false;
                rigidbody.AddForce(-rigidbody.velocity, ForceMode.VelocityChange);
            }

            if (onDragStopped != null)
            {
                onDragStopped(this);
            }
        }

        public void UseAlternativeTarget(InputTarget alternativeTarget)
        {
            if (alternativeTarget == null)
                return;

            inputTarget.onPress.Detach(this.OnPress);

            inputTarget = alternativeTarget;
            inputTarget.GetComponent<Collider>().enabled = true;
            inputTarget.onPress.Attach(this.OnPress);
        }

        /// <summary>
        /// Get current collider the object is being dragged over (if any)
        /// </summary>
        public Collider GetCurrentCollider()
        {
            return currentlyOverCollider;
        }

        void Awake()
        {
            statistics = new DragStatistics(dragStatisticTime);

            inputTarget = this.GetComponent<InputTarget>();
            inputTarget.onPress.Attach(this.OnPress);
            InputDetection.instance.onReleaseAnyWhere.Attach(this.OnReleaseAnywhere);

            if (adjustedOffestOnScreen)
            {
                offsetOnScreen *= Screen.height / 768.0f;
            }
        }

        void OnDestroy()
        {
            inputTarget.onPress.Detach(this.OnPress);
            InputDetection.instance.onReleaseAnyWhere.Detach(this.OnReleaseAnywhere);
        }

        // Update is called once per frame
        void Update()
        {
            doFixedUpdate = false;

            if (fingerId > -1)
            {
                // Fix for 4 finger gesture on iOS where no release is
                // being received from Unity
#if UNITY_EDITOR
#else
			    bool isStillTouched = false;

			    foreach(Touch touch in Input.touches)
			    {
				    if(touch.fingerId == fingerId)
				    {
					    isStillTouched = true;
				    }
			    }
			
			    if(!isStillTouched)
			    {
				    StopDragging();
                    return;
			    }
#endif

                if (!useScreenOffset)
                {
                    offset = Vector3.MoveTowards(offset, new Vector3(-offsetOnScreen.x, -offsetOnScreen.y, 0), moveToFingerSpeed * Time.deltaTime);
                }

                Vector3 inputpos = this.GetInputPosition(this.fingerId);
                Vector3 newpos = Vector3.zero;

                Vector3 raycastPos = inputpos - offset;
                raycastPos.x = Mathf.Clamp(raycastPos.x, 0, Screen.width);
                raycastPos.y = Mathf.Clamp(raycastPos.y, 0, Screen.height);

                Ray ray = cameraToUse.ScreenPointToRay(raycastPos);
                int maskToUse = useRaycastMask ? (int)this.raycastMask : cameraToUse.cullingMask;
                List<RaycastHit> hits = new List<RaycastHit>(Physics.RaycastAll(ray, maxRaycastDistance, maskToUse));
                hits.RemoveAll(hit => !colliders.Contains(hit.collider));
                if (hits.Count == 0)
                {
                    currentlyOverCollider = null;
                    return;
                }

                HitSorter.colliders = colliders;
                HitSorter.moveMode = moveMode;
                HitSorter.cameraToUse = cameraToUse;
                List<RaycastHit> sortedHits = HitSorter.Sort(hits);

                RaycastHit selectedHit = sortedHits[0];
                newpos = selectedHit.point;
                currentlyOverCollider = selectedHit.collider;
                if (alignWithTargetNormal)
                {
                    transform.forward = selectedHit.normal;
                }

                if (offscreenPrevention)
                {
                    Vector3 viewPortPosition = cameraToUse.WorldToViewportPoint(newpos);
                    viewPortPosition.x = Mathf.Clamp(viewPortPosition.x, 0, 1f);
                    viewPortPosition.y = Mathf.Clamp(viewPortPosition.y, 0, 1f);
                    newpos = cameraToUse.ViewportToWorldPoint(viewPortPosition);
                }

                Vector3 delta = newpos - transform.position;
                Vector3 newVelocity = delta / Time.deltaTime;
                newVelocity = newVelocity.normalized * Mathf.Clamp(newVelocity.magnitude, 0, maxDragVelocity);
                statistics.Record(newVelocity, Time.deltaTime);

                Rigidbody rigidbody = this.GetComponent<Rigidbody>();
                if (rigidbody == null || rigidbody.isKinematic)
                {
                    Vector3 velocity = (newpos - transform.position) / Time.deltaTime;
                    float speed = Mathf.Clamp(velocity.magnitude, 0f, maxDragVelocity);
                    transform.position += velocity.normalized * speed * Time.deltaTime;
                }
                else
                {
                    doFixedUpdate = true;
                    fixedUpdatePosition = newpos;
                }

                if (onDrag != null)
                {
                    onDrag(this);
                }
            }
            else
            {
                Rigidbody rigidbody = this.GetComponent<Rigidbody>();
                if (rigidbody == null || rigidbody.isKinematic)
                {
                    float speed = idleVelocity.magnitude;
                    speed = Mathf.MoveTowards(speed, 0f, idleDeceleration * Time.deltaTime);
                    idleVelocity = idleVelocity.normalized * speed;

                    if (!Mathf.Approximately(speed, 0))
                    {
                        transform.position += idleVelocity * Time.deltaTime;
                    }
                }
            }
        }

        void FixedUpdate()
        {
            if (!doFixedUpdate)
                return;

            Rigidbody rigidbody = this.GetComponent<Rigidbody>();
            if (rigidbody != null && !rigidbody.isKinematic)
            {
                Vector3 targetPos = Vector3.Lerp(transform.position, fixedUpdatePosition, moveToTargetFactor);
                Vector3 delta = targetPos - transform.position;

                Vector3 newVelocity = delta / Time.deltaTime;
                newVelocity = Vector3.Lerp(rigidbody.velocity, newVelocity, moveToSpeedFactor);
                rigidbody.AddForce(newVelocity - rigidbody.velocity, ForceMode.VelocityChange);

                if (onFixedDrag != null)
                {
                    onFixedDrag(this);
                }
            }
        }

        void OnPress(InputTouch touch)
        {
            if (this.enabled && this.autoStartAndStop)
            {
                if (this.fingerId == -1)
                {
                    this.StartDragging(touch.fingerId, touch.caster.camera);
                }
            }
        }

        void OnReleaseAnywhere(InputTouch touch)
        {
            if (autoStartAndStop)
            {
                if (this.fingerId == touch.fingerId)
                {
                    this.StopDragging();
                }
            }
        }

        Vector3 GetInputPosition(int finger)
        {
            if (Application.isEditor)
                return Input.mousePosition;

            foreach (Touch touch in Input.touches)
            {
                if (touch.fingerId == finger)
                    return touch.position;
            }

            Debug.LogError("Drag.GetInputPosition(): No touch found with finger id " + finger);
            return Vector3.zero;
        }

        private class DragStatistics
        {
            private List<Vector3> mVelocities = new List<Vector3>();
            private List<float> mTimes = new List<float>();
            private float mMaxTime = 0;
            private float mAccTime = 0;

            public List<Vector3> velocities
            {
                get { return mVelocities; }
            }

            public DragStatistics(float _maxTime)
            {
                this.mMaxTime = _maxTime;
                this.Reset();
            }

            public void Reset()
            {
                mVelocities.Clear();
                mTimes.Clear();
                mAccTime = 0f;
            }

            // Update is called once per frame
            public void Record(Vector3 velocity, float deltaTime)
            {
                mVelocities.Add(velocity);
                mTimes.Add(deltaTime);

                mAccTime += deltaTime;
                while (mAccTime > mMaxTime)
                {
                    mVelocities.RemoveAt(0);
                    mAccTime -= mTimes[0];
                    mTimes.RemoveAt(0);
                }
            }

            public Vector3 GetResult()
            {
                Vector3 newVelocity = Vector3.zero;

                for (int i = 0; i < mVelocities.Count; i++)
                {
                    newVelocity += mVelocities[i];
                }

                if (mVelocities.Count > 0)
                {
                    newVelocity /= mVelocities.Count;
                }

                return newVelocity;
            }
        }

        private class HitSorter
        {
            public static Camera cameraToUse;
            public static MoveMode moveMode;
            public static List<Collider> colliders;

            private static float distanceA;
            private static float distanceB;
            private static int indexA;
            private static int indexB;
            private static Vector3 cameraPosition;

            public static List<RaycastHit> Sort(List<RaycastHit> hits)
            {
                List<RaycastHit> result = new List<RaycastHit>(hits);
                switch (moveMode)
                {
                    case MoveMode.CollidersPreferClosest:
                        cameraPosition = cameraToUse.transform.position;
                        result.Sort(SortClosest);
                        break;
                    case MoveMode.CollidersPreferIndex:
                        result.Sort(SortIndex);
                        break;
                }

                return result;
            }

            static int SortClosest(RaycastHit a, RaycastHit b)
            {
                distanceA = Vector3.Distance(a.point, cameraPosition);
                distanceB = Vector3.Distance(b.point, cameraPosition);
                
                if (distanceA < distanceB)
                    return -1;
                else if (distanceB > distanceA)
                    return 1;
                else
                    return 0;
            }

            static int SortIndex(RaycastHit a, RaycastHit b)
            {
                indexA = colliders.IndexOf(a.collider);
                indexB = colliders.IndexOf(b.collider);

                if (indexA < indexB)
                    return -1;
                else if (indexB > indexA)
                    return 1;
                else
                    return 0;
            }
        }
    }
}
