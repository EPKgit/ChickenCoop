using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLHelpers
{
    private static Material lineMaterial;
    private static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    public static void GLDrawCircle(Transform transform, float radius, int segments, Color color)
    {
        (Vector3, Vector3)[] segmentPoints = Debug.GetCircleLineSegments(Vector3.zero, radius, segments);
        if (segmentPoints == null)
        {
            return;
        }
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        GL.Begin(GL.TRIANGLES);
        for (int i = 0; i < segments; ++i)
        {
            GL.Color(color);
            GL.Vertex(segmentPoints[i].Item1);
            GL.Vertex(segmentPoints[i].Item2);
            GL.Vertex(Vector3.zero);
        }
        GL.End();
        GL.PopMatrix();
    }

    public static void GLDrawRect(Transform transform, Rect rect, Color color)
    {
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetTRS(transform.position + (Vector3)rect.center, transform.rotation, transform.localScale);
        GL.MultMatrix(matrix);

        GL.Begin(GL.TRIANGLE_STRIP);
        GL.Color(color);
        rect.center = Vector2.zero;
        GL.Vertex3(rect.xMin, rect.yMin, 0);
        GL.Vertex3(rect.xMin, rect.yMax, 0);
        GL.Vertex3(rect.xMax, rect.yMin, 0);
        GL.Vertex3(rect.xMax, rect.yMax, 0);

        GL.End();
        GL.PopMatrix();
    }

    public static void GLDrawPolygon(Transform transform, Vector3[] points, Color color)
    {
        if(points.Length < 3)
        {
            return;
        }
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.TRIANGLES);
        GL.Color(color);
        for (int x = 0; x < points.Length - 1; ++x)
        {
            GL.Vertex(points[x]);
            GL.Vertex(points[x + 1]);
            GL.Vertex(Vector3.zero);
        }
        GL.Vertex(points[points.Length - 1]);
        GL.Vertex(points[0]);
        GL.Vertex(Vector3.zero);

        GL.End();
        GL.PopMatrix();
    }
}
