using Statuses;
public class Stun_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.STUN;

    public override GameplayTagFlags[] flags => new GameplayTagFlags[]
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

    public override GameplayTagFlags[] flags => new GameplayTagFlags[]
    {
        GameplayTagFlags.CASTING_DISABLED,
        GameplayTagFlags.CASTING_EFFECTED,
    };
}

public class Root_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.ROOT;

    public override GameplayTagFlags[] flags => new GameplayTagFlags[]
    {
        GameplayTagFlags.MOVEMENT_EFFECTED,
        GameplayTagFlags.MOVEMENT_DISABLED,
    };
}

public class Blind_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.BLIND;

    public override GameplayTagFlags[] flags => new GameplayTagFlags[]
    {
        GameplayTagFlags.ATTACKING_DISABLED,
        GameplayTagFlags.ATTACKING_EFFECTED,
    };
}

public class KnockbackImmunity_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.KNOCKBACK_IMMUNITY;

    public override GameplayTagFlags[] flags => new GameplayTagFlags[]
    {
        GameplayTagFlags.KNOCKBACK_IMMUNITY,
    };
}

public class Invulnerability_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.INVULERNABILITY;

    public override GameplayTagFlags[] flags => new GameplayTagFlags[]
    {
        GameplayTagFlags.INVULNERABLE,
    };
}