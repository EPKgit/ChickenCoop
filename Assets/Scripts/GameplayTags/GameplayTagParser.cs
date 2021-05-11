using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;

public class GameplayTagParser : EditorWindow
{
    [MenuItem("Window/GameplayTagParser")]
    public static void Init()
    {
        GetWindow(typeof(GameplayTagParser));
    }

    void OnGUI()
    {
        if (GUILayout.Button("Reimport"))
        {
            DoWork();
        }
    }

    const string XMLPath = "GameplayTags.xml";

    void DoWork()
    {
        //XmlSerializer serializer = new XmlSerializer(typeof(GameplayTags));
    }
}
