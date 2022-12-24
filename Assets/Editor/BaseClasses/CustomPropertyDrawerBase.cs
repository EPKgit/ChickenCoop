using System;
using System.Globalization;
using UnityEngine;
using UnityEditor;

public class CustomPropertyDrawerBase : PropertyDrawer
{
    protected float yofs;
    protected float yinc;
    protected TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
    protected Rect rect = new Rect();
    protected Rect startRect = new Rect();
    protected Rect topRect = new Rect();

    protected Rect NextLine()
    {
        rect = new Rect(startRect.x, yofs, startRect.width, yinc);
        yofs += yinc;
        return rect;
    }
    protected Rect NextLine(float percentageOfLine)
    {
        rect = new Rect(startRect.x, yofs, startRect.width, yinc * percentageOfLine);
        yofs += yinc * percentageOfLine;
        return rect;
    }

    protected bool StartOnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // EditorGUI.DrawRect(position, Color.red);
        startRect = position;
        yofs = position.y;
        yinc = (position.height - GetExtraSpace(property)) / GetNumLines(property);
        topRect = rect = NextLine();
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, textInfo.ToTitleCase(label.text));
        if (!property.isExpanded)
        {
            return true;
        }
        return false;
    }

    protected void EndOnGUI(SerializedProperty property)
    {
        Rect clickArea = topRect;

        Event current = Event.current;

        if (clickArea.Contains(current.mousePosition) && current.type == EventType.ContextClick)
        {
            CustomRightClickMenu(property);
            current.Use();
        }
    }

    protected virtual void CustomRightClickMenu(SerializedProperty property)
    {

    }

    protected virtual float GetNumLines(SerializedProperty property)
    {
        if (!property.isExpanded)
        {
            return 1;
        }
        return 1;
    }
    
    protected virtual float GetExtraSpace(SerializedProperty property)
    {
        return 0;
    }

    protected void DrawHorizontalLine(float height, Color? color = null)
    {
        NextLine();
        rect.y += (rect.height - height) / 2.0f;
        rect.height = height;
        EditorGUI.DrawRect(rect, color ?? Color.grey);
    }

    protected void DrawHorizontalLineWithoutFullLineAdvance(float height, Color? color = null)
    {
        rect = new Rect(startRect.x, yofs, startRect.width, height);
        yofs += height;
        EditorGUI.DrawRect(rect, color ?? Color.grey);
    }

    protected void DrawHorizontalLine(float height, float linePercentage, Color? color = null)
    {
        NextLine(linePercentage);
        rect.y += (rect.height - height) / 2.0f;
        rect.height = height;
        EditorGUI.DrawRect(rect, color ?? Color.grey);
    }
}
