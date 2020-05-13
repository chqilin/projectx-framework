using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectX
{
    public enum GUILayer
    {
        Background,
        Normal,
        Overlay
    }

    public class GUIGroup : MonoBehaviour
    {
        public GUILayer layer = GUILayer.Normal;
    }
}
