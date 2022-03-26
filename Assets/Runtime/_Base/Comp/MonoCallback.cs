using UnityEngine;

namespace ProjectX
{
    public class MonoCallback : MonoBehaviour
    {
        public string userState = "";

        public System.Action onAwake;
        void Awake()
        {
            XCSharp.InvokeAction(this.onAwake);
        }

        public System.Action onStart;
        void Start()
        {
            XCSharp.InvokeAction(this.onStart);
        }

        public System.Action onUpdate;
        void Update()
        {
            XCSharp.InvokeAction(this.onUpdate);
        }

        public System.Action onDestroy;
        void OnDestroy()
        {
            XCSharp.InvokeAction(this.onDestroy);
        }

        public System.Action onEnable;
        void OnEnable()
        {
            XCSharp.InvokeAction(this.onEnable);
        }

        public System.Action onDisable;
        void OnDisable()
        {
            XCSharp.InvokeAction(this.onDisable);
        }

        public System.Action<Collision> onCollisionEnter;
        void OnCollisionEnter(Collision collision)
        {
            XCSharp.InvokeAction(this.onCollisionEnter, collision);
        }

        public System.Action<Collision> onCollisionExit;
        void OnCollisionExit(Collision collision)
        {
            XCSharp.InvokeAction(this.onCollisionExit, collision);
        }

        public System.Action<Collision> onCollisionStay;
        void OnCollisionStay(Collision collision)
        {
            XCSharp.InvokeAction(this.onCollisionStay, collision);
        }

        public System.Action<Collider> onTriggerEnter;
        void OnTriggerEnter(Collider c)
        {
            XCSharp.InvokeAction(this.onTriggerEnter, c);
        }

        public System.Action<Collider> onTriggerExit;
        void OnTriggerExit(Collider c)
        {
            XCSharp.InvokeAction(this.onTriggerExit, c);
        }

        public System.Action<Collider> onTriggerStay;
        void OnTriggerStay(Collider c)
        {
            XCSharp.InvokeAction(this.onTriggerStay, c);
        }

        public System.Action<GameObject> onParticleCollision;
        void OnParticleCollision(GameObject other)
        {
            XCSharp.InvokeAction(this.onParticleCollision, other);
        }
    }
}

