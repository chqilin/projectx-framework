using UnityEngine;
using System.Collections;
using System.Text;
using System.Reflection;

namespace ProjectX
{
    [HideInInspector]
    public class DebugMonitor : MonoBehaviour
    {
        private bool mIsOpen = false;
        private bool mShowCaps = false;
        private Vector2 mScrollPosition = Vector2.zero;
        private string mCapsStr = "";

        void Start()
        {
            this.mScrollPosition = Vector2.zero;
            this.mCapsStr = this.GetCaps();
        }

        void OnGUI()
        {
            float w = Screen.width / 20;
            float h = Screen.width / 20;

            GUI.color = Color.white;
            GUI.skin.verticalScrollbar.fixedWidth = w;
            GUI.skin.verticalScrollbarThumb.fixedWidth = w;
            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
            GUI.skin.button.fontSize = h > 32 ? 32 : 16;
            GUI.skin.label.fontSize = h > 48 ? 48 : h > 32 ? 32 : 16;

            if (this.mIsOpen)
            {
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");

                float x = 0;
                float y = 0;

                if (GUI.Button(new Rect(x, y, w, h), "X"))
                {
                    this.mIsOpen = false;
                }

                x += w + 10;
                this.mShowCaps = GUI.Toggle(new Rect(x, y, 200, h), this.mShowCaps, "DEV-CAPS");

                x = 0;
                y += h;
                GUILayout.BeginArea(new Rect(x, y, Screen.width, Screen.height));
                mScrollPosition = GUILayout.BeginScrollView(mScrollPosition);
                GUILayout.Label(this.GetBriefs());
                if (this.mShowCaps)
                {
                    GUILayout.Label(this.mCapsStr);
                }
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            else if (GUI.Button(new Rect(0, 0, w, h), "M"))
            {
                this.mIsOpen = true;
            }
        }

        string GetBriefs()
        {
            StringBuilder message = new StringBuilder();
            message.AppendFormat("Screen : {0} x {1} \n", Screen.width, Screen.height);
            message.AppendFormat("Time in game : {0} \n", this.TimeStr((int)Time.realtimeSinceStartup));
            message.AppendFormat("Time in level : {0} \n", this.TimeStr((int)Time.timeSinceLevelLoad));
            message.AppendFormat("Fixed FPS : {0:F2} \n", 1.0f / Time.fixedDeltaTime);
            message.AppendFormat("Smooth FPS : {0:F2} \n", 1.0f / Time.smoothDeltaTime);
            message.AppendFormat("Accurate FPS : {0:F2} \n", 1.0f / Time.deltaTime);
            return message.ToString();
        }

        string GetCaps()
        {
            StringBuilder message = new StringBuilder();
            PropertyInfo[] pList = typeof(SystemInfo).GetProperties();
            System.Array.Sort(pList, (a, b) => a.Name.CompareTo(b.Name));
            foreach (var p in pList)
            {
                message.AppendFormat("{0} : {1} \n", p.Name, p.GetValue(null, null));
            }
            return message.ToString();
        }

        string TimeStr(int time)
        {
            return string.Format("{0}:{1}:{2}",
                time / 3600,
                time % 3600 / 60,
                time % 60);
        }
    }
}

