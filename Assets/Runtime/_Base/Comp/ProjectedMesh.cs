using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class ProjectedMesh : MonoBehaviour
    {
        public Renderer target;
        public float floatCushion = 0.005f;

        MeshFilter mMeshFilter;
        Mesh mMesh;
        Vector3[] mOriginVertices;
        Vector3[] mUpdateVertices;

        void Awake()
        {
            mMeshFilter = this.GetComponent<MeshFilter>();
            mMesh = mMeshFilter.mesh;
            mMesh.MarkDynamic();
            mOriginVertices = mMesh.vertices;
            mUpdateVertices = mMesh.vertices;
        }

        void Update()
        {
            if (this.target != null)
            {
                for (int i = 0; i < mUpdateVertices.Length; i++)
                {
                    Vector3 pos = mOriginVertices[i];
                    pos = this.transform.TransformPoint(pos);

                    Vector3 des = this.target.bounds.center;
                    Vector3 ori = pos + (des - pos).normalized * 0.001f;
                    Vector3 dir = des - ori;
                    bool cast = false;
                    foreach (RaycastHit hit in Physics.RaycastAll(ori, dir))
                    {
                        if (hit.collider.gameObject == this.target.gameObject)
                        {
                            Vector3 d = hit.point - des;
                            float f = d.magnitude + this.floatCushion;
                            pos = des + d.normalized * f;
                            cast = true;
                            break;
                        }
                    }
                    if (!cast)
                    {
                        Debug.DrawRay(ori, dir, Color.yellow);
                    }
                    pos = this.transform.InverseTransformPoint(pos);
                    mUpdateVertices[i] = pos;
                }
                mMesh.vertices = mUpdateVertices;
            }
        }

        void OnEnable() { mMesh.vertices = mOriginVertices; }
        void OnDiable() { mMesh.vertices = mOriginVertices; }

        void OnDestroy()
        {
            mMesh.vertices = mOriginVertices;
        }
    }
}

