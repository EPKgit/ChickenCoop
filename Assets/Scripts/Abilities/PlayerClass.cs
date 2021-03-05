using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerGameplay/Class")]
public class PlayerClass : ScriptableObject
{
	public new string name;
    public AbilitySet abilities;
	public GameObject playerModelPrefab;
	public List<StatInspectorValue> stats;
}