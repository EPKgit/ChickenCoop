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
        yinc = position.height / GetNumLines(property);
        rect = NextLine();
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, textInfo.ToTitleCase(label.text));
        if (!property.isExpanded)
        {
            return true;
        }
        return false;
    }

    protected virtual float GetNumLines(SerializedProperty property)
    {
        if (!property.isExpanded)
        {
            return 1;
        }
        return 1;
    }

    protected void DrawHorizontalLine(float height, Color? color = null)
    {
        NextLine();
        rect.y += (rect.height - height) / 2.0f;
        rect.height = height;
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
