using UnityEngine;

public delegate void HealthValueSetDelegate(float newCurrent, float newMax);
public delegate void MutableHealthChangeDelegate(HealthChangeEventData hced);
public delegate void HealthChangeNotificationDelegate(HealthChangeNotificationData hcnd);
public delegate void KilledCharacterDelegate(GameObject killed);

public class HealthChangeEventData
{
    public GameObject overallSource;
    public GameObject localSource;
    public GameObject target;
    public float delta;
    public bool cancelled;
    /// <summary>
    /// Constructor for a HealthChangeEventData. Represents one instance of taking or healing damage.
    /// </summary>
    /// <param name="o">The overall source</param>
    /// <param name="l">The local source</param>
    /// <param name="t">The targeted GameObject</param>
    /// <param name="d">The damage or healing delta</param>
    public HealthChangeEventData(GameObject o, GameObject l, GameObject t, float d)
    {
        overallSource = o;
        localSource = l;
        target = t;
        delta = d;
        cancelled = false;
    }
}

public class HealthChangeNotificationData
{
    public readonly GameObject overallSource;
    public readonly GameObject localSource;
    public readonly GameObject target;
    public readonly float value;
    public readonly float aggroPercentage;

    /// <summary>
    /// Constructor for a HealthChangeNotificaitonData. Represent on instance of taking or healing damage.
    /// </summary>
    /// <param name="o">The overall source</param>
    /// <param name="l">The local source</param>
    /// <param name="v">The amount of healing or damage done. Will be negative for damage, positive for healing.</param>
    /// <param name="a">The aggropercentage, defaulted to 1</param>
    public HealthChangeNotificationData(GameObject o, GameObject l, GameObject t, float v, float a = 1)
    {
        overallSource = o;
        localSource = l;
        target = t;
        value = v;
        aggroPercentage = a;
    }
}

public interface IDamagable
{
    /// <summary>
    /// Damage the object. Implements on a per object basis
    /// </summary>
    /// <param name="delta">The amount of damage</param>
    /// <param name="localSource">The actual gameobject that is applying damage. May be equal to the overallSource</param>
    /// <param name="overallSource"> The source of the damage overall. For example a projectiles local source may be the projectile itself, while the overall source would be the player that spawned it.</param>
    /// <param name="knockbackData"> The knockback to apply, leave null if no knockback is desired
    void Damage(float delta, GameObject localSource, GameObject overallSource = null, KnockbackData knockbackData = null);

    public GameObject attached
    {
        get;
    }
}