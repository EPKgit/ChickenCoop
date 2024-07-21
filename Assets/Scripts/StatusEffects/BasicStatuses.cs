using Statuses;
using System;
using System.Collections.Generic;
using UnityEngine;
using RawGameplayTag = System.String;

public class Stun_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.STUN;

    public override RawGameplayTag[] flags => new RawGameplayTag[]
    { 
        GameplayTagFlags.CASTING_DISABLED,
        GameplayTagFlags.CASTING_EFFECTED,
        GameplayTagFlags.MOVEMENT_EFFECTED,
        GameplayTagFlags.MOVEMENT_DISABLED,
        GameplayTagFlags.ATTACKING_DISABLED,
        GameplayTagFlags.ATTACKING_EFFECTED,
    };
}

public class Silence_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.SILENCE;

    public override RawGameplayTag[] flags => new RawGameplayTag[]
    {
        GameplayTagFlags.CASTING_DISABLED,
        GameplayTagFlags.CASTING_EFFECTED,
    };
}

public class Root_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.ROOT;

    public override RawGameplayTag[] flags => new RawGameplayTag[]
    {
        GameplayTagFlags.MOVEMENT_EFFECTED,
        GameplayTagFlags.MOVEMENT_DISABLED,
    };
}

public class Blind_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.BLIND;

    public override RawGameplayTag[] flags => new RawGameplayTag[]
    {
        GameplayTagFlags.ATTACKING_DISABLED,
        GameplayTagFlags.ATTACKING_EFFECTED,
    };
}

public class KnockbackImmunity_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.KNOCKBACK_IMMUNITY;

    public override RawGameplayTag[] flags => new RawGameplayTag[]
    {
        GameplayTagFlags.KNOCKBACK_IMMUNITY,
    };
}

public class Invulnerability_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.INVULERNABILITY;

    public override RawGameplayTag[] flags => new RawGameplayTag[]
    {
        GameplayTagFlags.INVULNERABLE,
    };
}