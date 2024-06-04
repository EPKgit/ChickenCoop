using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public GameObject GetPrefab(string name, string category)
    {
        return main.GetAsset(name, category);
    }

    public Sprite GetIcon(string name, string category)
    {
        return main.GetIcon(name, category);
    }
}
