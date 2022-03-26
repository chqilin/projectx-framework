using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class CanvasGroupFade : MonoBehaviour
    {
        #region Static Methods
        public static void FadeAlpha(MonoBehaviour holder, CanvasGroup group, float to, float time, System.Action onComplete = null)
        {
            group.gameObject.SetActive(true);
            holder.StartCoroutine(CanvasGroupFade.FadeAlphaAsync(group, to, time, onComplete));
        }

        public static IEnumerator FadeAlphaAsync(CanvasGroup group, float to, float time, System.Action onComplete = null)
        {
            time = Mathf.Max(time, 0.001f);
            float beginTime = Time.realtimeSinceStartup;
            float from = group.alpha;

            float t = 0;
            while (t <= 1.0f)
            {
                t = (Time.realtimeSinceStartup - beginTime) / time;
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            if (Mathf.Abs(group.alpha) < 0.001f)
            {
                group.gameObject.SetActive(false);
            }

            if (onComplete != null)
            {
                onComplete();
            }
        } 
        #endregion

        [SerializeField]
        private CanvasGroup canvasGroup = null;

        public float alpha
        {
            get { return this.canvasGroup.alpha; }
            set { this.canvasGroup.alpha = value; }
        }

        public void FadeAlpha(float to, float time, System.Action onComplete = null)
        {
            CanvasGroupFade.FadeAlpha(this, this.canvasGroup, to, time, onComplete);
        }

        void Awake()
        {
            if (this.canvasGroup == null)
                this.canvasGroup = this.GetComponent<CanvasGroup>();
        }
    }
}

