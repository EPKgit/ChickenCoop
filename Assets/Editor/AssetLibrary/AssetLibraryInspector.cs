using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AssetLibrary), true)]
public class AssetLibraryInspector : Editor
{
    private AssetLibrary library;

    private SerializedProperty libraryName;
    private SerializedProperty subLibraryArray;
    private SerializedProperty assetArray;

    void OnEnable()
    {
        library = target as AssetLibrary;

        libraryName = serializedObject.FindProperty("libraryName");
        subLibraryArray = serializedObject.FindProperty("subLibraries");
        assetArray = serializedObject.FindProperty("assets");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(libraryName);
        GUISubLibraries();
        GUIAssets();
        serializedObject.ApplyModifiedProperties();
    }

    void DrawSeperator()
    {
        var oldIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUI.indentLevel = oldIndent;
    }

    private static bool sublibFoldout = false;
    void GUISubLibraries()
    {
        if(!(sublibFoldout = EditorGUILayout.Foldout(sublibFoldout, "Sub-Libraries")))
        {
            return;
        }

        Color backgroundColor = new Color32(255, 255, 255, 15);
        var screenRect = GUILayoutUtility.GetRect(1, 1);
        var vertRect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(new Rect(screenRect.x, screenRect.y, screenRect.width, vertRect.height), backgroundColor);

        int n = subLibraryArray.arraySize;

        DrawSeperator();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Add Library", EditorStyles.boldLabel);
        if (GUILayout.Button("+"))
        {
            subLibraryArray.InsertArrayElementAtIndex(n);
        }
        EditorGUILayout.EndHorizontal();

        DrawSeperator();

        if (n <= 0)
        {
            EditorGUILayout.LabelField("No Sub-Libraries", EditorStyles.boldLabel);
        }
        ++EditorGUI.indentLevel;
        for (int x = 0; x < n; ++x)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(subLibraryArray.GetArrayElementAtIndex(x), new GUIContent("Sub-Library " + x));
            if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
            {
                subLibraryArray.DeleteArrayElementAtIndex(x);
            }
            EditorGUILayout.EndHorizontal();
        }
        --EditorGUI.indentLevel;

        DrawSeperator();

        EditorGUILayout.EndVertical();
    }

    void GUIAssets()
    {
        Color backgroundColor = new Color32(255, 255, 255, 15);
        var screenRect = GUILayoutUtility.GetRect(1, 1);
        var vertRect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(new Rect(screenRect.x, screenRect.y, screenRect.width, vertRect.height), backgroundColor);

        int n = assetArray.arraySize;

        EditorGUILayout.PrefixLabel("Library Assets");
        DrawSeperator();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Add Asset", EditorStyles.boldLabel);
        if (GUILayout.Button("+"))
        {
            assetArray.InsertArrayElementAtIndex(n);
        }
        EditorGUILayout.EndHorizontal();

        DrawSeperator();

        if (n <= 0)
        {
            EditorGUILayout.LabelField("No Assets", EditorStyles.boldLabel);
        }
        ++EditorGUI.indentLevel;
        for (int x = 0; x < n; ++x)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(assetArray.GetArrayElementAtIndex(x));
            if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
            {
                assetArray.DeleteArrayElementAtIndex(x);
            }
            EditorGUILayout.EndHorizontal();
        }
        --EditorGUI.indentLevel;

        DrawSeperator();

        EditorGUILayout.EndVertical();
    }
}