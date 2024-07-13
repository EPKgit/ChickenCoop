using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using Targeting;
using Encounters;

/**********************************************************
 * 
 * CURRENT UNUSED
 *
 **********************************************************/

[CustomPropertyDrawer(typeof(HitboxShapeAsset))]
public class HitboxShapeAssetDrawer : CustomPropertyDrawerBase
{
    // 1 for foldout arrow
    // 1 for targeting type
    // 0.25 * 2 for dividers
    private const float EXTRA_LINES = 2.5f;

    private SerializedObject self;
    private SerializedProperty _type;
    private SerializedProperty _points;
    private SerializedProperty _radius;
    private SerializedProperty _length;

    protected override bool DoesExpansion() { return true; }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // EditorGUI.DrawRect(position, Color.red);
        if (StartOnGUI(position, property, label, true))
        {
            return;
        }
        SetupProperties(property);
        if(_type == null)
        {
            return;
        }
        DrawHorizontalLine(1, 0.25f);
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(NextLine(), _type);
        HitboxShapeType type = (HitboxShapeType)_type.enumValueIndex;
        if (type == HitboxShapeType.MAX)
        {
            EditorGUI.LabelField(NextLine(), "ERROR: shape is max");
        }
        switch (type)
        {
            case HitboxShapeType.PROJECTED_RECT:
                EditorGUI.PropertyField(NextLine(), _radius);
                EditorGUI.PropertyField(NextLine(), _length);
                break;
            case HitboxShapeType.SQUARE:
            case HitboxShapeType.CIRCLE:
                EditorGUI.PropertyField(NextLine(), _radius);
                break;
            case HitboxShapeType.POLYGON:
                EditorGUI.PropertyField(NextLine(), _points);
                break;
        }
        if(EditorGUI.EndChangeCheck() && self != null)
        {
            self.ApplyModifiedProperties();
        }
        DrawHorizontalLine(1, 0.25f);
        EndOnGUI(property);
    }

    void SetupProperties(SerializedProperty prop)
    {
        if (prop.objectReferenceValue == null)
        {
            _type = prop.FindPropertyRelative("_type");
            _points = prop.FindPropertyRelative("_points");
            _radius = prop.FindPropertyRelative("_radius");
            _length = prop.FindPropertyRelative("_length");
        }
        else
        {
            self = new SerializedObject(prop.objectReferenceValue);
            _type = self.FindProperty("_type");
            _points = self.FindProperty("_points");
            _radius = self.FindProperty("_radius");
            _length = self.FindProperty("_length");
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var baseHeight = base.GetPropertyHeight(property, label);
        var lines = GetNumLines(property);
        var extra = GetExtraSpace(property);
        return baseHeight * lines + extra;
    }

    protected override float GetExtraSpace(SerializedProperty property)
    {
        float extra = 0;
        if (property.isExpanded)
        {
            SetupProperties(property);
            if(_type == null)
            {
                return 0;
            }
            HitboxShapeType type = (HitboxShapeType)_type.enumValueIndex;
            if(type == HitboxShapeType.POLYGON)
            {
                extra += EditorGUI.GetPropertyHeight(_points);
            }
        }
        return extra;
    }

    protected override float GetNumLines(SerializedProperty property)
    {
        if (!property.isExpanded)
        {
            return 1;
        }
        SetupProperties(property);
        float num = EXTRA_LINES;
        if (_type == null)
        {
            return num;
        }
        HitboxShapeType type = (HitboxShapeType)_type.enumValueIndex;
        if(type == HitboxShapeType.MAX)
        {
            return num + 1;
        }
        switch (type)
        {
            case HitboxShapeType.PROJECTED_RECT:
                num += 2;
                break;
            case HitboxShapeType.SQUARE:
            case HitboxShapeType.CIRCLE:
                num += 1;
                break;
            case HitboxShapeType.POLYGON:
                break;
        }

        return num;
    }
}


