using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class VisibleLocker : MonoBehaviour
    {
        public bool visible = true;

        void Update()
        {
            if (this.GetComponent<Renderer>() != null && this.GetComponent<Renderer>().enabled != this.visible)
            {
                this.GetComponent<Renderer>().enabled = this.visible;
            }
        }
    }
}

