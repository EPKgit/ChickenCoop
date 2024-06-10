using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEditor.Callbacks;

public class AssetModificationWatchdog : AssetModificationProcessor
{
    static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
    {
        DebugFlags.Log(DebugFlags.Flags.ASSET_WATCHDOG, $"OnWillDeleteAsset {path}");

        MonoImporter monoImporter = AssetImporter.GetAtPath(path) as MonoImporter;
        if (monoImporter == null)
        {
            return AssetDeleteResult.DidNotDelete;
        }

        MonoScript script = monoImporter.GetScript();
        Type scriptType = script.GetClass();
        if (scriptType.BaseType != typeof(Ability))
        {
            return AssetDeleteResult.DidNotDelete;
        }

        AssetPostProcessorWatchdog.QueueFileForReprocess("FORCE");
        return AssetDeleteResult.DidNotDelete;
    }
}