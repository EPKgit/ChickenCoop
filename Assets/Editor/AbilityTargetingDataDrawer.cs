using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using Targeting;

[CustomPropertyDrawer(typeof(Targeting.AbilityTargetingData))]
public class AbilityTargetingDataDrawer : CustomPropertyDrawerBase
{
    // 1 for foldout arrow
    // 1 for targeting type
    // 1 for custom prefab checkbox
    // 0.25 * 2 for dividers
    private const float EXTRA_LINES = 3.5f;

    private SerializedProperty targetType;
    private SerializedProperty rangePreviewPrefab;
    private SerializedProperty secondaryPreviewPrefab;
    private SerializedProperty preview;
    private SerializedProperty previewSecondary;
    private SerializedProperty range;
    private SerializedProperty previewScale;
    private SerializedProperty affiliation;

    private bool customTargetingPrefabs;

    protected override bool DoesExpansion() { return true; }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // EditorGUI.DrawRect(position, Color.red);
        if (StartOnGUI(position, property, label))
        {
            return;
        }
        SetupProperties(property);
        DrawHorizontalLine(1, 0.25f);
        EditorGUI.PropertyField(NextLine(), targetType);
        customTargetingPrefabs |= secondaryPreviewPrefab.objectReferenceValue != null;
        customTargetingPrefabs |= EditorGUI.Toggle(NextLine(), "Custom Prefabs", customTargetingPrefabs);
        TargetType type = (TargetType)targetType.enumValueIndex;
        if(type != TargetType.NONE)
        {
            EditorGUI.PropertyField(NextLine(), range);
        }
        switch((TargetType)targetType.enumValueIndex)
        {
            case TargetType.LINE_TARGETED:
            {
                if (customTargetingPrefabs)
                {
                    EditorGUI.PropertyField(NextLine(), secondaryPreviewPrefab, new GUIContent("Line Prefab"));
                }
                previewScale.vector3Value = new Vector3(EditorGUI.DelayedFloatField(NextLine(), "Width", previewScale.vector3Value.x), 0, 0);
                break;
            } 
            case TargetType.GROUND_TARGETED:
            {
                if (customTargetingPrefabs)
                {
                    EditorGUI.PropertyField(NextLine(), secondaryPreviewPrefab, new GUIContent("Target Prefab"));
                }
                EditorGUI.PropertyField(NextLine(), previewScale);
                break;
            } 
            case TargetType.ENTITY_TARGETED:
            {
                if (customTargetingPrefabs)
                {
                    EditorGUI.PropertyField(NextLine(), secondaryPreviewPrefab, new GUIContent("Crosshair Prefab"));
                }
                EditorGUI.PropertyField(NextLine(), affiliation);
                break;
            } 
        }
        DrawHorizontalLine(1, 0.25f);
        EndOnGUI(property);
    }

    void SetupProperties(SerializedProperty prop)
    {
        targetType = prop.FindPropertyRelative("targetType");
        rangePreviewPrefab = prop.FindPropertyRelative("rangePreviewPrefab");
        secondaryPreviewPrefab = prop.FindPropertyRelative("secondaryPreviewPrefab");
        preview = prop.FindPropertyRelative("preview");
        previewSecondary = prop.FindPropertyRelative("previewSecondary");
        range = prop.FindPropertyRelative("range");
        previewScale = prop.FindPropertyRelative("previewScale");
        affiliation = prop.FindPropertyRelative("affiliation");
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * GetNumLines(property);
    }

    protected override float GetNumLines(SerializedProperty property)
    {
        if (!property.isExpanded)
        {
            return 1;
        }
        SetupProperties(property);
        float num = EXTRA_LINES;
        if(targetType == null)
        {
            return num;
        }
        TargetType type = (TargetType)targetType.enumValueIndex;
        if (type != TargetType.NONE)
        {
            num += 1;
        }
        switch(type)
        {
            case TargetType.LINE_TARGETED:
                num += customTargetingPrefabs ? 2 : 1;
                break;
            case TargetType.GROUND_TARGETED:
                num += customTargetingPrefabs ? 2 : 1;
                break;
            case TargetType.ENTITY_TARGETED:
                num += customTargetingPrefabs ? 2 : 1;
                break;
        }
        
        return num;
    }
}


