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

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        dataAsset = EditorGUILayout.ObjectField(dataAsset, typeof(HitboxDataAsset), false) as HitboxDataAsset;
        if (hitbox.data == null)
        {
            if (dataAsset != null)
            {
                hitbox.Setup(HitboxData.GetBuilder()
                    .StartPosition(hitbox.transform.position)
                    .Callback(FakeCallback)
                    .StartRotationZ(hitbox.transform.rotation.eulerAngles.z)
                    .ShapeType(dataAsset.ShapeAsset.Type)
                    .Points(dataAsset.ShapeAsset.Points)
                    .Radius(dataAsset.ShapeAsset.Radius)
                    .InteractionType(dataAsset.InteractionType)
                    .RepeatPolicy(dataAsset.RepeatPolicy)
                    .RepeatCooldown(dataAsset.RepeatCooldown)
                    .Layer(dataAsset.LayerMask)
                    .Duration(dataAsset.Duration)
                    .Finalize());
            }
        }
        else
        {
            EditorGUILayout.PropertyField(data);
        }
    }

    void FakeCallback(Collider2D col, Hitbox hitbox)
    {

    }
}
