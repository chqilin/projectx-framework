using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public class PaperDoll : MonoBehaviour
    {
        public enum Method
        {
            Dress,
            Mount
        }

        public class Part
        {
            private Method mMethod;
            private string mLocator;
            private GameObject mElement;
            private bool mChanged;

            public Part(Method method, string locator, GameObject element)
            {
                this.mMethod = method;
                this.mLocator = locator;
                this.mElement = element;
                this.mChanged = true;
            }

            public Method method
            {
                get { return this.mMethod; }
            }

            public string locator
            {
                get { return this.mLocator; }
            }

            public GameObject element
            {
                get { return this.mElement; }
                set
                {
                    if (this.mElement == value)
                        return;

                    if (this.mElement != null)
                    {
                        Object.Destroy(this.mElement);
                    }

                    this.mElement = value;

                    if (this.mElement != null)
                    {
                        this.mElement.AddComponent<StableObject>();
                        this.mElement.SetActive(false);
                        // XUtility.SetVisibleRecursively(this.mElement, false);
                    }

                    this.mChanged = true;
                }
            }

            public bool changed
            {
                get { return this.mChanged; }
                set { this.mChanged = value; }
            }
        }


        private Dictionary<int, Part> mParts = new Dictionary<int, Part>();

        public Part this[int slot]
        {
            get
            {
                Part part = null;
                this.mParts.TryGetValue(slot, out part);
                return part;
            }
            set
            {
                this.mParts[slot] = value;
            }
        }

        public bool changed
        {
            get
            {
                foreach (Part part in this.mParts.Values)
                {
                    if (part.changed == true)
                        return true;
                }
                return false;
            }
            set
            {
                foreach (Part part in this.mParts.Values)
                {
                    part.changed = value;
                }
            }
        }

        void Generate()
        {
            SkinnedMeshRenderer result = XUtility.FindOrCreateComponent<SkinnedMeshRenderer>(this);

            List<Transform> skeleton = new List<Transform>();
            this.gameObject.GetComponentsInChildren<Transform>(true, skeleton);

            List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
            foreach (Part p in this.mParts.Values)
            {
                if (p.element == null)
                    continue;

                if (p.method == Method.Dress)
                {
                    SkinnedMeshRenderer[] rs = p.element.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    renderers.AddRange(rs);
                    if (p.element.transform.parent != this.transform)
                    {
                        p.element.transform.parent = this.transform;
                    }
                }
                else
                {
                    // if p is not changed, dont destroy and attach
                    if (p.changed)
                    {
                        GameObject locator = XUtility.FindGameObjectRecursively(this.gameObject, p.locator);
                        XUtility.DestroyChildren(locator);
                        XUtility.AttachGameObject(locator, p.element);
                        p.element.SetActive(true);
                        //XUtility.SetVisibleRecursively(p.element, true);
                    }
                }
            }

            XUtility.CombineSkinnedMeshRenderer(result, skeleton, renderers);
            result.updateWhenOffscreen = true;
        }

        void Update()
        {
            if (this.changed)
            {
                this.Generate();
                this.changed = false;
            }
        }

        /*
        public GameObject Ele1;
        public GameObject Ele2;
        void Start()
        {
            this[1] = new Part(Method.Dress, "", this.Ele1);
        }
        void OnGUI()
        {
            if (GUILayout.Button(" ELE 1"))
            {
                this[1].element = this.Ele1;
            }
            if (GUILayout.Button(" ELE 2"))
            {
                this[1].element = this.Ele2;
            }
            if (GUILayout.Button(" ELE 0"))
            {
                this[1].element = null;
            }
        }
         */
    }
}
