using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogVisionGrid
{
    public const int IMAGE_RESOLUTION = 256;
    private readonly Color visibleColor = new Color(1.0f, 1.0f, 0, 0.0f);
    private readonly Color fogColor = FogOfWarManager.TESTING_FOG ? new Color(0, 0, 0.2f, 0.5f) : new Color(0, 0, 0, 1.0f);

    private int width;
    private int height;
    private int halfWidth;
    private int halfHeight;
    private byte[,] visionValues;
    private Texture2D texture;
    private CameraValues cameraValues;

    public FogVisionGrid(Camera displayCamera)
    {
        cameraValues = new CameraValues();
        texture = new Texture2D(1, 1);
        ResizeGridByCameraValue(displayCamera);
        Clear();
    }

    private void ResizeGridByCameraValue(Camera displayCamera)
    {
        height = IMAGE_RESOLUTION;
        width = (int)(displayCamera.aspect * height);
        halfWidth = (int)(width * 0.5f);
        halfHeight = (int)(height * 0.5f);
        texture.Resize(width, height);
        visionValues = new byte[width, height];
    }

    public void Clear()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                visionValues[x, y] = 0;
            }
        }
    }

    public bool IsVisible(int x, int y)
    {
        return visionValues[x, y] != 0;
    }
    
    public void Set(int x, int y)
    {
        if(x < 0 || x >= width)
        {
            return;
        }
        if(y < 0 || y >= height)
        {
            return;
        }
        visionValues[x, y] = 1;
    }

    public void BeginTextureProcessing(Camera displayCamera)
    {
        Clear();
        bool cameraPosChange = (displayCamera.transform.position - cameraValues.cameraPosition).magnitude > float.Epsilon;
        bool cameraFOVChanged = (cameraValues.pixelSize - new Vector2(displayCamera.pixelWidth, displayCamera.pixelHeight)).magnitude > float.Epsilon;
        if(cameraPosChange || cameraFOVChanged)            
        {
            if(cameraFOVChanged)
            {
                ResizeGridByCameraValue(displayCamera);
            }
            cameraValues.cameraPosition = displayCamera.transform.position;
            cameraValues.pixelSize = new Vector2(displayCamera.pixelWidth, displayCamera.pixelHeight);
            Vector3 worldPoint = displayCamera.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, -cameraValues.cameraPosition.z));
            float w = worldPoint.x - cameraValues.cameraPosition.x;
            float h = worldPoint.y - cameraValues.cameraPosition.y;
            cameraValues.clippingRect = new Rect(cameraValues.cameraPosition.x - w, cameraValues.cameraPosition.y - h, w * 2, h * 2);
            cameraValues.scaleFactorX = width / 2 / w;
            cameraValues.scaleFactorY = height / 2 / h;
        }
    }

    public void ProcessUnit(FogOfWarUnitComponent unit)
    {
        Vector3 pos = unit.position;
        int r = unit.range;
        if(ShouldDiscard(pos, r))
        {
            return;
        }
        //square
        // for (int x = -r; x <= r; ++x)
        // {
        //     int diff = r - Mathf.Abs(x);
        //     for (int y = -diff; y <= diff; ++y)
        //     {
        for (int x = -r; x <= r; ++x)
        {
            for (int y = -r; y <= r; ++y)
            {
                if(x*x + y*y > r*r)
                {
                    continue;
                }
                Set
                (
                    halfWidth + x + (int)((pos.x - cameraValues.cameraPosition.x) * cameraValues.scaleFactorX),
                    halfHeight + y + (int)((pos.y - cameraValues.cameraPosition.y) * cameraValues.scaleFactorY)
                );
            }
        }
    }

    private bool ShouldDiscard(Vector3 pos, int r)
    {
        return false;
    }

    public void FinalizeTexture()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (visionValues[x, y] > 0)
                {
                    texture.SetPixel(x, y, visibleColor);
                }
                else
                {
                    texture.SetPixel(x, y, fogColor);
                }
            }
        }
        texture.Apply();
    }

    public Texture2D GetTexture()
    {
        return texture;
    }

    private class CameraValues
    {
        public float scaleFactorX;
        public float scaleFactorY;
        public Vector3 cameraPosition;
        public Rect clippingRect;
        public Vector2 pixelSize;
    }
}
