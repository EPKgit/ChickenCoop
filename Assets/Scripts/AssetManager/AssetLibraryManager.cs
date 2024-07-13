using System;
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

    public Tuple<string, string> ParseStringToCategoryAndName(string initial, string defaultLibrary = "")
    {
        string category = "";
        string name = "";
        int index = initial.IndexOf('.');
        if (index == -1)
        {
            category = defaultLibrary;
            name = initial;
        }
        else
        {
            category = initial.Substring(0, index);
            name = initial.Substring(index + 1);
        }
        return new Tuple<string, string>(name, category);
    }

    public GameObject GetPrefab(string name, string category)
    {
        return main.GetAsset(name, category);
    }

    public Sprite GetIcon(string name, string category)
    {
        return main.GetIcon(name, category);
    }

    public ScriptableObject GetScriptableObject(string name, string category)
    {
        return main.GetScriptableObject(name, category);
    }
}
