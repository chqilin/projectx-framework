using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ProjectX
{
    public class StableObject : MonoBehaviour
    {
        public bool isStable = true;

        void Awake()
        {           
            SceneManager.sceneUnloaded += this.OnSceneUnloaded;
            if (this.isStable)
            {
                Object.DontDestroyOnLoad(this.gameObject);
            }
        }

        void OnDestroy()
        {
            SceneManager.sceneUnloaded -= this.OnSceneUnloaded;
        }

        void OnSceneUnloaded(Scene scene)
        {
            if (!this.isStable)
            {
                Object.Destroy(this.gameObject);
            }
        }
    }
}

