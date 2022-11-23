using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FogOfWarManager : MonoSingleton<FogOfWarManager>
{
    public static bool FOG_ENABLED = false;
    public static bool TESTING_FOG = false;

    public RenderTexture terrainTexture;
    public RenderTexture nonTerrainTexture;

    public RawImage DEBUGIMAGE;
    public RawImage overlayImage;

    private List<FogOfWarUnitComponent> units;

    private FogVisionGrid grid;

    protected override void OnCreation()
    {
        base.OnCreation();

        grid = new FogVisionGrid(Camera.main);
        units = new List<FogOfWarUnitComponent>();

        if (!FOG_ENABLED)
        {
            this.enabled = false;
            return;
        }
        
        DEBUGIMAGE.texture = grid.GetTexture();

        overlayImage.material.SetTexture("_FogTexture", grid.GetTexture());
        overlayImage.material.SetTexture("_TerrainRenderTexture", terrainTexture);
        overlayImage.material.SetTexture("_NonTerrainRenderTexture", nonTerrainTexture);

        overlayImage.gameObject.SetActive(true);
        DEBUGIMAGE.gameObject.SetActive(true);

        overlayImage.gameObject.SetActive(false);
        DEBUGIMAGE.gameObject.SetActive(false);
        this.enabled = false;
    }

    public bool RegisterUnit(FogOfWarUnitComponent comp)
    {
        if(units.Contains(comp))
        {
            return false;
        }
        units.Add(comp);
        return true;
    }

    public bool DeregisterUnit(FogOfWarUnitComponent comp)
    {
        if(units.Contains(comp))
        {
            units.Remove(comp);
            return true;
        }
        return false;
    }

    void Update()
    {
        grid.BeginTextureProcessing(Camera.main);
        foreach(FogOfWarUnitComponent unit in units)
        {
            grid.ProcessUnit(unit);
        }
        grid.FinalizeTexture();
    }
}
