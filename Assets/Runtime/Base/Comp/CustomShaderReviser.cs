using UnityEngine;
using System.Collections;

namespace ProjectX
{
    /// <summary>
    /// If your game-object in assetbundle referenced a custom shader.
    /// You should use this script to fix custom shader missing.
    /// </summary>
    public class CustomShaderReviser : MonoBehaviour
    {
        void Start()
        {
            Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (r == null)
                    continue;
                Material[] materials = r.sharedMaterials;
                foreach (Material m in materials)
                {
                    if (m == null)
                        continue;
                    m.shader = Shader.Find(m.shader.name);
                }
            }
            Object.Destroy(this);
        }
    }
}


