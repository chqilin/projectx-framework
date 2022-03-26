using UnityEngine;

namespace ProjectX
{
    public class GUITiltWindow : MonoBehaviour
    {
        public Vector2 range = new Vector2(5f, 3f);

        Quaternion mStart;
        Vector2 mRot = Vector2.zero;

        void Start()
        {
            this.mStart = this.transform.localRotation;
        }

        void Update()
        {
            Vector3 pos = Input.mousePosition;

            float halfWidth = Screen.width * 0.5f;
            float halfHeight = Screen.height * 0.5f;
            float x = Mathf.Clamp((pos.x - halfWidth) / halfWidth, -1f, 1f);
            float y = Mathf.Clamp((pos.y - halfHeight) / halfHeight, -1f, 1f);
            this.mRot = Vector2.Lerp(mRot, new Vector2(x, y), Time.deltaTime * 5f);

            this.transform.localRotation = mStart * Quaternion.Euler(-mRot.y * range.y, mRot.x * range.x, 0f);
        }
    }
}

