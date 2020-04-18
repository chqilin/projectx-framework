using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class ObjectShaker : MonoBehaviour
    {
        public enum Axis { X, Y, Z }

        public Axis axis = Axis.Y;
        public float amplitude = 10.0f;
        public int frequency = 10;
        public float timeSpan = 2.0f;

        private Vector3 mOrigin;
        private float mCumulant;

        void Start()
        {
            this.mOrigin = this.transform.localPosition;
            this.mCumulant = 0;
        }

        void Update()
        {
            this.mCumulant += Time.deltaTime;
            float percent = this.mCumulant / this.timeSpan;
            if (percent < 1.0f)
            {
                float p = Mathf.PI * 2 * percent;
                float a = Mathf.Cos(p + Mathf.PI) * 0.5f + 0.5f;
                float t = this.amplitude * a * Mathf.Sin(this.frequency * p);
                Vector3 pos = this.mOrigin;
                if (this.axis == Axis.X) pos.x += t;
                else if (this.axis == Axis.Y) pos.y += t;
                else pos.z += t;
                this.transform.localPosition = pos;
            }
            else
            {
                this.transform.localPosition = this.mOrigin;
                this.enabled = false;
            }
        }

        void OnEnable() { this.Start(); }
        void OnDisable() { this.Start(); }
    }
}

