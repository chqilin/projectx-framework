using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    [HideInInspector]
    public class DebugConsole : MonoBehaviour
    {
        public delegate void Handler(string text);
        public Handler OnInput;
        public List<string> TextList = new List<string>();

        private bool mIsOpen = false;
        private string mText = "";
        private Vector2 mScrollPosition = Vector2.zero;

        void OnGUI()
        {
            float w = Screen.width / 20;
            float h = Screen.width / 20;

            GUI.color = Color.white;
            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
            GUI.skin.button.fontSize = h > 32 ? 32 : 16;
            GUI.skin.textField.fontSize = h > 32 ? 32 : 16;

            if (this.mIsOpen)
            {
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");

                float x = Screen.width - w;
                float y = Screen.height - h;

                if (GUI.Button(new Rect(x, y, w, h), "X"))
                {
                    this.mIsOpen = false;
                }
                x -= w;
                if (GUI.Button(new Rect(x, y, w, h), "C"))
                {
                    this.mText = "";
                }
                x -= w;
                if (GUI.Button(new Rect(x, y, w, h), "+"))
                {
                    if (!string.IsNullOrEmpty(this.mText) && !this.TextList.Contains(this.mText))
                    {
                        this.TextList.Add(this.mText);
                    }
                }
                x -= w;
                if (GUI.Button(new Rect(x, y, w, h), "O"))
                {
                    if (this.OnInput != null)
                    {
                        this.OnInput(this.mText);
                    }
                }

                this.mText = GUI.TextField(new Rect(0, y, x, h), this.mText);

                GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height - h));
                mScrollPosition = GUILayout.BeginScrollView(mScrollPosition);
                GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                foreach (string text in this.TextList)
                {
                    if (GUILayout.Button(text))
                    {
                        this.mText = text;
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.EndArea();

            }
            else
            {
                if (GUI.Button(new Rect(Screen.width - w, Screen.height - h, w, h), "C"))
                {
                    this.mIsOpen = true;
                }
            }
        }
    }
}

