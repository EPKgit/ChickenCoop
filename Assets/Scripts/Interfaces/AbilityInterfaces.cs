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
    float movementDuration
    {
        get;
        set;
    }
    float movementDistance
    {
        get;
        set;
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
