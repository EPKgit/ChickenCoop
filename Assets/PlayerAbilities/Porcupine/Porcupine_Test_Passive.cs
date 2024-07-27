using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Porcupine_Test_Passive : Ability
{
    public override uint ID => 200;

    public StatModificationHandle handle;
    public override void Initialize(PlayerAbilities pa)
    {
        base.Initialize(pa);
        handle = pa.stats.GetStat(StatName.MaxHealth).AddAdditiveModifier(5);
        pa.OnAbilityActivating += OnCast;
    }

    public override void Cleanup(PlayerAbilities pa)
    {
        base.Cleanup(pa);
        pa.stats.GetStat(StatName.MaxHealth).RemoveAdditiveModifier(handle);
        pa.OnAbilityActivating -= OnCast;
    }

    void OnCast(AbilityEventData data)
    {
        if(data.ability.abilityTags.Contains(GameplayTagFlags.ABILITY_DAMAGE))
        {
            Debug.Log("damage");
        }
    }
}
