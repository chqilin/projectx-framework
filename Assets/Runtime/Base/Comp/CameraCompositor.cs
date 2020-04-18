using UnityEngine;
using System.Collections;

namespace ProjectX
{
    [ExecuteInEditMode]
    public class CameraCompositor : MonoBehaviour
    {
        public Material material;
        public Color color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

        void Update()
        {
            if (this.material != null)
            {
                this.material.SetColor("_ColorTint", this.color);
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (material == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            Graphics.Blit(source, destination, material);
        }

        public void Blit(RenderTexture rt, Material material, int pass)
        {
            Graphics.SetRenderTarget(rt);
            this.RenderFullscreenQuad(material, pass);
        }

        public void RenderFullscreenQuad(Material material, int pass)
        {
            GL.PushMatrix();
            GL.LoadOrtho();
            material.SetPass(pass);
            GL.Begin(GL.QUADS);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
            GL.End();
            GL.PopMatrix();
        }
    }
}
