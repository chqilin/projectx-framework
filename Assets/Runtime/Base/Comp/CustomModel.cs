using System.Collections.Generic;
using UnityEngine;

namespace ProjectX
{
    public class CustomModel : MonoBehaviour
    {
        public Animator animator = null;     
        public List<Renderer> renderers = new List<Renderer>();
        public List<Collider> colliders = new List<Collider>();
        public List<Transform> locators = new List<Transform>();

        #region Renderers
        public void EnableRenderers(bool enabled)
        {
            for (int i = 0; i < this.renderers.Count; i++)
            {
                this.renderers[i].enabled = enabled;
            }
        }

        public void SetRendersLayer(int layer)
        {
            for (int i = 0; i < this.renderers.Count; i++)
            {
                this.renderers[i].gameObject.layer = layer;
            }
        }

        public void SetRendersLayer(string layer)
        {
            this.SetRendersLayer(LayerMask.NameToLayer(layer));
        }
        #endregion

        #region Colliders
        public void EnableColliders(bool enabled)
        {
            for (int i = 0; i < this.colliders.Count; i++)
            {
                this.colliders[i].enabled = enabled;
            }
        }

        public void SetCollidersLayer(int layer)
        {
            for (int i = 0; i < this.colliders.Count; i++)
            {
                this.colliders[i].gameObject.layer = layer;
            }
        }

        public void SetCollidersLayer(string layer)
        {
            this.SetCollidersLayer(LayerMask.NameToLayer(layer));
        }
        #endregion

        #region Locators
        public Transform GetLocator(string locatorName)
        {
            for (int i = 0; i < this.locators.Count; i++)
            {
                if (this.locators[i].name == locatorName)
                    return this.locators[i];
            }
            return null;
        } 
        #endregion
    }
}
