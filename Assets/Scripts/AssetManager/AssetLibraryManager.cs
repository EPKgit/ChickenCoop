using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLibraryManager : MonoSingleton<AssetLibraryManager>
{
    [SerializeField]
    private AssetLibrary main;

    protected override void OnCreation()
    {
        base.OnCreation();
        main = ScriptableObject.Instantiate(main);
        main.Initialize();
    }

    public GameObject RequestPrefab(string category, string name)
    {
        return main.GetAsset(category, name);
    }
}
