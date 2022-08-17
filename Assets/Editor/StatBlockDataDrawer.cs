using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(StatBlock))]
public class StatBlockDataDrawer : CustomPropertyDrawerBase
{
    // 1 for foldout arrow
    // 1 for add button
    // 0.25 * 2 for dividers
    private const float EXTRA_LINES = 2.5f;

    private static StatName newKey = (StatName)0;

    private SerializedProperty statKeys;
    private SerializedProperty statVals;
    private SerializedProperty serializationOverriden;

    private bool customTargetingPrefabs;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // EditorGUI.DrawRect(position, Color.red);
        if (StartOnGUI(position, property, label))
        {
            return;
        }
        SetupProperties(property);
        DrawHorizontalLine(1, 0.25f);
        DrawDict();
        DrawHorizontalLine(1, 0.25f);
        EndOnGUI(property);
    }

    protected override void CustomRightClickMenu(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Reset"), false, () => 
        {
            SetupProperties(property);
            property.serializedObject.Update();
            statKeys.ClearArray();
            statVals.ClearArray();
            serializationOverriden.boolValue = true;
            serializationOverriden.serializedObject.ApplyModifiedProperties();
        });
        menu.ShowAsContext();
    }

    void DrawDict()
    {
        var names = Enum.GetNames(typeof(StatName));
        if (!statKeys.isArray || !statVals.isArray || statKeys.arraySize != statVals.arraySize)
        {
            return;
        }
        for (int x = 0; x < statKeys.arraySize; ++x)
        {
            var nameAtIndex = statKeys.GetArrayElementAtIndex(x);
            var valAtIndex = statVals.GetArrayElementAtIndex(x);
            var baseValueProp = valAtIndex.FindPropertyRelative("_baseValue");
            EditorGUI.BeginChangeCheck();
            Rect fullRect = NextLine();
            fullRect.width -= 20.0f;
            float f = EditorGUI.DelayedFloatField(fullRect, new GUIContent(Enum.GetName(typeof(StatName), nameAtIndex.enumValueIndex)), baseValueProp.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                baseValueProp.floatValue = f;
                serializationOverriden.boolValue = true;
                serializationOverriden.serializedObject.ApplyModifiedProperties();
            }
            fullRect.x += fullRect.width;
            fullRect.width = 20.0f;
            if(GUI.Button(fullRect, new GUIContent("-")))
            {
                statKeys.DeleteArrayElementAtIndex(x);
                statVals.DeleteArrayElementAtIndex(x);
                serializationOverriden.boolValue = true;
                serializationOverriden.serializedObject.ApplyModifiedProperties();
                SetAddEnum();
            }
        }
        DrawAddButton();
    }

    void DrawAddButton()
    {
        Rect fullRect = NextLine();
        Rect halfRect = fullRect;
        halfRect.width *= 0.5f;
        newKey = (StatName)EditorGUI.EnumPopup(halfRect, new GUIContent(), newKey, Discriminator, true);
        halfRect.x += halfRect.width;
        if(GUI.Button(halfRect, new GUIContent("Add Stat")) && Discriminator(newKey))
        {
            statKeys.InsertArrayElementAtIndex(statKeys.arraySize);
            statVals.InsertArrayElementAtIndex(statVals.arraySize);
            statKeys.GetArrayElementAtIndex(statKeys.arraySize - 1).enumValueIndex = (int)newKey;
            statVals.GetArrayElementAtIndex(statKeys.arraySize - 1).FindPropertyRelative("_baseValue").floatValue = StatBlock.defaultValues[newKey];
            serializationOverriden.boolValue = true;
            serializationOverriden.serializedObject.ApplyModifiedProperties();
            SetAddEnum();
        }
    }

    void SetupProperties(SerializedProperty prop)
    {
        statKeys = prop.FindPropertyRelative("_keys");
        statVals = prop.FindPropertyRelative("_values");
        serializationOverriden = prop.FindPropertyRelative("serializationOverriden");
        SetAddEnum();
    }

    bool Discriminator(Enum e)
    {
        if (!(e is StatName))
        {
            return false;
        }
        StatName stat = (StatName)e;
        for (var iter = statKeys.GetEnumerator(); iter.MoveNext();)
        {
            SerializedProperty prop = iter.Current as SerializedProperty;
            if (prop.enumValueIndex == (int)stat)
            {
                return false;
            }
        }
        return true;
    }

    void SetAddEnum()
    {
        int n = Enum.GetValues(typeof(StatName)).Length;
        for (int x = 0; x < n && !Discriminator(newKey); ++x)
        {
            newKey = (StatName)(((int)newKey + 1) % n);
        }
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
        num += statKeys.arraySize;
        return num;
    }
}


