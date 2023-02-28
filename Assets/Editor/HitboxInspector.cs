using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Hitbox))]
public class HitboxInspector : Editor
{
    private Hitbox hb;

    HitboxDataAsset dataAsset;
    SerializedProperty data;

    void OnEnable()
    {
        hb = target as Hitbox;
        data = serializedObject.FindProperty("data");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        dataAsset = EditorGUILayout.ObjectField(dataAsset, typeof(HitboxDataAsset), false) as HitboxDataAsset;
        if (hb.data == null)
        {
            if (dataAsset != null)
            {
                hb.Setup(HitboxData.GetBuilder()
                    .StartPosition(hb.transform.position)
                    .Callback(FakeCallback)
                    .StartRotationZ(hb.transform.rotation.eulerAngles.z)
                    .Shape(dataAsset.Shape)
                    .Points(dataAsset.Points)
                    .InteractionType(dataAsset.InteractionType)
                    .RepeatPolicy(dataAsset.RepeatPolicy)
                    .RepeatCooldown(dataAsset.RepeatCooldown)
                    .Layer(dataAsset.LayerMask)
                    .Radius(dataAsset.Radius)
                    .Duration(dataAsset.Duration)
                    .Finalize());
            }
        }
        else
        {
            //EditorGUILayout.PropertyField(data);
        }
    }

    void FakeCallback(Collider2D col)
    {

    }
}
