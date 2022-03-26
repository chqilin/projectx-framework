using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class ScreenAnchor : MonoBehaviour
    {
        public enum HAlignment
        {
            Free, Left, Center, Right
        }

        public enum VAlignment
        {
            Free, Top, Center, Bottom
        }

        public enum DAlignment
        {
            Free, Specific
        }

        public enum RotationMode
        {
            Free, Orthographic, Perspective
        }

        [Header("Basic Settings:")]
        public new Camera camera = null;
        public Vector2 standardResolution = new Vector2(1024, 768);

        [Header("H-V Settings:")]
        public HAlignment hAlignment = HAlignment.Free;
        public VAlignment vAlignment = VAlignment.Free;
        public Vector2 offset = Vector2.zero;

        [Header("Depth Settings:")]
        public DAlignment dAlignment = DAlignment.Free;
        public float specificDepth = 10;
        
        [Header("Rotation Settings:")]
        public RotationMode rotaionMode = RotationMode.Free;
        public bool runOnlyOnce = false;

        private Vector3 mPoint = Vector2.zero;

        void Start()
        {
            if (this.camera == null)
            {
                this.camera = Camera.main;
            }
            this.Anchor();
        }

        void Update()
        {
            if (this.runOnlyOnce)
                return;
            this.Anchor();
        }

        void Anchor()
        {
            if (this.camera == null)
                return;

            Rect viewport = this.camera.pixelRect;

            Vector3 freePoint = Vector3.zero;
            if(this.hAlignment == HAlignment.Free || this.vAlignment == VAlignment.Free)
            {
                freePoint = this.camera.WorldToScreenPoint(this.transform.position);
            }

            if (this.hAlignment == HAlignment.Free)
                this.mPoint.x = freePoint.x;
            else if (this.hAlignment == HAlignment.Left)
                this.mPoint.x = viewport.xMin;
            else if (this.hAlignment == HAlignment.Right)
                this.mPoint.x = viewport.xMax;
            else if (this.hAlignment == HAlignment.Center)
                this.mPoint.x = viewport.center.x;

            if (this.vAlignment == VAlignment.Free)
                this.mPoint.y = freePoint.y;
            else if (this.vAlignment == VAlignment.Bottom)
                this.mPoint.y = viewport.yMin;
            else if (this.vAlignment == VAlignment.Top)
                this.mPoint.y = viewport.yMax;
            else if (this.vAlignment == VAlignment.Center)
                this.mPoint.y = viewport.center.y;

            // Modified : adapt to different resolution.
            float factor = viewport.height / standardResolution.y;
            this.mPoint.x += this.offset.x * factor;
            this.mPoint.y += this.offset.y * factor;

            Transform ct = this.camera.transform;

            // plane
            Vector3 pn = -ct.forward;
            Vector3 pp = this.transform.position;
            if(this.dAlignment == DAlignment.Specific)
            {
                pp = ct.position + ct.forward * this.specificDepth;
            }

            // ray
            Ray ray = this.camera.ScreenPointToRay(this.mPoint);
            Vector3 rp = ray.origin;
            Vector3 rn = ray.direction;

            // intersection-point of plane & ray
            float t = ((pn.x * pp.x + pn.y * pp.y + pn.z * pp.z) - (pn.x * rp.x + pn.y * rp.y + pn.z * rp.z)) / (pn.x * rn.x + pn.y * rn.y + pn.z * rn.z);
            Vector3 pos = ray.GetPoint(t);

            // position
            this.transform.position = pos;

            // rotation
            if (this.rotaionMode == RotationMode.Orthographic)
            {
                Vector3 dst = pos + ct.forward;
                this.transform.LookAt(dst, ct.up);
            }
            else if (this.rotaionMode == RotationMode.Perspective)
            {
                Vector3 dst = pos + (pos - ct.position);
                this.transform.LookAt(dst, ct.up);
            }
        }
    }
}
