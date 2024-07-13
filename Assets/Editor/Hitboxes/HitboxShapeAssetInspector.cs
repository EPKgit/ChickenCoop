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
    private SerializedProperty _length;

    void OnEnable()
    {
        shapeAsset = target as HitboxShapeAsset;
        _type = serializedObject.FindProperty("_type");
        _points = serializedObject.FindProperty("_points");
        _radius = serializedObject.FindProperty("_radius");
        _length = serializedObject.FindProperty("_length");
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
            case HitboxShapeType.PROJECTED_RECT:
                EditorGUILayout.PropertyField(_length);
                EditorGUILayout.PropertyField(_radius);
                break;
            case HitboxShapeType.SQUARE:
            case HitboxShapeType.CIRCLE:
                EditorGUILayout.PropertyField(_radius);
                break;
            case HitboxShapeType.POLYGON:
                EditorGUILayout.PropertyField(_points);
                break;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
