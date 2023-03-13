using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(GameplayTagComponent), typeof(StatBlockComponent))]
public class BaseHealth : MonoBehaviour, IHealable, IDamagable
{
    public event HealthValueSetDelegate healthValueUpdateEvent = delegate { };
	public event MutableHealthChangeDelegate preDamageEvent = delegate { };
	public event MutableHealthChangeDelegate preHealEvent = delegate { };
	public event HealthChangeNotificationDelegate postDamageEvent = delegate { };
	public event HealthChangeNotificationDelegate postHealEvent = delegate { };
	public event HealthChangeNotificationDelegate healthChangeEvent = delegate { };
	
	public float maxHealth
    {
        get
        {
            return _maxHealth;
        }
        protected set
        {
            _maxHealth = value;
        }
    }
    [SerializeField]
    private float _maxHealth;

	public float currentHealth
    {
        get;
        protected set;
    }
    public GameObject attached { get => gameObject; }

    private StatBlockComponent stats;
	private GameplayTagComponent tagComponent;
    private IKnockbackHandler knockbackHandler;

    protected virtual void Awake()
    {
		stats = GetComponent<StatBlockComponent>();
		tagComponent = GetComponent<GameplayTagComponent>();
        if (!stats.HasStat(StatName.MaxHealth))
        {
            stats.AddStat(StatName.MaxHealth, maxHealth);
        }
        maxHealth = stats.GetValue(StatName.MaxHealth);
        currentHealth = maxHealth;
        knockbackHandler = Lib.FindDownThenUpwardsInTree<IKnockbackHandler>(gameObject);
    }

	protected virtual void OnEnable()
	{
        stats.RegisterStatChangeCallback(StatName.MaxHealth, UpdateMaxHealth);
    }

	protected virtual void OnDisable()
	{
		stats.DeregisterStatChangeCallback(StatName.MaxHealth, UpdateMaxHealth);
	}

	public void UpdateMaxHealth(float value)
	{
        float t;
        if(maxHealth <= 0)
		{
            t = 1;
        }
		else
		{
            t = currentHealth / maxHealth;
        }
		currentHealth = t * value;
		maxHealth = value;
		healthValueUpdateEvent(currentHealth, maxHealth);
	}

	public void SetHealth(float delta)
	{
		currentHealth += delta;
		if(currentHealth <= 0)
		{
			Die();
		}
		if(currentHealth > maxHealth)
		{
			currentHealth = maxHealth;
		}
		healthValueUpdateEvent(currentHealth, maxHealth);
	}

	public void Damage(HealthChangeData data)
	{
		if(!data.Valid)
        {
			throw new System.Exception("ERROR Invalid HealthChangeData used in damage method");
        }
		if(tagComponent?.tags.Contains(GameplayTagFlags.INVULNERABLE) ?? false)
		{
			DebugFlags.Log(DebugFlags.Flags.HEALTH, "DAMAGE CANCELED FROM INVULN");
			return;
		}
		DebugFlags.Log(DebugFlags.Flags.HEALTH, "taking damage");
		MutableHealthChangeEventData mutableEventData = new MutableHealthChangeEventData(data);
		DebugFlags.Log(DebugFlags.Flags.HEALTH, "pre damage");
		preDamageEvent(mutableEventData);
		if(mutableEventData.cancelled)
		{
			DebugFlags.Log(DebugFlags.Flags.HEALTH, "cancelled");
			return;
		}
		DebugFlags.Log(DebugFlags.Flags.HEALTH, "not cancelled");
		currentHealth += mutableEventData.delta;
		float aggroValue = data.OverallSource?.GetComponent<StatBlockComponent>()?.GetValue(StatName.AggroPercentage) ?? 1;
        if (data.KnockbackData != null && knockbackHandler != null)
        {
			if(data.KnockbackData.direction == Vector2.zero)
			{
                data.KnockbackData.direction = (knockbackHandler.position - data.LocalSource.transform.position).normalized;
            }
            knockbackHandler.ApplyKnockback(data.KnockbackData);
        }
		postDamageEvent(data);
		data.OverallSource?.GetComponent<IHealthCallbacks>()?.DamageDealtCallback(data);
		healthChangeEvent(data);
		
		if(currentHealth <= 0)
		{
			Die(data.OverallSource);			
		}
	}

	public void Heal(HealthChangeData data)
	{
		DebugFlags.Log(DebugFlags.Flags.HEALTH, "healing");
		MutableHealthChangeEventData mutableEventData = new MutableHealthChangeEventData(data);
		preHealEvent(mutableEventData);
		if(mutableEventData.cancelled)
		{
			return;
		}
		currentHealth += mutableEventData.delta;
		float aggroValue = data.OverallSource?.GetComponent<StatBlockComponent>()?.GetValue(StatName.AggroPercentage) ?? 1;
		postHealEvent(data);
		healthChangeEvent(data);
		data.OverallSource?.GetComponent<IHealthCallbacks>()?.DamageHealedCallback(data);
		if(currentHealth > maxHealth)
		{
			currentHealth = maxHealth;
		}
	}

	protected virtual void Die(GameObject killer = null)
	{
		currentHealth = maxHealth;
		healthValueUpdateEvent(currentHealth, maxHealth);
		killer?.GetComponent<IHealthCallbacks>()?.KillCallback(gameObject);
	}
}
