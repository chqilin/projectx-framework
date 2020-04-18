using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public class XEditorUtility
    {
        #region Scripting Symbols
        public static void DefineScriptingSymbols(BuildTargetGroup target, params string[] symbols)
        {
            foreach (var sym in symbols)
            {
                if (string.IsNullOrEmpty(sym))
                    continue;
                XEditorUtility.SetScriptingSymbol(target, sym, true);
            }
        }

        public static void UndefineScriptingSymbols(BuildTargetGroup target, params string[] symbols)
        {
            foreach (var sym in symbols)
            {
                if (string.IsNullOrEmpty(sym))
                    continue;
                XEditorUtility.SetScriptingSymbol(target, sym, false);
            }
        }

        public static string[] GetScriptingSymbol(BuildTargetGroup target)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            string[] result = symbols.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            return result;
        }

        public static void SetScriptingSymbol(BuildTargetGroup target, string symbol, bool active)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

            symbols = symbols.Replace(symbol + ";", "");
            symbols = symbols.Replace(symbol, "");
            if (active)
                symbols = symbol + ";" + symbols;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, symbols);
        }
        #endregion

        #region Selection
        public static IEnumerable<GameObject> SceneRoots
        {
            get
            {
                var prop = new HierarchyProperty(HierarchyType.GameObjects);
                var expanded = new int[0];
                while (prop.Next(expanded))
                {
                    yield return prop.pptrValue as GameObject;
                }
            }
        } 
        #endregion

        #region GUI Kit
        public static string BrouseFolderRow(string path, string label, string button, string window)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.MaxWidth(100));
            GUILayout.TextField(path);
            if (GUILayout.Button(button))
            {
                string result = UnityEditor.EditorUtility.OpenFolderPanel(window, path, Application.dataPath);
                path = string.IsNullOrEmpty(result) ? path : result;
            }
            GUILayout.EndHorizontal();
            return path;
        }

        public static int SingleSelectionStrings(int selection, string[] elements, int xcount)
        {
            xcount = Mathf.Min(elements.Length, xcount);
            selection = GUILayout.SelectionGrid(selection, elements, xcount);
            return selection;
        }

        public static T SingleSelectionEnums<T>(T selection, int xcount)
        {
            string[] names = System.Enum.GetNames(typeof(T));
            int index = System.Array.FindIndex<string>(names, name => name == selection.ToString());
            xcount = Mathf.Min(names.Length, xcount);
            index = GUILayout.SelectionGrid(index, names, xcount);
            return (T)System.Enum.Parse(typeof(T), names[index]);
        }

        public static List<int> MultipleSelectionStrings(List<int> selections, string[] elements, int xcount)
        {
            xcount = Mathf.Min(elements.Length, xcount);

            int index = 0;
            while (index < elements.Length)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < xcount; i++)
                {
                    if (index >= elements.Length)
                        break;

                    Color oldColor = GUI.color;

                    if (selections.Contains(index))
                    {
                        GUI.color = Color.gray;
                        if (GUILayout.Button(elements[index], EditorStyles.toolbarButton))
                        {
                            selections.Remove(index);
                        }
                    }
                    else
                    {
                        GUI.color = Color.white;
                        if (GUILayout.Button(elements[index], EditorStyles.toolbarButton))
                        {
                            selections.Add(index);
                        }
                    }

                    GUI.color = oldColor;

                    index++;
                }
                EditorGUILayout.EndHorizontal();
            }
            return selections;
        }

        public static List<T> MultipleSelectionEnums<T>(List<T> selections, int xcount)
        {
            string[] names = System.Enum.GetNames(typeof(T));
            System.Array values = System.Enum.GetValues(typeof(T));
            xcount = Mathf.Min(names.Length, xcount);

            int index = 0;
            while (index < names.Length)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < xcount; i++)
                {
                    if (index >= names.Length)
                        break;

                    Color oldColor = GUI.color;

                    T value = (T)values.GetValue(index);
                    if (selections.Contains(value))
                    {
                        GUI.color = Color.gray;
                        if (GUILayout.Button(names[index], EditorStyles.toolbarButton))
                        {
                            selections.Remove(value);
                        }
                    }
                    else
                    {
                        GUI.color = Color.white;
                        if (GUILayout.Button(names[index], EditorStyles.toolbarButton))
                        {
                            selections.Add(value);
                        }
                    }

                    GUI.color = oldColor;

                    index++;
                }
                EditorGUILayout.EndHorizontal();
            }
            return selections;
        } 
        #endregion
    }
}

