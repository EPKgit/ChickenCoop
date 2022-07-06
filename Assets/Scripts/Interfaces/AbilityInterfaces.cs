public interface IDamagingAbility
{
    float damage
    {
        get;
        set;
    }
}
public interface IAOEAbility
{
    float aoe
    {
        get;
        set;
    }
}

public interface IMovementAbility
{
    MovementType type
    {
        get;
    }
}

public interface IHealingAbility
{
    float amount
    {
        get;
        set;
    }
}
