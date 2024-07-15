using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HitboxDataAsset))]
public class HitboxDataAssetInspector : Editor
{
    private HitboxDataAsset data;

    SerializedProperty _shape;
    SerializedProperty _interactionType;
    SerializedProperty _repeatPolicy;
    SerializedProperty _repeatCooldown;
    SerializedProperty _layerMask;
    SerializedProperty _duration;

    void OnEnable()
    {
        data = target as HitboxDataAsset;

        _shape = serializedObject.FindProperty("_shape");
        _interactionType = serializedObject.FindProperty("_interactionType");
        _repeatPolicy = serializedObject.FindProperty("_repeatPolicy");
        _repeatCooldown = serializedObject.FindProperty("_repeatCooldown");
        _layerMask = serializedObject.FindProperty("_layerMask");
        _duration = serializedObject.FindProperty("_duration");
    }

    public override void OnInspectorGUI()
    {
        if (_shape != null)
        {
            EditorGUILayout.PropertyField(_shape);
        }

        EditorGUILayout.PropertyField(_interactionType);
        EditorGUILayout.PropertyField(_repeatPolicy);
        if ((HitboxRepeatPolicy)_repeatPolicy.enumValueIndex == HitboxRepeatPolicy.COOLDOWN)
        {
            EditorGUILayout.PropertyField(_repeatCooldown);
        }
        EditorGUILayout.PropertyField(_layerMask);
        EditorGUILayout.PropertyField(_duration);
    }
}
