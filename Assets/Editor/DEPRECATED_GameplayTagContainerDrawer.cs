#if false
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GameplayTagContainer))]
public class GameplayTagContainerDrawer : CustomPropertyDrawerBase
{
    //these 4 lines are the toolbar 2 lines and 
    private const int EXTRA_LINES = 4;
    private List<GameplayTag> tags;
    private Tuple<string, GameplayTagFlags>[] enumValues;
    private bool[] enumChecked;

    private int displayTab = 0;
    private static readonly string[] toolbarStrings = new string[] { "Current Tags", "Tag Editor" };
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if(StartOnGUI(position, property, label))
        {
            return;
        }
        NextLine();
        displayTab = GUI.Toolbar(rect, displayTab, toolbarStrings);
        DrawHorizontalLine(5);
        switch (displayTab)
        {
            case 0:
            {
                DrawActiveTags();
            }
            break;
            case 1:
            {
                DrawFullTags(property);
            }
            break;
        }
        DrawHorizontalLine(5);
    }

    private void DrawActiveTags()
    {
        if (tags == null)
        {
            return;
        }
        if(tags.Count == 0)
        {
            NextLine();
            EditorGUI.LabelField(rect, "NONE");
        }
        foreach (GameplayTag t in tags)
        {
            NextLine();
            EditorGUI.LabelField(rect, t.Flag.ToString());
        }
    }

    private void DrawFullTags(SerializedProperty serializedProperty)
    {
        if(!SetupEnumValues())
        {
            return;
        }
        UpdateCheckboxes();

        int indentLevel = EditorGUI.indentLevel;
        EditorGUI.BeginChangeCheck();
        for (int x = 0; x < enumValues.Length; ++x)
        {
            if(enumValues[x].Item2 == 0)
            {
                continue;
            }
            NextLine();
            rect.x += ((int)enumValues[x].Item2 & GameplayTagConstants.LAYER_MASK) * 15;
            rect.width -= 30;
            EditorGUI.LabelField(rect, enumValues[x].Item1);
            rect.x = rect.width;
            rect.width = 50;
            enumChecked[x] = EditorGUI.Toggle(rect, enumChecked[x]);

            if (x != enumValues.Length - 1)
            {
                yofs += 1;
                rect = new Rect(startRect.x, yofs, startRect.width, 1);
                EditorGUI.DrawRect(rect, Color.grey);
            }
        }
        EditorGUI.indentLevel = indentLevel;
        if (EditorGUI.EndChangeCheck())
        {
            WriteGameplayTagListOut(serializedProperty);
        }
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
                    flag.intValue = (int)enumValues[x].Item2;
                    tags.Add(new GameplayTag((int)enumValues[x].Item2, true));
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
        string[] names = Enum.GetNames(typeof(GameplayTagFlags));
        Array values = Enum.GetValues(typeof(GameplayTagFlags));
        enumValues = new Tuple<string, GameplayTagFlags>[names.Length];
        for (int x = 0; x < names.Length; ++x)
        {
            enumValues[x] = new Tuple<string, GameplayTagFlags>(names[x], (GameplayTagFlags)Enum.Parse(typeof(GameplayTagFlags), names[x]));
        }
        Array.Sort(enumValues, (i1, i2) =>
        {
            UInt32 layer1_1 = (UInt32)i1.Item2 & GameplayTagConstants.LAYER_1_BIT_MASK;
            UInt32 layer1_2 = (UInt32)i2.Item2 & GameplayTagConstants.LAYER_1_BIT_MASK;
            if (layer1_1 != layer1_2)
            {
                return layer1_1.CompareTo(layer1_2);
            }
            UInt32 layer2_1 = (UInt32)i1.Item2 & GameplayTagConstants.LAYER_2_BIT_MASK;
            UInt32 layer2_2 = (UInt32)i2.Item2 & GameplayTagConstants.LAYER_2_BIT_MASK;
            if (layer2_1 != layer2_2)
            {
                return layer2_1.CompareTo(layer2_2);
            }
            UInt32 layer3_1 = (UInt32)i1.Item2 & GameplayTagConstants.LAYER_3_BIT_MASK;
            UInt32 layer3_2 = (UInt32)i2.Item2 & GameplayTagConstants.LAYER_3_BIT_MASK;
            if (layer3_1 != layer3_2)
            {
                return layer3_1.CompareTo(layer3_2);
            }
            UInt32 layer4_1 = (UInt32)i1.Item2 & GameplayTagConstants.LAYER_4_BIT_MASK;
            UInt32 layer4_2 = (UInt32)i2.Item2 & GameplayTagConstants.LAYER_4_BIT_MASK;
            return layer4_1.CompareTo(layer4_2);

        });
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
                if (tags[x].Flag == enumValues[y].Item2)
                {
                    enumChecked[y] = true;
                    break;
                }
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int extra = 0;
        if(property.isExpanded && displayTab == 1)
        {
            //these extra are the lines in between different tags
            extra += enumValues.Length - 1;
        }
        return base.GetPropertyHeight(property, label) * GetNumLines(property) + extra;
    }

    protected override float GetNumLines(SerializedProperty property)
    {
        if(!property.isExpanded)
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
                    return enumValues.Length - 1 + EXTRA_LINES; //-1 for removing NONE
                }
            } break;
        }
        return 1;
    }
}
#endif