using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AssetLibrary.AssetReference))]
public class AssetReferencePropertyDrawer : CustomPropertyDrawerBase
{
    private SerializedProperty name;
    private SerializedProperty prefab;

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
        rect.width *= 0.33f;
        name.stringValue = EditorGUI.DelayedTextField(rect, name.stringValue);
        rect.x = rect.width + 30;
        rect.width = rect.width * 2f - 30;
        EditorGUI.ObjectField(rect, prefab, GUIContent.none);
        //EditorGUI.PropertyField(rect, prefab);
        DrawHorizontalLine(1, 0.25f);

        EndOnGUI(property);
    }

    void SetupProperties(SerializedProperty prop)
    {
        name = prop.FindPropertyRelative("name");
        prefab = prop.FindPropertyRelative("reference");
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