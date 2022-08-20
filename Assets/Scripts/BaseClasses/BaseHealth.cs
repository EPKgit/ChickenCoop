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
		if(tagComponent?.tags.Contains(GameplayTagFlags.INVULNERABLE) ?? false)
		{
			DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "DAMAGE CANCELED FROM INVULN");
			return;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "taking damage");
		MutableHealthChangeEventData mutableEventData = new MutableHealthChangeEventData(data);
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "pre damage");
		preDamageEvent(mutableEventData);
		if(mutableEventData.cancelled)
		{
			DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "cancelled");
			return;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "not cancelled");
		currentHealth += mutableEventData.delta;
		float aggroValue = data.overallSource?.GetComponent<StatBlockComponent>()?.GetValue(StatName.AggroPercentage) ?? 1;
        if (data.knockbackData != null && knockbackHandler != null)
        {
			if(data.knockbackData.direction == Vector2.zero)
			{
                data.knockbackData.direction = (knockbackHandler.position - data.localSource.transform.position).normalized;
            }
            knockbackHandler.DoKnockback(data.knockbackData);
        }
		postDamageEvent(data);
		data.overallSource?.GetComponent<IHealthCallbacks>()?.DamageDealtCallback(data);
		healthChangeEvent(data);
		
		if(currentHealth <= 0)
		{
			Die(data.overallSource);			
		}
	}

	public void Heal(HealthChangeData data)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "healing");
		MutableHealthChangeEventData mutableEventData = new MutableHealthChangeEventData(data);
		preHealEvent(mutableEventData);
		if(mutableEventData.cancelled)
		{
			return;
		}
		currentHealth += mutableEventData.delta;
		float aggroValue = data.overallSource?.GetComponent<StatBlockComponent>()?.GetValue(StatName.AggroPercentage) ?? 1;
		postHealEvent(data);
		healthChangeEvent(data);
		data.overallSource?.GetComponent<IHealthCallbacks>()?.DamageHealedCallback(data);
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
