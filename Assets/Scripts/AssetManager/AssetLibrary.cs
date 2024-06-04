using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "PlayerGameplay/AssetLibrary")]
public class AssetLibrary : ScriptableObject
{
    public string libraryName;

    private Dictionary<string, AssetLibrary> libraryLookups;
    private Dictionary<string, GameObject> assetLookups;

    [SerializeField]
    private AssetLibrary[] subLibraries;

    [System.Serializable]
    public class AssetReference
    {
        public enum AssetType
        {
            GAMEOBJECT,
            ICON,
        }
        public string name;
        public AssetType type = AssetType.GAMEOBJECT;
        public GameObject reference;
        public Sprite icon;
    }

    [SerializeField]
    private AssetReference[] assets;

    public void Initialize()
    {
        assetLookups = new Dictionary<string, GameObject>();
        foreach (var asset in assets) 
        {
            if(assetLookups.ContainsKey(asset.name))
            {
                throw new System.Exception("ERROR: duplicate asset created");
            }
            assetLookups[asset.name] = asset.reference;
        }

        libraryLookups = new Dictionary<string, AssetLibrary>();
        foreach(var subLibrary in subLibraries)
        {
            if(libraryLookups.ContainsKey(subLibrary.libraryName))
            {
                throw new System.Exception("ERROR: duplicate library created");
            }
            libraryLookups[subLibrary.libraryName] = subLibrary;
            subLibrary.Initialize();
        }
    }


    public GameObject GetAsset(string name, string category = "")
    {
        if(category != "")
        {
            return libraryLookups[category].GetAsset(name);
        }
        return assetLookups[name];
    }

    public Sprite GetIcon(string name, string category = "")
    {
        if (category != "")
        {
            return libraryLookups[category].GetIcon(name);
        }

        foreach(AssetReference asset in assets)
        {
            if(asset.name == name)
            {
                return asset.icon;
            }
        }
        return null;
    }

}
