using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerGameplay/AbilitySet")]
public class AbilitySetAsset : ScriptableObject
{
	public new string name;
    public uint[] abilityIDs = new uint[(int)AbilitySlot.MAX];
	public List<uint> passiveEffectIDs;
}
