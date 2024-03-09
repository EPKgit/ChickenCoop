using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public AssetReference(string name, GameObject reference)
        {
            this.name = name;
            this.reference = reference;
        }
        public string name;
        public GameObject reference;
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
}
