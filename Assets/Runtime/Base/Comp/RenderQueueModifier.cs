using UnityEngine;
using System.Collections;

namespace ProjectX
{
    [ExecuteInEditMode]
    public class RenderQueueModifier : MonoBehaviour
    {
        public int renderQueue = 2000;
        public bool resetNow = false;

        void Start()
        {
            this.Set(this.renderQueue);
        }

        void Update()
        {
            if (this.resetNow)
            {
                this.Reset();
                this.resetNow = false;
            }
        }

        void OnDestroy()
        {
            this.Reset();
        }

        public void Set(int rq)
        {
            Renderer renderer = this.GetComponent<Renderer>();
            if (renderer == null)
                return;
            foreach (Material m in renderer.sharedMaterials)
            {
                m.renderQueue = rq;
            }
        }

        public void Reset()
        {
            Renderer renderer = this.GetComponent<Renderer>();
            if (renderer == null)
                return;
            foreach (Material m in renderer.sharedMaterials)
            {
                m.renderQueue = m.shader.renderQueue;
            }
        }
    }
}

