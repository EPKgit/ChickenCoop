using System;
using System.Globalization;
using UnityEngine;
using UnityEditor;

public abstract class CustomPropertyDrawerBase : PropertyDrawer
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

    protected Rect NextLine(SerializedProperty property)
    {
        float height = EditorGUI.GetPropertyHeight(property);
        rect = new Rect(startRect.x, yofs, startRect.width, height);
        yofs += height;
        return rect;
    }

    abstract protected bool DoesExpansion();

    protected bool StartOnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // EditorGUI.DrawRect(position, Color.red);

        EditorGUI.BeginProperty(position, label, property);

        startRect = position;
        yofs = position.y;
        yinc = (position.height - GetExtraSpace(property)) / GetNumLines(property);

        if (DoesExpansion())
        {
            topRect = rect = NextLine();
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, textInfo.ToTitleCase(label.text));
        }
        else
        {
            property.isExpanded = true;
        }
        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
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
        EditorGUI.EndProperty();
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
