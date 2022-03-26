using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class ParabolaSampler : MonoBehaviour
    {
        public delegate void Handler();
        public Handler OnStart;
        public Handler OnStop;

        public float speed = 7.5f;
        public float height = 5;
        public Vector3 destination = Vector3.zero;

        private Vector3 mP1 = Vector3.zero;
        private Vector3 mP2 = Vector3.zero;
        private Vector3 mP = Vector3.zero;
        private float mA = 0;
        private float mB = 0;
        private float mT = 0;

        private float mCumulant = 0;

        void Awake()
        {
            this.enabled = false;
        }

        void Start()
        {
            this.mP1 = this.transform.position;
            this.mP2 = this.destination;
            this.mP = this.mP2 - this.mP1;
            XMath.ComputeParabola(this.mP.x, this.mP.y, this.height,
                out this.mA, out this.mB);

            this.mT = this.mP.magnitude / this.speed;
            this.mCumulant = 0;
            if (this.OnStart != null)
            {
                this.OnStart();
            }
        }

        void Update()
        {
            this.mCumulant += Time.smoothDeltaTime;
            float t = Mathf.Clamp(this.mCumulant / this.mT, 0, 1);

            Vector3 pos = this.transform.position;
            pos.x = Mathf.Lerp(0, this.mP.x, t);
            pos.y = XMath.SampleParabola(pos.x, this.mA, this.mB, 0);
            pos.z = Mathf.Lerp(0, this.mP.z, t);
            this.transform.position = this.mP1 + pos;

            if (Mathf.Approximately(t, 1))
            {
                this.enabled = false;
                if (this.OnStop != null)
                {
                    this.OnStop();
                }
            }
        }

        void OnEnable() { this.Start(); }
    }
}

