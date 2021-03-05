using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AbilityImporter : EditorWindow
{
    
    [MenuItem("Window/AbilityImporter")]
    public static void Init()
    {
        GetWindow(typeof(AbilityImporter));
    }

    void OnGUI()
    {
		if(GUILayout.Button("Reimport"))
		{
			AssetDatabase.Refresh();
			string[] abilityPaths = AssetDatabase.FindAssets("t:Ability");
			HashSet<string> alreadyCreatedAssets = new HashSet<string>();
			foreach(string s in abilityPaths)
			{
				alreadyCreatedAssets.Add(((Ability)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(Ability))).GetType().Name);
			}

			string[] allAbilityScriptsNames = AssetDatabase.FindAssets("", new [] {"Assets/ScriptableObjects/Abilities"});
			string fullPath;
			string path;
			string abilityName;
			foreach(string s in allAbilityScriptsNames)
			{
				fullPath = AssetDatabase.GUIDToAssetPath(s);
				path = fullPath.Substring(0, AssetDatabase.GUIDToAssetPath(s).LastIndexOf('/'));
				// Debug.Log("fp:"+fullPath);
				// Debug.Log("path:"+path);
				if(fullPath.IndexOf('.') == -1 || !fullPath.Contains(".cs"))
				{
					continue;
				}
				abilityName = fullPath.Substring(AssetDatabase.GUIDToAssetPath(s).LastIndexOf('/') + 1, fullPath.LastIndexOf('.') - AssetDatabase.GUIDToAssetPath(s).LastIndexOf('/') - 1);
				// Debug.Log(abilityName);
				if(alreadyCreatedAssets.Contains(abilityName))
				{
					continue;
				}
				ScriptableObject newEffectAsset = ScriptableObject.CreateInstance(abilityName);
				AssetDatabase.CreateAsset(newEffectAsset, path + "/" + abilityName +".asset");
				Debug.Log("Creating new asset: " + abilityName + ".asset");
			}
			Debug.Log("Tool running complete");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			return;
		}
	}
}
