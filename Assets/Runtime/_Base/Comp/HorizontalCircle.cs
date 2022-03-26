using UnityEngine;
using System.Collections;

namespace ProjectX
{
    [RequireComponent(typeof(LineRenderer))]
    public class HorizontalCircle : MonoBehaviour
    {
        public float radius = 1.0f;
        public Color color = Color.white;

        private Vector3[] mVertices = new Vector3[361];
        private LineRenderer mLineRenderer = null;

        void Start()
        {
            this.mLineRenderer = this.GetComponent<LineRenderer>();
            if(this.mLineRenderer == null)
                this.mLineRenderer = this.gameObject.AddComponent<LineRenderer>();
            this.mLineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            this.mLineRenderer.useWorldSpace = false;
            this.mLineRenderer.startWidth = 0.2f;
            this.mLineRenderer.endWidth = 0.2f;
            this.mLineRenderer.startColor = this.color;
            this.mLineRenderer.endColor = this.color;
            this.mLineRenderer.positionCount = this.mVertices.Length;
        }

        void Update()
        {
            for (int angle = 0; angle < 361; angle++)
            {
                float x = this.radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float z = this.radius * Mathf.Sin(angle * Mathf.Deg2Rad);
                float y = this.transform.localPosition.y + 1;
                this.mVertices[angle] = new Vector3(x, y, z);
            }
            for (int i = 0; i < this.mVertices.Length; i++)
            {
                this.mLineRenderer.SetPosition(i, this.mVertices[i]);
            }
        }
    }
}
