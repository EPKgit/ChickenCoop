using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEditor.Callbacks;
using System.Text;

public class AssetPostProcessorWatchdog : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        DebugFlags.Log(DebugFlags.Flags.ASSET_WATCHDOG, "OnPostprocessAllAssets");

        foreach (string path in importedAssets)
        {
            DebugFlags.Log(DebugFlags.Flags.ASSET_WATCHDOG, $"importedAssets {path}");
            if (path.Length <= 2)
            {
                continue;
            }
            string[] splitStr = path.Split('/', '.');
            string fileName = splitStr[splitStr.Length - 2];
            string extension = splitStr[splitStr.Length - 1];
            CheckReimportedFile(path, fileName, extension);
        }        
    }

    static void CheckReimportedFile(string path, string fileName, string extension)
    {
        if (extension == "xml")
        {
            if (path.Contains("AbilityData"))
            {
                Debug.Log(EditorApplication.timeSinceStartup + "~ Hot Reloading Ability XMLs");
                AbilityDataXMLParser.instance.ForceReimport();
            }
            if (fileName == "GameplayTags")
            {
                Debug.Log(EditorApplication.timeSinceStartup + "~ Hot Reloading Gameplay Tags");
                GameplayTagInternals.GameplayTagXMLParser.instance.ForceReimport();
#if UNITY_EDITOR_WIN
                GameplayTagInternals.GameplayTagXMLParser.instance.GenerateFiles();
#endif
            }
        }
        else if (path == WatchDogCacheFile)
        {
            OnCompileScripts();
        }
        else if (extension == "cs")
        {
            //we need to queue cs files because unity hasn't finished recompiling
            //scripts by now so we can't fetch the scripts type yet so we defer
            //until scripts get reloaded
            QueueFileForReprocess(path);
        }
    }

    private const string WatchDogCacheFile = "Assets/Editor/AssetWatchdogCache.ccc";
    static bool CheckIfNeedAbilityImport(string path, bool isQueued)
    {
        MonoImporter monoImporter = AssetImporter.GetAtPath(path) as MonoImporter;
        if (monoImporter == null)
        {
            // not a script file
            return false;
        }

        MonoScript script = monoImporter.GetScript();
        Type scriptType = script.GetClass();
        if(scriptType == null)
        {
            Debug.LogError($"failed on queued file:{path}");
            return false;
        }

        if (scriptType.BaseType != typeof(Ability))
        {
            // not an ability, just a cs file
            return false;
        }

        return true;
    }

    public static void QueueFileForReprocess(string path)
    {
        DebugFlags.Log(DebugFlags.Flags.ASSET_WATCHDOG, $"queuing file:{path}");
        StringBuilder builder = new StringBuilder();
        if (File.Exists(WatchDogCacheFile))
        {
            builder.Append(File.ReadAllText(WatchDogCacheFile));
            builder.Append('\n');
        }
        builder.Append(path);
        File.WriteAllBytes(WatchDogCacheFile, Encoding.ASCII.GetBytes(builder.ToString().ToCharArray()));
    }

    [DidReloadScripts]
    static void OnCompileScripts()
    {
        DebugFlags.Log(DebugFlags.Flags.ASSET_WATCHDOG, $"OnCompileScripts");

        if (HasCachedFiles())
        {
            DebugFlags.Log(DebugFlags.Flags.ASSET_WATCHDOG, "HasCachedFiles");
            string fileText = File.ReadAllText(WatchDogCacheFile);
            DebugFlags.Log(DebugFlags.Flags.ASSET_WATCHDOG, $"{fileText}");
            string[] lines = fileText.Split('\n');
            bool shouldReimport = false;
            foreach (string line in lines)
            {
                DebugFlags.Log(DebugFlags.Flags.ASSET_WATCHDOG, $"checking queued {line}");
                if(line == "FORCE" || CheckIfNeedAbilityImport(line, true))
                {
                    shouldReimport = true;
                }
            }
            AssetDatabase.DeleteAsset(WatchDogCacheFile);

            if (shouldReimport)
            {
                AbilityImporter.Reimport();
            }
        }
    }

    public static bool HasCachedFiles()
    {
        return File.Exists(WatchDogCacheFile);
    }
}