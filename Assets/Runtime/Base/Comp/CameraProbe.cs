using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class CameraProbe : MonoBehaviour
    {
        public enum TestMode
        {
            Renderer,
            Collider
        }

        public Camera target = null;      
        public TestMode mode = TestMode.Renderer;

        public delegate void Handler(CameraProbe me);
        public Handler onEnterViewField;
        public Handler onExitViewField;

        private bool mIsInViewField;

        public bool IsInViewField
        {
            get { return this.IsInCameraViewField(this.target); }
        }

        public bool IsInCameraViewField(Camera camera)
        {
            if (camera == null)
                return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return this.TestPlanesAABB(planes);
        }

        public bool IsInCameraXField(Camera camera)
        {
            if (camera == null)
                return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            Plane l = default(Plane);
            Plane r = default(Plane);
            foreach (Plane p in planes)
            {
                Vector3 pos = -p.normal * p.distance;
                if (pos.x < 0) l = p;
                if (pos.x > 0) r = p;
            }
            Plane[] vplanes = new Plane[2] { l, r };
            return this.TestPlanesAABB(vplanes);
        }

        public bool IsInCameraYField(Camera camera)
        {
            if (camera == null)
                return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            Plane t = default(Plane);
            Plane b = default(Plane);
            foreach (Plane p in planes)
            {
                Vector3 pos = -p.normal * p.distance;
                if (pos.y < 0) b = p;
                if (pos.y > 0) t = p;
            }
            Plane[] vplanes = new Plane[2] { t, b };
            return this.TestPlanesAABB(vplanes);
        }

        public bool IsInCameraZField(Camera camera)
        {
            if (camera == null)
                return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            Plane n = default(Plane);
            Plane f = default(Plane);
            foreach (Plane p in planes)
            {
                Vector3 pos = -p.normal * p.distance;
                if (pos.x == 0 && pos.y == 0)
                {
                    if (pos.z > camera.nearClipPlane)
                        f = p;
                    else
                        n = p;
                }
            }
            Plane[] vplanes = new Plane[2] { n, f };
            return this.TestPlanesAABB(vplanes);
        }

        public bool TestPlanesAABB(Plane[] planes)
        {
            if (this.mode == TestMode.Renderer && this.GetComponent<Renderer>() != null)
            {
                return GeometryUtility.TestPlanesAABB(planes, this.GetComponent<Renderer>().bounds);
            }
            else if (this.mode == TestMode.Collider && this.GetComponent<Collider>() != null)
            {
                return GeometryUtility.TestPlanesAABB(planes, this.GetComponent<Collider>().bounds);
            }
            return false;
        }

        void Start()
        {
            this.mIsInViewField = this.IsInViewField;
        }

        void Update()
        {
            if (this.onEnterViewField == null && this.onExitViewField == null)
                return;

            bool inViewField = this.IsInViewField;
            if (this.mIsInViewField != inViewField)
            {
                if (inViewField)
                {
                    if (this.onEnterViewField != null)
                    {
                        this.onEnterViewField(this);
                    }
                }
                else
                {
                    if (this.onExitViewField != null)
                    {
                        this.onExitViewField(this);
                    }
                }
            }
            this.mIsInViewField = inViewField;
        }
    }
}
