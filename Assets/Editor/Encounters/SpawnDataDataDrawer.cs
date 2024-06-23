using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using Targeting;
using Encounters;
using static Codice.CM.WorkspaceServer.DataStore.WkTree.WriteWorkspaceTree;

[CustomPropertyDrawer(typeof(Encounters.SpawnData))]
public class SpawnDataDrawer : CustomPropertyDrawerBase
{
    // 1 for foldout arrow
    // 1 for spawn type type
    // 0.25 * 2 for dividers
    private const float EXTRA_LINES = 2.5f;

    private SerializedProperty spawnType;
    private SerializedProperty spawnCount;
    private SerializedProperty spawnPoints;
    private SerializedProperty spawnArea;

    protected override bool DoesExpansion() { return true; }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SetupProperties(property);
        if (StartOnGUI(position, property, label))
        {
            return;
        }
        DrawHorizontalLine(1, 0.25f);
        EditorGUI.PropertyField(NextLine(), spawnType, new GUIContent("Type"));
        SpawnType type = (SpawnType)spawnType.enumValueIndex;
        bool doCount = true;
        if (type == SpawnType.MAX)
        {
            EditorGUI.LabelField(NextLine(), "SPAWN WILL NOT FUNCTION ON MAX");
            doCount = false;
        }
        switch (type)
        {
            case SpawnType.USE_GENERAL_RANDOM:
                break;
            case SpawnType.SINGLE_POINT:
                //do array handling but only display first
                if(spawnPoints.arraySize != 1)
                {
                    spawnPoints.ClearArray();
                    spawnPoints.InsertArrayElementAtIndex(0);
                }
                EditorGUI.BeginChangeCheck();
                Vector2 point = spawnPoints.GetArrayElementAtIndex(0).vector2Value;
                point = EditorGUI.Vector2Field(NextLine(), new GUIContent("Point"), point);
                if (EditorGUI.EndChangeCheck())
                {
                    spawnPoints.GetArrayElementAtIndex(0).vector2Value = point;
                    spawnPoints.serializedObject.ApplyModifiedProperties();
                }
                doCount = false;
                break;
            case SpawnType.RANDOM_IN_AREA:
                NextLine(0.25f);
                EditorGUI.PropertyField(NextLine(), spawnArea, new GUIContent("Area"));
                NextLine(1.25f);
                break;
            case SpawnType.SEQUENTIAL_OVER_POINTS:
            case SpawnType.RANDOM_OVER_POINTS:
                EditorGUI.PropertyField(NextLine(spawnPoints), spawnPoints);
                break;
        }

        if (doCount)
        {
            EditorGUI.PropertyField(NextLine(), spawnCount, new GUIContent("Count"));
        }
        DrawHorizontalLine(1, 0.25f);
        EndOnGUI(property);
    }

    void SetupProperties(SerializedProperty prop)
    {
        spawnType = prop.FindPropertyRelative("spawnType");
        spawnCount = prop.FindPropertyRelative("spawnCount");
        spawnPoints = prop.FindPropertyRelative("spawnPoints");
        spawnArea = prop.FindPropertyRelative("spawnArea");
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var lines = GetNumLines(property);
        var baseHeight = base.GetPropertyHeight(property, label);
        var extra = GetExtraSpace(property);
        return baseHeight * lines + extra;
    }

    protected override float GetExtraSpace(SerializedProperty property)
    {
        float extra = 0;
        if (property.isExpanded)
        {
            SetupProperties(property);
            SpawnType type = (SpawnType)spawnType.enumValueIndex;
            switch (type)
            {
                case SpawnType.SEQUENTIAL_OVER_POINTS:
                case SpawnType.RANDOM_OVER_POINTS:
                    if (spawnPoints.isExpanded)
                    {
                        extra += EditorGUI.GetPropertyHeight(spawnPoints);
                    }
                    else
                    {
                        extra += 1;
                    }
                    break;
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
        SpawnType type = (SpawnType)spawnType.enumValueIndex;
        switch (type)
        {
            case SpawnType.USE_GENERAL_RANDOM:
                num += 1; // just count
                break;
            case SpawnType.SINGLE_POINT:
                num += 1; // just location
                break;
            case SpawnType.RANDOM_IN_AREA:
                num += 3.5f; // rect=2 + 0.5 spacing + count
                break;
            case SpawnType.SEQUENTIAL_OVER_POINTS:
            case SpawnType.RANDOM_OVER_POINTS:
                num += 1; // count
                if(!spawnPoints.isExpanded)
                {
                    num += 1;
                }
                break;
            case SpawnType.MAX:
                num += 1; //error label
                break;
        }

        return num;
    }
}


