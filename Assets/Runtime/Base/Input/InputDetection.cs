using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace ProjectX
{
    public class InputTouch
    {
        public int fingerId = 0;

        /// <summary>
        /// touch position on screen.
        /// </summary>
        public Vector2 position = Vector2.zero;

        /// <summary>
        /// delta position between this-touch and last-touch.
        /// </summary>
        public Vector2 deltaPosition = Vector2.zero;

        /// <summary>
        /// time at this touch occurred.
        /// </summary>
        public float time = 0;

        /// <summary>
        /// elapse from last-touch to this-touch.
        /// </summary>
        public float deltaTime = 0;

        /// <summary>
        /// input ray caster for this touch.
        /// </summary>
        public InputCaster caster = null;

        /// <summary>
        /// input event source.
        /// </summary>
        public InputTarget source = null;

        /// <summary>
        /// object which is hovered by this touch.
        /// </summary>
        public InputTarget target = null;

        /// <summary>
        /// input ray.
        /// </summary>
        public Ray ray;

        /// <summary>
        /// input ray cast hit.
        /// </summary>
        public RaycastHit hit;
    }

    public class InputEvent
    {
        private System.Action<InputTouch> mOnEvent = null;

        public void Invoke(InputTouch touch)
        {
            if (this.mOnEvent != null)
            {
                this.mOnEvent(touch);
            }
        }

        public void Attach(System.Action<InputTouch> handler)
        {
            this.mOnEvent += handler;
        }
        public void Detach(System.Action<InputTouch> handler)
        {
            this.mOnEvent -= handler;
        }
    }

    public class InputDetection : MonoBehaviour
    {
        public static InputDetection instance { get; private set; }

        public static List<InputCaster> casters = new List<InputCaster>();
        public static List<InputTarget> handlers = new List<InputTarget>();

        public static bool IsOverUI(int fingerId)
        {
            if (EventSystem.current == null)
                return false;
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
            {
                return EventSystem.current.IsPointerOverGameObject(fingerId);
            }
            else
            {
                return EventSystem.current.IsPointerOverGameObject();
            }
        }

        public static bool IsOverUI(Vector2 screenpos)
        {
            if (EventSystem.current == null)
                return false;
            PointerEventData evt = new PointerEventData(EventSystem.current);
            evt.position = screenpos;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(evt, results);
            return results.Count > 0;
        }

        public static bool IsOverUI(Canvas canvas, Vector2 screenpos)
        {
            if (EventSystem.current == null)
                return false;
            PointerEventData evt = new PointerEventData(EventSystem.current);
            evt.position = screenpos;
            List<RaycastResult> results = new List<RaycastResult>();
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            raycaster.Raycast(evt, results);
            return results.Count > 0;
        }

        /// <summary>
        /// open this option will not detecte the moving(dragging) events, but it will improve the performance.
        /// </summary>
        public bool ignoreMove = false;

        /// <summary>
        /// open this option will not detecte events on ui.
        /// </summary>
        public bool ignoreUI = true;

        /// <summary>
        /// open this option will not detecte event on trigger.
        /// </summary>
        public bool ignoreTrigger = true;

        public bool checkTarget = true;

        /// <summary>
        /// minimum time in seconds for click.
        /// </summary>
        public float clickTime = 0.2f;

        /// <summary>
        /// minimum movement in pixels for click.
        /// </summary>
        public float clickDistance = 10;

        public InputEvent onPressAnyWhere = new InputEvent();
        public InputEvent onReleaseAnyWhere = new InputEvent();
        public InputEvent onMoveAnyWhere = new InputEvent();

        private bool mIsMouseDown = false;
        private Vector3 mLastMousePos = Vector3.zero;

        // private Dictionary<int, InputTouch> mPressTouches = new Dictionary<int, InputTouch>();
        // private Dictionary<int, InputTouch> mLastTouches = new Dictionary<int, InputTouch>();
        private InputTouch[] mPressTouches = new InputTouch[10];
        private InputTouch[] mLastTouches = new InputTouch[10];

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            casters.Sort((c1, c2) => c2.camera.depth.CompareTo(c1.camera.depth));
        }

        void Update()
        {
            bool hasFakePhase = false;
            TouchPhase fakePhase = TouchPhase.Began;

            if (Application.platform != RuntimePlatform.IPhonePlayer &&
                Application.platform != RuntimePlatform.Android)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    this.mIsMouseDown = true;
                    this.mLastMousePos = Input.mousePosition;
                    hasFakePhase = true;
                    fakePhase = TouchPhase.Began;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    this.mIsMouseDown = false;
                    hasFakePhase = true;
                    fakePhase = TouchPhase.Ended;
                }
                else if (this.mIsMouseDown)
                {
                    if (Input.mousePosition != this.mLastMousePos)
                    {
                        this.mLastMousePos = Input.mousePosition;
                        fakePhase = TouchPhase.Moved;
                    }
                    else
                    {
                        fakePhase = TouchPhase.Stationary;
                    }
                    
                    hasFakePhase = true;
                }
            }

            // process each touch		
            Touch[] touches = Input.touches;
            for (int i = 0; i < touches.Length; i++)
            {
                this.ProcessTouchAt(touches[i].position, touches[i].phase, touches[i].fingerId);
            }

            // process fake touch if there is one
            if (hasFakePhase)
            {
                this.ProcessTouchAt(Input.mousePosition, fakePhase, 0);
            }
        }

        private void ProcessTouchAt(Vector2 position, TouchPhase phase, int fingerId)
        {
            // Ignore stationary phase
            if (phase == TouchPhase.Stationary)
                return;

            // Ignore canceled phase
            if (phase == TouchPhase.Canceled)
                return;

            bool touchDown = phase == TouchPhase.Began;
            bool touchUp = phase == TouchPhase.Ended;

            // dont check move if not pressed.
            if (this.ignoreMove && !touchDown && !touchUp)
                return;

            // dont check ui events if press on ui
            if (this.ignoreUI && touchDown && IsOverUI(fingerId))
                return;

            // press & last touch on this finger
            InputTouch pressTouch = this.mPressTouches[fingerId];
            InputTouch lastTouch = this.mLastTouches[fingerId];

            // new touch in this frame
            InputTouch touch = new InputTouch();
            touch.fingerId = fingerId;
            touch.position = position;
            touch.deltaPosition = lastTouch != null ? position - lastTouch.position : position;
            touch.time = Time.realtimeSinceStartup;
            touch.deltaTime = lastTouch != null ? touch.time - lastTouch.time : touch.time;

            this.Raycast(position, out touch.ray, out touch.hit, out touch.caster, out touch.target);

            touch.source = touch.target;

            if (touchDown)
            {
                this.mPressTouches[touch.fingerId] = touch;

                this.TriggerEvent(this.onPressAnyWhere, touch);

                if (touch.source != null)
                {
                    this.TriggerEvent(touch.source.onPress, touch);
                }
            }
            else if (touchUp)
            {
                this.TriggerEvent(this.onReleaseAnyWhere, touch);

                if (touch.source != null)
                {
                    this.TriggerEvent(touch.source.onRelease, touch);

                    if (pressTouch != null && pressTouch.source == touch.source)
                    {
                        float timeSincePress = touch.time - pressTouch.time;
                        Vector2 moveSincePress = position - pressTouch.position;
                        if (timeSincePress <= this.clickTime && moveSincePress.sqrMagnitude <= this.clickDistance * this.clickDistance)
                        {
                            this.TriggerEvent(touch.source.onClick, touch);
                        }
                    }
                }
                this.mPressTouches[fingerId] = null;
            }
            else // move
            {
                this.TriggerEvent(this.onMoveAnyWhere, touch);

                if (touch.source != null)
                {
                    this.TriggerEvent(touch.source.onMove, touch);
                }
            }

            this.mLastTouches[touch.fingerId] = touch;
        }

        private bool Raycast(Vector3 position, out Ray ray, out RaycastHit hitInfo, out InputCaster hitCaster, out InputTarget hitTarget)
        {
            ray = default(Ray);
            hitInfo = default(RaycastHit);
            hitCaster = null;
            hitTarget = null;

            for (int i = 0; i < casters.Count; i++)
            {
                var caster = casters[i];
                if (!caster.enabled)
                    continue;

                hitCaster = caster;

                if(caster.Cast(position, 
                    out ray, 
                    out hitInfo, 
                    out hitTarget, 
                    this.checkTarget, this.ignoreTrigger))
                {
                    return true;
                }
            }
                        
            return false;
        }

        public void TriggerEvent(InputEvent evt, InputTouch touch)
        {
            evt.Invoke(touch);
        }
    }
}

