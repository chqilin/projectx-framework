using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ProjectX
{
    public class XTween : MonoBehaviour
    {
        #region Transform Tweeners
        public static void TweenPosition(Transform trans, Vector3 to, float time, System.Action onFinish = null)
        {
            XTween.Tween(trans.position, to, time, null, onFinish,
                (a, b, t) =>
                {
                    trans.position = Vector3.Lerp(a, b, t);
                });
        }

        public static void TweenRotation(Transform trans, Quaternion to, float time, System.Action onFinish = null)
        {
            XTween.Tween(trans.rotation, to, time, null, onFinish,
                (a, b, t) =>
                {
                    trans.rotation = Quaternion.Lerp(a, b, t);
                });
        }

        public static void TweenScale(Transform trans, Vector3 to, float time, System.Action onFinish = null)
        {
            XTween.Tween(trans.localScale, to, time, null, onFinish,
                (a, b, t) =>
                {
                    trans.localScale = Vector3.Lerp(a, b, t);
                });
        } 
        #endregion

        #region UI Tweeners
        public static void TweenUIColor(Graphic graphic, Color to, float time, System.Action onFinish = null)
        {
            XTween.Tween(graphic.color, to, time, null, onFinish,
                (a, b, t) =>
                {
                    graphic.color = Color.Lerp(a, b, t);
                });
        }

        public static void TweenUIAlpha(Graphic graphic, float to, float time, System.Action onFinish = null)
        {
            XTween.Tween(graphic.color.a, to, time, null, onFinish,
                (a, b, t) =>
                {
                    Color c = graphic.color;
                    c.a = Mathf.Lerp(a, b, t);
                    graphic.color = c;
                });
        }

        public static void TweenUIAlpha(CanvasGroup group, float to, float time, System.Action onFinish = null)
        {
            XTween.Tween(group.alpha, to, time, null, onFinish,
                (a, b, t) =>
                {
                    float v = Mathf.Lerp(a, b, t);
                    group.alpha = v;
                });
        } 
        #endregion

        #region Material Tweeners
        public static void TweenMaterialFloat(Material mat, string property, float to, float time, System.Action onFinish = null)
        {
            XTween.Tween(mat.GetFloat(property), to, time, null, onFinish,
                (a, b, t) =>
                {
                    float v = Mathf.Lerp(a, b, t);
                    mat.SetFloat(property, v);
                });
        }

        public static void TweenMaterialVector(Material mat, string property, Vector4 to, float time, System.Action onFinish = null)
        {
            XTween.Tween(mat.GetVector(property), to, time, null, onFinish,
                (a, b, t) =>
                {
                    Vector4 v = Vector4.Lerp(a, b, t);
                    mat.SetVector(property, v);
                });
        }

        public static void TweenMaterialColor(Material mat, string property, Color to, float time, System.Action onFinish = null)
        {
            XTween.Tween(mat.GetColor(property), to, time, null, onFinish,
                (a, b, t) =>
                {
                    Color v = Color.Lerp(a, b, t);
                    mat.SetColor(property, v);
                });
        } 
        #endregion

        #region Lowlevel Methods
        public static void Tween<T>(T from, T to, float time, System.Action onStart, System.Action onFinish, System.Action<T, T, float> onUpdate)
        {
            XTween.Instance.StartCoroutine(
                XTween.TweenAsync(from, to, time, onStart, onFinish, onUpdate));
        }

        public static IEnumerator TweenAsync<T>(T from, T to, float time, System.Action onStart, System.Action onFinish, System.Action<T, T, float> onUpdate)
        {
            if (onUpdate == null)
                yield break;

            if (onStart != null)
            {
                onStart();
            }

            time = Mathf.Max(time, 0.001f);
            float beginTime = Time.realtimeSinceStartup;

            float t = 0;
            while (t <= 1.0f)
            {
                t = (Time.realtimeSinceStartup - beginTime) / time;
                onUpdate(from, to, t);
                yield return null;
            }

            if (onFinish != null)
            {
                onFinish();
            }
        }
        #endregion

        #region Private Members
        private static XTween msInstance = null;
        private static XTween Instance
        {
            get
            {
                if (msInstance == null)
                {
                    GameObject o = new GameObject("__XTween__");
                    msInstance = o.AddComponent<XTween>();
                }
                return msInstance;
            }
        } 

        void Awake()
        {
            Object.DontDestroyOnLoad(this.gameObject);
        }
        #endregion
    }
}
