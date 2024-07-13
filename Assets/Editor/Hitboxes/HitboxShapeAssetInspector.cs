using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HitboxShapeAsset), true)]
public class HitboxShapeAssetInspector : Editor
{
    private HitboxShapeAsset shapeAsset;

    private SerializedProperty _type;
    private SerializedProperty _points;
    private SerializedProperty _radius;

    void OnEnable()
    {
        shapeAsset = target as HitboxShapeAsset;
        _type = serializedObject.FindProperty("_type");
        _points = serializedObject.FindProperty("_points");
        _radius = serializedObject.FindProperty("_radius");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(_type);
        HitboxShapeType type = (HitboxShapeType)_type.enumValueIndex;
        if (type == HitboxShapeType.MAX)
        {
            EditorGUILayout.LabelField("ERROR: shape is max");
            return;
        }
        switch (type)
        {
            case HitboxShapeType.CIRCLE:
            case HitboxShapeType.SQUARE:
                Debug.Log("radius");
                EditorGUILayout.PropertyField(_radius);
                break;
            case HitboxShapeType.POLYGON:
                Debug.Log("radius");
                EditorGUILayout.PropertyField(_points);
                break;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
