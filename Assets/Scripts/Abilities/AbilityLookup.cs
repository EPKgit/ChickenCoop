//this file is generated by AbilityImporter
using System;
using System.Collections.Generic;
public static class AbilityLookup
{
	public static Dictionary<uint, Type> Lookup = new Dictionary<uint, Type>()
	{
		{ 6, typeof(Ability_Blank) },
		{ 2, typeof(BasicBomb) },
		{ 1, typeof(BasicShoot) },
		{ 4, typeof(BasicTargetHeal) },
		{ 15, typeof(BasicTargetShield) },
		{ 7, typeof(BasicTargetStun) },
		{ 13, typeof(ConstructTeleporter_Ability) },
		{ 5, typeof(CooldownOnAbilityCast) },
		{ 3, typeof(DashDodge) },
		{ 11, typeof(EveryXAbilityNoCD_Ability) },
		{ 12, typeof(FlipFlopCooldown_Ability) },
		{ 14, typeof(PassiveRegen) },
		{ 8, typeof(RecastableTeleport) },
		{ 10, typeof(Shockwave_Ability) },
		{ 102, typeof(Bite_Ability) },
		{ 104, typeof(Maul_Ability) },
		{ 101, typeof(Scurry_Ability) },
		{ 103, typeof(Swipe_Ability) },
		{ 202, typeof(SpineBackpack_Ability) },
		{ 201, typeof(PiercingSpine_Ability) },
		{ 204, typeof(SpineRain_Ability) },
		{ 203, typeof(TailWhip_Ability) },
	};
	public static Ability CreateAbilityFromId(uint ID)
	{
		if(!Lookup.ContainsKey(ID))
		{
			return null;
		}
		return Activator.CreateInstance(Lookup[ID]) as Ability;
	}
}
