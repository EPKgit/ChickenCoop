using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerGameplay/AbilitySet")]
public class AbilitySetAsset : ScriptableObject
{
	public new string name;
    public Ability[] abilities = new Ability[(int)AbilitySlots.MAX];
	public List<Ability> passiveEffects;
}
