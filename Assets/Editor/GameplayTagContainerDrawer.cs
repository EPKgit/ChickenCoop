using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameplayTagInternals;

using RawGameplayTag = System.String;

[CustomPropertyDrawer(typeof(GameplayTagContainer))]
public class GameplayTagContainerDrawer : CustomPropertyDrawerBase
{
    //these 2 lines are the header, toolbar
    private const int EXTRA_LINES = 2;
    private const int DELIMITER_LINE_THICKNESS = 5;
    private const int SPACER_LINE_THICKNESS = 1;
    private const int EXTRA_SPACE = DELIMITER_LINE_THICKNESS * 2;
    private const int DEPTH_OFFSET_X = 15;


    private List<GameplayTagWrapper> tags;
    private (RawGameplayTag, bool)[] checkboxStates;

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
        DrawHorizontalLineWithoutFullLineAdvance(DELIMITER_LINE_THICKNESS, Color.grey);
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
        DrawHorizontalLineWithoutFullLineAdvance(DELIMITER_LINE_THICKNESS, Color.grey);
        EndOnGUI(property);
    }

    protected override void CustomRightClickMenu(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Reset"), false, () => 
        {
            property.serializedObject.Update();
            tags.Clear();
            for(int x = 0; x < checkboxStates.Length; ++x)
            {
                checkboxStates[x].Item2 = false;
            }
            WriteGameplayTagListOut(property);
        });
        menu.ShowAsContext();
    }

    private void DrawActiveTags()
    {
        if (tags == null)
        {
            return;
        }
        FetchTagNames();
        if(tags.Count == 0)
        {
            NextLine();
            EditorGUI.LabelField(rect, "NONE");
        }
        //do it this way so we have a consistent order
        for(int x = 0; x < checkboxStates.Length; ++x)
        {
            for(int y = 0; y < tags.Count; ++y)
            {
                if(tags[y].MatchesExact(checkboxStates[x].Item1))
                {
                    NextLine();
                    EditorGUI.LabelField(rect, tags[y].Flag.ToString());
                    break;
                }
            }
        }
    }

    private void DrawFullTags(SerializedProperty serializedProperty)
    {
        FetchTagNames();
        UpdateCheckboxes();

        int indentLevel = EditorGUI.indentLevel;
        EditorGUI.BeginChangeCheck();
        for (int x = 0; x < checkboxStates.Length; ++x)
        {
            RawGameplayTag currentTag = checkboxStates[x].Item1;
            if(currentTag == GameplayTagFlags.NONE)
            {
                continue;
            }
            NextLine();
            int depth = 0;
            int lastPeriodIndex = -1;
            for(int y = 0; y < currentTag.Length; ++y)
            {
                if (currentTag[y] == '.')
                {
                    depth++;
                    lastPeriodIndex = y;
                }
            }
            depth = depth * DEPTH_OFFSET_X;
            rect.x += depth;
            rect.width -= DEPTH_OFFSET_X;
            EditorGUI.LabelField(rect, lastPeriodIndex == -1 ? currentTag : currentTag.Substring(lastPeriodIndex + 1));
            rect.x = rect.width;
            rect.width = 50;
            checkboxStates[x].Item2 = EditorGUI.Toggle(rect, checkboxStates[x].Item2);

            if (x != checkboxStates.Length - 1)
            {
                DrawHorizontalLineWithoutFullLineAdvance(SPACER_LINE_THICKNESS, Color.grey);
            }
        }
        EditorGUI.indentLevel = indentLevel;
        if (EditorGUI.EndChangeCheck())
        {
            WriteGameplayTagListOut(serializedProperty);
        }
    }

    private List<GameplayTagWrapper> FetchGameplayTagListFromProperty(SerializedProperty serializedProperty)
    {
        SerializedProperty sp = serializedProperty.FindPropertyRelative("tags");
        if (sp != null && sp.isArray)
        {
            int arrayLength = sp.arraySize;

            List<GameplayTagWrapper> list = new List<GameplayTagWrapper>();

            for (int x = 0; x < arrayLength; x++)
            {
                SerializedProperty curr = sp.GetArrayElementAtIndex(x);
                SerializedProperty flag = curr.FindPropertyRelative("_flag"); //we need to drill one level deeper
                list.Add(new GameplayTagWrapper(flag.stringValue, true));
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
            for(int x = 0; x < checkboxStates.Length; ++x)
            {
                if(checkboxStates[x].Item2)
                {
                    sp.InsertArrayElementAtIndex(0);
                    SerializedProperty curr = sp.GetArrayElementAtIndex(0);
                    SerializedProperty flag = curr.FindPropertyRelative("_flag"); //we need to drill one level deeper
                    flag.stringValue = checkboxStates[x].Item1;
                    tags.Add(new GameplayTagWrapper(checkboxStates[x].Item1, true));
                }
            }
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    private bool FetchTagNames()
    {
        if(checkboxStates != null)
        {
            return false;
        }
        List<RawGameplayTag> tagNames = GameplayTagXMLParser.instance.GetRawTagList(); 
        checkboxStates = new (RawGameplayTag, bool)[tagNames.Count];
        int index = 0;
        foreach(var s in tagNames)
        {
            checkboxStates[index++] = (s, false);
        }
        return true;
    }

    void UpdateCheckboxes()
    {
        for(int x = 0; x < checkboxStates.Length; ++x)
        {
            checkboxStates[x].Item2 = false;
        }
        for (int x = 0; x < tags.Count; ++x)
        {
            for (int y = 0; y < checkboxStates.Length; ++y)
            {
                if (tags[x].MatchesExact(checkboxStates[y].Item1))
                {
                    checkboxStates[y].Item2 = true;
                    break;
                }
            }
        }
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
        int extra = 0;
        if(property.isExpanded)
        {
            switch(displayTab)
            {
                case 0: break;
                case 1:
                {
                    //these extra are the lines in between different tags in the full tag editor
                    // -2 because there is 1 less line than tag and we are also missing one for NONE
                    extra += (checkboxStates.Length - 2) * SPACER_LINE_THICKNESS;
                } break;
            }
            extra += EXTRA_SPACE;            
        }
        return extra;
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
                FetchTagNames();
                return checkboxStates.Length - 1 + EXTRA_LINES; //-1 for removing NONE
            }
        }
        return 1;
    }
}