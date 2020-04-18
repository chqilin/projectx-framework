using UnityEngine;
using System.Collections.Generic;

namespace ProjectX
{
    public class BezierPathSampler : MonoBehaviour
    {
        public Transform target = null;
        public List<Transform> path = new List<Transform>();
        public float speed = 1.0f;
        public bool lookOnPath = true;

        private List<Vector3> mRealPath = new List<Vector3>();
        private int mIndex = 0;
        private float mLerpT = 0.0f;

        public void Start()
        {
            if(this.target == null)
            {
                this.target = this.transform;
            }

            this.mRealPath.Clear();
            if (this.path.Count >= 3)
            {
                for (int i = 0; i < this.path.Count - 1; i++)
                {
                    Vector3 p0 = this.path[i].position;
                    Vector3 p1 = this.path[i + 1].position;

                    this.mRealPath.Add((p0 + p1) * 0.5f);
                    this.mRealPath.Add(p1);
                }
                {
                    Vector3 pn = this.path[this.path.Count - 1].position;
                    Vector3 p0 = this.path[0].position;
                    this.mRealPath.Add((pn + p0) * 0.5f);
                    this.mRealPath.Add(p0);
                }
            }

            this.speed = Mathf.Max(this.speed, 0.0001f);

            this.mIndex = 0;         
            this.mLerpT = 0.0f;
        }

        void Update()
        {
            if (this.target == null)
                return;

            int count = this.mRealPath.Count;
            if (count < 3)
                return;

            Vector3 p0 = mRealPath[(mIndex + 0) % count];
            Vector3 p1 = mRealPath[(mIndex + 1) % count];
            Vector3 p2 = mRealPath[(mIndex + 2) % count];

            float time = ((p1 - p0).magnitude + (p2 - p1).magnitude) / this.speed;
            time = Mathf.Max(time, 0.0001f);

            this.mLerpT += (Time.deltaTime / time);
            while (this.mLerpT > 1.0f)
            {
                this.mLerpT -= 1.0f;
                this.mIndex += 2;
            }

            Vector3 pos = this.SampleBezierCurve(p0, p1, p2, mLerpT);

            this.target.position = pos;

            if(this.lookOnPath)
            {
                Vector3 dir = (p1 + (p2 - p1) * this.mLerpT) - (p0 + (p1 - p0) * this.mLerpT);
                this.target.LookAt(pos + dir);
            }
        }

        Vector3 SampleBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return (1 - t) * (1 - t) * p0 + 2 * t * (1 - t) * p1 + t * t * p2;
        }

        void OnDrawGizmos()
        {
            int count = this.mRealPath.Count;
            if (count < 3)
                return;

            float time = 1.0f;
            float t = 0;
            int i = 0;

            Gizmos.color = Color.white;
            while (i <= mRealPath.Count)
            {
                Vector3 p0 = mRealPath[(i + 0) % count];
                Vector3 p1 = mRealPath[(i + 1) % count];
                Vector3 p2 = mRealPath[(i + 2) % count];

                t += (Time.deltaTime / time);
                if (t > 1.0f)
                {
                    t = 0.0f;
                    i += 2;
                }

                Vector3 pos = XMath.SampleBezierCurve(p0, p1, p2, t);
                Vector3 dir = (p1 + (p2 - p1) * t) - (p0 + (p1 - p0) * t);
                dir = dir.normalized * 0.5f;
                Gizmos.DrawLine(pos - dir, pos + dir);

                Gizmos.DrawSphere(p1, 0.1f);
                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}
