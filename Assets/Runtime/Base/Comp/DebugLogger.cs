using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    [HideInInspector]
    public class DebugLogger : MonoBehaviour
    {
        struct Message
        {
            public LogType Type;
            public string Log;
            public string Stack;
        }

        bool mIsOpen = false;
        bool mShowStack = false;
        bool mShowInfo = false;
        bool mShowWarn = false;
        bool mShowError = true;
        List<Message> mMessages = new List<Message>();
        Vector2 mScrollPosition = Vector2.zero;

        void Awake()
        {
            Application.logMessageReceived += this.LogCallback;
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= this.LogCallback;
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

                float x = Screen.width - w;
                float y = 0;

                if (GUI.Button(new Rect(x, y, w, h), "X"))
                {
                    this.mIsOpen = false;
                }
                x -= w;
                if (GUI.Button(new Rect(x, y, w, h), "C"))
                {
                    this.mMessages.Clear();
                }
                x -= w;
                if (GUI.Button(new Rect(x, y, w, h), this.mShowStack ? "S" : "s"))
                {
                    this.mShowStack = !this.mShowStack;
                }
                x -= w;
                if (GUI.Button(new Rect(x, y, w, h), this.mShowInfo ? "I" : "i"))
                {
                    this.mShowInfo = !this.mShowInfo;
                }
                x -= w;
                GUI.color = Color.yellow;
                if (GUI.Button(new Rect(x, y, w, h), this.mShowWarn ? "W" : "w"))
                {
                    this.mShowWarn = !this.mShowWarn;
                }
                x -= w;
                GUI.color = Color.red;
                if (GUI.Button(new Rect(x, y, w, h), this.mShowError ? "E" : "e"))
                {
                    this.mShowError = !this.mShowError;
                }

                GUILayout.BeginArea(new Rect(0, h, Screen.width, Screen.height - h));
                mScrollPosition = GUILayout.BeginScrollView(mScrollPosition);
                this.ShowMessage();
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            else
            {
                if (GUI.Button(new Rect(Screen.width - w, 0, w, h), "L"))
                {
                    this.mIsOpen = true;
                }
            }
        }

        void LogCallback(string log, string stack, LogType type)
        {
            if (type == LogType.Exception) type = LogType.Error;
            Message m = new Message();
            m.Type = type;
            m.Log = log;
            m.Stack = stack;
            this.mMessages.Add(m);
        }

        void ShowMessage()
        {
            foreach (Message m in this.mMessages)
            {
                if (this.mShowStack)
                {
                    switch (m.Type)
                    {
                        case LogType.Log:
                            if (this.mShowInfo)
                            {
                                GUI.color = Color.white;
                                GUILayout.Label(m.Log + "\n" + m.Stack);
                            }
                            break;
                        case LogType.Warning:
                            if (this.mShowWarn)
                            {
                                GUI.color = Color.yellow;
                                GUILayout.Label(m.Log + "\n" + m.Stack);
                            }
                            break;
                        case LogType.Error:
                            if (this.mShowError)
                            {
                                GUI.color = Color.red;
                                GUILayout.Label(m.Log + "\n" + m.Stack);
                            }
                            break;
                    }
                }
                else
                {
                    switch (m.Type)
                    {
                        case LogType.Log:
                            if (this.mShowInfo)
                            {
                                GUI.color = Color.white;
                                GUILayout.Label(m.Log);
                            }
                            break;
                        case LogType.Warning:
                            if (this.mShowWarn)
                            {
                                GUI.color = Color.yellow;
                                GUILayout.Label(m.Log);
                            }
                            break;
                        case LogType.Error:
                            if (this.mShowError)
                            {
                                GUI.color = Color.red;
                                GUILayout.Label(m.Log);
                            }
                            break;
                    }
                }
            }
        }
    }
}

