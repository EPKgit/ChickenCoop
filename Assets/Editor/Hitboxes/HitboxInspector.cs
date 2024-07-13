using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Hitbox))]
public class HitboxInspector : Editor
{
    private Hitbox hitbox;

    HitboxDataAsset dataAsset;
    SerializedProperty data;

    void OnEnable()
    {
        hitbox = target as Hitbox;
        data = serializedObject.FindProperty("data");
    }

    static bool preview = false;
    public override void OnInspectorGUI()
    {
        if (data != null)
        {
            EditorGUILayout.PropertyField(data);
        }

        EditorGUI.BeginChangeCheck();
        dataAsset = EditorGUILayout.ObjectField(dataAsset, typeof(HitboxDataAsset), false) as HitboxDataAsset;
        if(EditorGUI.EndChangeCheck()) 
        {
            preview = false;
        }
        if (dataAsset != null)
        {
            preview = GUILayout.Toggle(preview, "Preview");
            if(preview)
            {
                hitbox.previewData = HitboxData.GetBuilder(dataAsset)
                    .Callback(FakeCallback)
                    .StartPosition(hitbox.transform.position)
                    .Duration(10)
                    .Finalize();
            }
            else
            {
                hitbox.previewData = null;
            }
        }
    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }

    void FakeCallback(Collider2D col, Hitbox hitbox)
    {

    }
}
