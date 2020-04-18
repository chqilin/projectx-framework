using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ProjectX
{
    public class ScriptingSymbolsWindow : EditorWindow
    {
        private static ScriptingSymbolsWindow msInstance = null;

        private BuildTargetGroup mBuildTarget = BuildTargetGroup.Standalone;
        private Vector2 mScrollViewForSymbols = Vector2.zero;
        private string mNewSymbol = "";

        [MenuItem("Tools/ProjectX/Scripting Symbols", priority = 1)]
        public static void EditorMain()
        {
            if (msInstance == null)
            {
                msInstance = EditorWindow.CreateInstance<ScriptingSymbolsWindow>();
                msInstance.titleContent = new GUIContent("Scripting Symbols");
                float width = 480;
                float height = 320;
                msInstance.Show();
                msInstance.position = new Rect(100, 100, width, height);
                msInstance.minSize = new Vector2(width, height);
            }

            msInstance.mBuildTarget = (BuildTargetGroup)EditorPrefs.GetInt("ProjectX.ScriptSymbols.BuildTarget", (int)BuildTargetGroup.Standalone);
        }

        void OnGUI()
        {
            EditorGUILayout.Separator();
            var target = (BuildTargetGroup)EditorGUILayout.EnumPopup("Build Target :", this.mBuildTarget);
            if (target != this.mBuildTarget)
            {
                EditorPrefs.SetInt("ProjectX.ScriptSymbols.BuildTarget", (int)target);
                this.mBuildTarget = target;
            }
            
            EditorGUILayout.Separator();
            this.mScrollViewForSymbols = EditorGUILayout.BeginScrollView(this.mScrollViewForSymbols);
            foreach (var symbol in XEditorUtility.GetScriptingSymbol(this.mBuildTarget))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.TextField(symbol);
                if(GUILayout.Button("X", GUILayout.Width(30)))
                {
                    XEditorUtility.SetScriptingSymbol(this.mBuildTarget, symbol, false);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            this.mNewSymbol = GUILayout.TextField(this.mNewSymbol);
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                string symbol = this.mNewSymbol.Trim().ToUpper();
                if (symbol.Length > 0)
                {                  
                    XEditorUtility.SetScriptingSymbol(this.mBuildTarget, symbol, true);
                    this.mNewSymbol = "";
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Separator();
        }
    }
}


