using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class AutoRelease : MonoBehaviour
    {
        public delegate void ReleaseFunc(GameObject gameObject);
        public float timeSpan = 5.0f;
        public ReleaseFunc releaseFunc = null;

        private float mCumulant = 0.0f;

        void Start()
        {
            this.mCumulant = 0;
        }

        void OnEnable()
        {
            this.mCumulant = 0.0f;
        }

        void Update()
        {
            this.mCumulant += Time.deltaTime;
            if (this.mCumulant >= this.timeSpan)
            {
                if (this.releaseFunc != null)
                {
                    this.releaseFunc(this.gameObject);
                }
                else
                {
                    Object.Destroy(this.gameObject);
                }
            }
        }
    }
}
