using UnityEngine;
using System.Collections;

namespace ProjectX
{
    [ExecuteInEditMode]
    public class CameraTransparencySorter : MonoBehaviour
    {
        public TransparencySortMode mode = TransparencySortMode.Default;
        public bool runOnlyOnce = true;

        void Start()
        {
            this.Reset();
        }

        void Update()
        {
            if (!this.runOnlyOnce)
            {
                this.Reset();
            }
        }

        void OnEnable()
        {
            this.Reset();
        }

        void Reset()
        {
            this.GetComponent<Camera>().transparencySortMode = this.mode;
        }
    }
}
