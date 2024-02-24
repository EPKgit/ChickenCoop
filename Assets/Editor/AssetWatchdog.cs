using UnityEngine;
using UnityEditor;

public class AssetWatchdog : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string str in importedAssets)
        {
            string[] splitStr = str.Split('/', '.');
            if(str.Length <= 2)
            {
                continue;
            }
            string fileName = splitStr[splitStr.Length - 2];
            string extension = splitStr[splitStr.Length - 1];
            if(extension == "xml")
            {
                if(fileName == "AbilityData" || str.Contains("SplitAbilityData"))
                {
                    Debug.Log(EditorApplication.timeSinceStartup + "~ Hot Reloading Ability XMLs");
                    AbilityDataXMLParser.instance.ForceReimport();
                }
                if (fileName == "GameplayTags")
                {
                    Debug.Log(EditorApplication.timeSinceStartup + "~ Hot Reloading Gameplay Tags");
                    GameplayTagInternals.GameplayTagXMLParser.instance.ForceReimport();
#if UNITY_EDITOR_WIN
                    GameplayTagInternals.GameplayTagXMLParser.instance.GenerateTagEnumFile();
#endif
                }
            }
        }
    }
}