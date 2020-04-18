using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class BillBoard : MonoBehaviour
    {
        void Update()
        {
            if (!this.enabled || Camera.main == null)
                return;
            this.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
