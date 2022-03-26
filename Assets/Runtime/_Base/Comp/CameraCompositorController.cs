using UnityEngine;
using System.Collections;

namespace ProjectX
{
    [ExecuteInEditMode]
    public class CameraCompositorController : MonoBehaviour
    {
        public CameraCompositor compositor;
        public Material material;
        public Color color;

        void Start()
        {
            if (this.compositor == null && Camera.main != null)
            {
                this.compositor = Camera.main.GetComponent<CameraCompositor>();
            }
            if (this.compositor != null)
            {
                this.material = this.compositor.material;
                this.color = this.compositor.color;
            }
        }

        void Update()
        {
            if (this.compositor == null || !this.compositor.enabled)
                return;
            if (this.compositor.material != this.material)
            {
                this.compositor.material = this.material;
            }
            if (this.compositor.color != this.color)
            {
                this.compositor.color = this.color;
            }
        }
    }
}
