using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class CameraLooker : MonoBehaviour
    {
        void Update()
        {
            if (!this.enabled || Camera.main == null)
                return;
            this.transform.LookAt(Camera.main.transform);
        }
    }
}
