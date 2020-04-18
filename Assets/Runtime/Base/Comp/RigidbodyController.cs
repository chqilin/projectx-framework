using UnityEngine;
using System.Collections;

namespace ProjectX
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyController : MonoBehaviour
    {
        public float speed = 5.0f;
        public float gravity = 10.0f;
        public float maxVelocityChange = 10.0f;
        public float jumpHeight = 3.0f;
        public bool openAutoInputDetection = true;

        private Vector3 mMomentum = Vector3.zero;
        private bool mGrounded = false;

        #region Unity Life Circle
        void Awake()
        {
            GetComponent<Rigidbody>().freezeRotation = true;
            GetComponent<Rigidbody>().useGravity = false;
        }

        void Update()
        {
            if (this.openAutoInputDetection)
            {
                float x = Input.GetAxis("Horizontal");
                float y = Input.GetAxis("Vertical");
                Vector2 v = new Vector2(x, y).normalized * this.speed;
                this.Move(v.x, v.y);

                if (Input.GetButtonDown("Jump"))
                {
                    this.Jump();
                }
            }
        }

        void FixedUpdate()
        {
            if (!XMath.FloatEqual(this.mMomentum, Vector3.zero))
            {
                Vector3 velocity = GetComponent<Rigidbody>().velocity;
                Vector3 velocityChange = this.mMomentum - velocity;
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.VelocityChange);
            }

            // We apply gravity manually for more tuning control
            GetComponent<Rigidbody>().AddForce(new Vector3(0, -gravity * GetComponent<Rigidbody>().mass, 0));

            mGrounded = false;
        }

        void OnCollisionStay()
        {
            mGrounded = true;
        }
        #endregion

        #region Public Methods
        public void Move(float x, float z)
        {
            this.mMomentum.x = x;
            this.mMomentum.z = z;
        }

        public void Jump()
        {
            float speed = Mathf.Sqrt(2 * jumpHeight * gravity);
            this.CastImpulseForce(Vector3.up, speed);
        }

        public void CastImpulseForce(Vector3 dir, float speed)
        {
            float m = this.GetComponent<Rigidbody>().mass;
            Vector3 v = dir.normalized * speed;
            Vector3 f = v * m;
            this.GetComponent<Rigidbody>().AddForce(f, ForceMode.Impulse);
        }
        #endregion
    }
}
