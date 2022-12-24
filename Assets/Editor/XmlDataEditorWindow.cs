using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class XmlDataEditorWindow : EditorWindow
{

    [MenuItem("Window/XmlDataEditorWindow")]
    public static void Init()
    {
        GetWindow(typeof(XmlDataEditorWindow));
    }

    void OnGUI()
    {
        if (GUILayout.Button("Reimport Ability XML"))
        {
            AbilityDataXMLParser.instance.ForceReimport();
        }

        if (GUILayout.Button("Reimport Tag XML"))
        {
            GameplayTagInternals.GameplayTagXMLParser.instance.ForceReimport();
#if UNITY_EDITOR_WIN
            GameplayTagInternals.GameplayTagXMLParser.instance.GenerateTagEnumFile();
#endif
        }
    }
}
