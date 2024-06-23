using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class GizmoHelpers
{
    public struct GizmoColor
    {
        public int index;
        public Color Color() { return GizmoHelpers.GetColor(index); }
        public Color Get() { return GizmoHelpers.GetColor(index); }
        public Color Next() 
        {
            Color c = GizmoHelpers.GetColor(index++);
            index %= progressiveColors.Length;
            Gizmos.color = c;
            return c; 
        }
        public GizmoColor(int i = 0)
        {
            index = i - 1;
            Next();
        }
    }

    private readonly static Color[] progressiveColors = new Color[]
    {
        Color.red,
        Color.yellow,
        Color.green,
        Color.blue,
        Color.cyan,
        Color.magenta,
    };
    public static Color GetColor(int index)
    {
        return progressiveColors[index];
    }

    public static GizmoColor GetColor()
    {
        return new GizmoColor();
    }
}
