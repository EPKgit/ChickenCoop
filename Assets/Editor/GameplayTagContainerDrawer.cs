using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GameplayTagContainer))]
public class GameplayTagContainerDrawer : PropertyDrawer
{
    private const int EXTRA_LINES = 5;


    private float yofs;
    private float yinc;
    private List<GameplayTag> tags;
    private string[] enumNames;
    private GameplayTagFlags[] enumValues;
    private bool[] enumChecked;

    private TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

    private bool foldout = false;
    private int displayTab = 0;
    private static readonly string[] toolbarStrings = new string[] { "Current Tags", "Tag Editor" };
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        yofs = position.y;
        yinc = position.height / GetNumLines(property);
        Rect rect = NextLine(position);
        bool temp = EditorGUI.Foldout(rect, foldout, textInfo.ToTitleCase(property.name));
        if(foldout != temp || !foldout)
        {
            foldout = temp;
            return;
        }
        rect = NextLine(position);
        rect = NextLine(position);
        displayTab = GUI.Toolbar(rect, displayTab, toolbarStrings);
        rect = NextLine(position);
        rect.y += (rect.height - 5) / 2.0f;
        rect.height = 5;
        EditorGUI.DrawRect(rect, Color.grey);
        switch (displayTab)
        {
            case 0:
            {
                DrawActiveTags(position);
            }
            break;
            case 1:
            {
                DrawFullTags(position, property);
            }
            break;
        }
        rect = NextLine(position);
        rect.y += (rect.height - 5) / 2.0f;
        rect.height = 5;
        EditorGUI.DrawRect(rect, Color.grey);
    }

    private void DrawActiveTags(Rect position)
    {
        if (tags == null)
        {
            return;
        }
        Rect rect;
        if(tags.Count == 0)
        {
            rect = NextLine(position);
            EditorGUI.LabelField(rect, "NONE");
        }
        foreach (GameplayTag t in tags)
        {
            rect = NextLine(position);
            EditorGUI.LabelField(rect, t.Flag.ToString());
        }
    }

    private void DrawFullTags(Rect position, SerializedProperty serializedProperty)
    {
        if(!SetupEnumValues())
        {
            return;
        }
        UpdateCheckboxes();

        Rect rect;
        int indentLevel = EditorGUI.indentLevel;
        EditorGUI.BeginChangeCheck();
        for (int x = 0; x < enumNames.Length; ++x)
        {
            if(enumValues[x] == 0)
            {
                continue;
            }
            rect = NextLine(position);
            rect.x += ((int)enumValues[x] & GameplayTagConstants.LAYER_MASK) * 15;
            rect.width -= 30;
            EditorGUI.LabelField(rect, enumNames[x]);
            rect.x = rect.width;
            rect.width = 30;
            enumChecked[x] = EditorGUI.Toggle(rect, enumChecked[x]);
        }
        EditorGUI.indentLevel = indentLevel;
        if (EditorGUI.EndChangeCheck())
        {
            WriteGameplayTagListOut(serializedProperty);
        }
    }

    private Rect NextLine(Rect position)
    {
        Rect rect = new Rect(position.x, yofs, position.width, yinc);
        yofs += yinc;
        return rect;
    }

    private List<GameplayTag> FetchGameplayTagListFromProperty(SerializedProperty serializedProperty)
    {
        SerializedProperty sp = serializedProperty.FindPropertyRelative("tags");
        if (sp != null && sp.isArray)
        {
            int arrayLength = sp.arraySize;

            List<GameplayTag> list = new List<GameplayTag>();

            for (int x = 0; x < arrayLength; x++)
            {
                SerializedProperty curr = sp.GetArrayElementAtIndex(x);
                SerializedProperty flag = curr.FindPropertyRelative("_flag"); //we need to drill one level deeper
                list.Add(new GameplayTag(flag.intValue, true));
            }
            return list;
        }
        return null;
    }

    private void WriteGameplayTagListOut(SerializedProperty serializedProperty)
    {
        SerializedProperty sp = serializedProperty.FindPropertyRelative("tags");
        if (sp != null && sp.isArray)
        {
            int arrayLength = sp.arraySize;

            for (int x = 0; x < arrayLength; x++)
            {
                sp.DeleteArrayElementAtIndex(0);
            }

            tags.Clear();
            for(int x = 0; x < enumChecked.Length; ++x)
            {
                if(enumChecked[x])
                {
                    sp.InsertArrayElementAtIndex(0);
                    SerializedProperty curr = sp.GetArrayElementAtIndex(0);
                    SerializedProperty flag = curr.FindPropertyRelative("_flag"); //we need to drill one level deeper
                    flag.intValue = (int)enumValues[x];
                    tags.Add(new GameplayTag((int)enumValues[x], true));
                }
            }
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    private bool SetupEnumValues()
    {
        if(enumValues != null)
        {
            return true;
        }
        Array arr = Enum.GetValues(typeof(GameplayTagFlags));
        enumValues = new GameplayTagFlags[arr.Length];
        for (int x = 0; x < arr.Length; ++x)
        {
            enumValues[x] = (GameplayTagFlags)arr.GetValue(x);
        }
        Array.Sort(enumValues, (i1, i2) =>
        {
            UInt32 layer1_1 = (UInt32)i1 & GameplayTagConstants.LAYER_1_BIT_MASK;
            UInt32 layer1_2 = (UInt32)i2 & GameplayTagConstants.LAYER_1_BIT_MASK;
            if (layer1_1 != layer1_2)
            {
                return layer1_1.CompareTo(layer1_2);
            }
            UInt32 layer2_1 = (UInt32)i1 & GameplayTagConstants.LAYER_2_BIT_MASK;
            UInt32 layer2_2 = (UInt32)i2 & GameplayTagConstants.LAYER_2_BIT_MASK;
            if (layer2_1 != layer2_2)
            {
                return layer2_1.CompareTo(layer2_2);
            }
            UInt32 layer3_1 = (UInt32)i1 & GameplayTagConstants.LAYER_3_BIT_MASK;
            UInt32 layer3_2 = (UInt32)i2 & GameplayTagConstants.LAYER_3_BIT_MASK;
            if (layer3_1 != layer3_2)
            {
                return layer3_1.CompareTo(layer3_2);
            }
            UInt32 layer4_1 = (UInt32)i1 & GameplayTagConstants.LAYER_4_BIT_MASK;
            UInt32 layer4_2 = (UInt32)i2 & GameplayTagConstants.LAYER_4_BIT_MASK;
            return layer4_1.CompareTo(layer4_2);

        });
        enumNames = new string[enumValues.Length];
        int index = 0;
        foreach(var val in enumValues)
        {
            enumNames[index++] = Enum.GetName(typeof(GameplayTagFlags), val);
        }
        enumChecked = new bool[enumValues.Length];
        return true;
    }

    void UpdateCheckboxes()
    {
        for(int x = 0; x < enumChecked.Length; ++x)
        {
            enumChecked[x] = false;
        }
        for (int x = 0; x < tags.Count; ++x)
        {
            for (int y = 0; y < enumValues.Length; ++y)
            {
                if (tags[x].Flag == enumValues[y])
                {
                    enumChecked[y] = true;
                    break;
                }
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * GetNumLines(property);
    }

    private float GetNumLines(SerializedProperty property)
    {
        if(!foldout)
        {
            return 1;
        }
        switch (displayTab)
        {
            case 0:
            {
                tags = FetchGameplayTagListFromProperty(property);
                return (tags.Count == 0 ? 1 : tags.Count) + EXTRA_LINES;
            }
            case 1:
            {
                if (SetupEnumValues())
                {
                    return enumNames.Length + EXTRA_LINES - 1; //-1 for removing NONE
                }
            } break;
        }
        return 1;
    }
}


