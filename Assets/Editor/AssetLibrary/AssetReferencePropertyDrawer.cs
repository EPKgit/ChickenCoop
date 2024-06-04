using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AssetLibrary.AssetReference))]
public class AssetReferencePropertyDrawer : CustomPropertyDrawerBase
{
    private SerializedProperty name;
    private SerializedProperty type;
    private SerializedProperty prefab;
    private SerializedProperty icon;

    private List<string> assetTypeNames;

    protected override bool DoesExpansion() { return false; }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //EditorGUI.DrawRect(position, Color.red);
        if (StartOnGUI(position, property, label))
        {
            return;
        }
        SetupProperties(property);
        DrawHorizontalLine(1, 0.25f);
        NextLine();

        //type
        GetAssetTypeNames();
        rect.width *= 0.2f;
        type.enumValueIndex = EditorGUI.Popup(rect, type.enumValueIndex, assetTypeNames.ToArray());

        // lookup value
        rect.x += rect.width;
        DrawLookupValue();

        //reference
        rect.x += rect.width;
        rect.width = rect.width * 3f;
        DrawAssetValue();

        DrawHorizontalLine(1, 0.25f);

        EndOnGUI(property);
    }

    void DrawLookupValue()
    {
        switch (type.enumValueIndex)
        {
            case (int)AssetLibrary.AssetReference.AssetType.GAMEOBJECT:
            case (int)AssetLibrary.AssetReference.AssetType.ICON:
                name.stringValue = EditorGUI.DelayedTextField(rect, name.stringValue);
                break;
        }
    }

    void DrawAssetValue()
    {
        switch (type.enumValueIndex)
        {
            case (int)AssetLibrary.AssetReference.AssetType.GAMEOBJECT:
                EditorGUI.ObjectField(rect, prefab, GUIContent.none);
                break;
            case (int)AssetLibrary.AssetReference.AssetType.ICON:
                EditorGUI.ObjectField(rect, icon, GUIContent.none);
                break;
        }
    }

    void GetAssetTypeNames()
    {
        if(assetTypeNames != null)
        {
            return;
        }
        assetTypeNames = new List<String>(Enum.GetNames(typeof(AssetLibrary.AssetReference.AssetType)));
    }

    void SetupProperties(SerializedProperty prop)
    {
        name = prop.FindPropertyRelative("name");
        type = prop.FindPropertyRelative("type");
        prefab = prop.FindPropertyRelative("reference");
        icon = prop.FindPropertyRelative("icon");
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * GetNumLines(property);
    }

    protected override float GetNumLines(SerializedProperty property)
    {
        return 1.5f;
    }
}