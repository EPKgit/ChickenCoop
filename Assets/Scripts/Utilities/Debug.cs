using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug : UnityEngine.Debug
{
    public static void DrawCircle(Vector3 position, float radius, int segments)
    {
        DrawCircle(position, radius, segments, Color.red);
    }

    public static void DrawCircle(Vector3 position, float radius, int segments, Color color)
    {
        DrawCircle(position, radius, segments, color, 0);
    }

    public static void DrawCircle(Vector3 position, float radius, int segments, Color color, float duration)
    {
        DrawCircle(position, radius, segments, color, duration, false);
    }

    public static void DrawCircle(Vector3 position, float radius, int segments, Color color, float duration, bool depthTest)
    {
        (Vector3, Vector3)[] segmentPoints = GetCircleLineSegments(position, radius, segments);
        if(segmentPoints == null)
        {
            return;
        }
        for (int i = 0; i < segments; i++)
        {
            DrawLine(segmentPoints[i].Item1, segmentPoints[i].Item2, color, duration, depthTest);
        }
    }

    public static (Vector3, Vector3)[] GetCircleLineSegments(Vector3 position, float radius, int segments)
    {
        if (radius <= 0.0f || segments <= 0)
        {
            return null;
        }
        (Vector3, Vector3)[] returnValue = new (Vector3, Vector3)[segments];

        float angleStep = (360.0f / segments) * Mathf.Deg2Rad;
        Vector3 lineStart = Vector3.zero;
        Vector3 lineEnd = Vector3.zero;
        for (int i = 0; i < segments; ++i)
        {
            lineStart.Set   (Mathf.Cos(angleStep * i),          Mathf.Sin(angleStep * i), 0);
            lineEnd.Set     (Mathf.Cos(angleStep * (i + 1)),    Mathf.Sin(angleStep * (i + 1)), 0);

            returnValue[i] = (lineStart * radius + position, lineEnd * radius + position);
        }
        return returnValue;
    }
}
