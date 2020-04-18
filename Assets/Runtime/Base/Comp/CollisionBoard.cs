using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshCollider))]
    public class CollisionBoard : MonoBehaviour
    {
        public bool closeToL = true;
        public bool closeToR = true;
        public bool closeToU = true;
        public bool closeToD = true;

        private MeshCollider mCollider = null;

        public void Apply()
        {
            this.mCollider.sharedMesh.triangles = this.BuildTriangles();
        }

        void Awake()
        {
            this.mCollider = this.GetComponent<MeshCollider>();

            Mesh mesh = new Mesh();
            mesh.vertices = this.BuildVertices();
            mesh.triangles = this.BuildTriangles();
            mesh.MarkDynamic();
            this.mCollider.sharedMesh = mesh;
        }

        Vector3[] BuildVertices()
        {
            Vector3[] verts = new Vector3[]
            {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f,  0.5f),
            new Vector3(-0.5f, -0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f,  0.5f),
            };
            return verts;
        }

        int[] BuildTriangles()
        {
            List<int> tris = new List<int>();
            tris.AddRange(new int[] { 0, 4, 5, 0, 5, 1 }); // b
            tris.AddRange(new int[] { 2, 6, 7, 2, 7, 3 }); // f
            if (this.closeToL)
                tris.AddRange(new int[] { 1, 5, 6, 1, 6, 2 }); // r
            if (this.closeToR)
                tris.AddRange(new int[] { 3, 7, 4, 3, 4, 0 }); // l
            if (this.closeToD)
                tris.AddRange(new int[] { 4, 7, 6, 4, 6, 5 }); // u
            if (this.closeToU)
                tris.AddRange(new int[] { 0, 1, 2, 0, 2, 3 }); // d
            return tris.ToArray();
        }

        void OnDrawGizmosSelected()
        {
            Color cWire = new Color(1.0f, 1.0f, 0.0f, 1.0f);
            Color cCore = new Color(1.0f, 1.0f, 0.0f, 0.7f);

            Gizmos.matrix = this.transform.localToWorldMatrix;
            Gizmos.color = cWire;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.color = cCore;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;
        }

        void OnDrawGizmos()
        {
            Color cWire = new Color(1.0f, 1.0f, 0.0f, 1.0f);
            Color cCore = new Color(1.0f, 1.0f, 0.0f, 0.2f);

            Gizmos.matrix = this.transform.localToWorldMatrix;
            Gizmos.color = cWire;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.color = cCore;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
