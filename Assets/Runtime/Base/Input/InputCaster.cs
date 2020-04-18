using UnityEngine;
using System.Collections;

namespace ProjectX
{
    [RequireComponent(typeof(Camera))]
    public class InputCaster : MonoBehaviour
    {
        /// <summary>
        /// Mask to be used for raycasting, you can use it to improve performance of raycasts 
        /// </summary>
        public int mask = -1;

        public new Camera camera { get; private set; }

        void Awake()
        {
            InputDetection.casters.Add(this);
            this.camera = this.GetComponent<Camera>();
        }

        void OnDestroy()
        {
            InputDetection.casters.Remove(this);
        }

        public bool Cast(Vector3 position, out Ray ray, out RaycastHit hitInfo, out InputTarget hitTarget, bool checkTarget = true, bool ignoreTrigger = false)
        {
            ray = default(Ray);
            hitInfo = default(RaycastHit);
            hitTarget = null;
            if(!this.camera)
                return false;

            ray = this.camera.ScreenPointToRay(position);
       
            int maskToUse = this.mask != -1 ? this.mask : this.camera.cullingMask;

            if (checkTarget)
            {
                RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue, maskToUse);
                System.Array.Sort(hits, (h1, h2) => h1.distance.CompareTo(h2.distance));
                for (int j = 0; j < hits.Length; j++)
                {
                    RaycastHit h = hits[j];
                    if (ignoreTrigger && h.collider.isTrigger)
                        continue;
                    InputTarget target = h.collider.GetComponentInParent<InputTarget>();
                    if (target == null || !target.enabled)
                        continue;
                    hitInfo = h;
                    hitTarget = target;
                    Debug.DrawLine(this.transform.position, h.point, Color.green, 0.5f);
                    return true;
                }
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, maskToUse))
                {
                    InputTarget target = hit.collider.GetComponentInParent<InputTarget>();
                    hitInfo = hit;
                    hitTarget = target != null && target.enabled ? target : null;
                    Debug.DrawLine(this.transform.position, hit.point, Color.red, 0.5f);
                    return true;
                }
            }
            
            return false;
        }
    }
}
