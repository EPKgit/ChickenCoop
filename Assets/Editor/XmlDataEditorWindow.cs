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
    }
}
