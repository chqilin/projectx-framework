using UnityEngine;
using System.Collections;

namespace ProjectX
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class SkinnedCollider : MonoBehaviour
    {
        SkinnedMeshRenderer mRenderer;
        MeshCollider mCollider;
        Mesh mMesh;

        void Awake()
        {
            this.mRenderer = this.GetComponent<SkinnedMeshRenderer>();
            this.mCollider = this.GetComponent<MeshCollider>();
            this.mMesh = new Mesh();
        }

        void Update()
        {
            if (this.mRenderer != null && this.mCollider != null)
            {
                this.mMesh.Clear();
                this.mRenderer.BakeMesh(this.mMesh);
                this.mCollider.sharedMesh = null;
                this.mCollider.sharedMesh = this.mMesh;
            }
        }

        void OnBecameInvisible()
        {
            this.enabled = false;
        }

        void OnBecameVisible()
        {
            this.enabled = true;
        }
    }
}

