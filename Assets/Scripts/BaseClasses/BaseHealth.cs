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
        maxHealth = stats.GetValue(StatName.Toughness);
        currentHealth = maxHealth;
        knockbackHandler = Lib.FindDownThenUpwardsInTree<IKnockbackHandler>(gameObject);
    }

	protected virtual void OnEnable()
	{
        stats.RegisterStatChangeCallback(StatName.Toughness, UpdateMaxHealth);
    }

	protected virtual void OnDisable()
	{
		stats.DeregisterStatChangeCallback(StatName.Toughness, UpdateMaxHealth);
	}

	public void UpdateMaxHealth(float value)
	{
		currentHealth = currentHealth / maxHealth * value;
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

	public void Damage(float delta, GameObject localSource, GameObject overallSource = null, KnockbackData knockbackData = null)
	{
		if(tagComponent?.tags.Contains(GameplayTagFlags.INVULNERABLE) ?? false)
		{
			DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "DAMAGE CANCELED FROM INVULN");
			return;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "taking damage");
		HealthChangeEventData data = new HealthChangeEventData(overallSource, localSource, gameObject, delta);
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "pre damage");
		preDamageEvent(data);
		if(data.cancelled)
		{
			DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "cancelled");
			return;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "not cancelled");
		currentHealth -= data.delta;
		float aggroValue = overallSource?.GetComponent<StatBlockComponent>()?.GetStat(StatName.AggroPercentage)?.Value ?? 1;
        if (knockbackData != null && knockbackHandler != null)
        {
			if(knockbackData.direction == Vector2.zero)
			{
                knockbackData.direction = (knockbackHandler.position - localSource.transform.position).normalized;
            }
            knockbackHandler.DoKnockback(knockbackData);
        }
        HealthChangeNotificationData notifData = new HealthChangeNotificationData(overallSource, localSource, gameObject, data.delta, aggroValue);
		postDamageEvent(notifData);
		overallSource?.GetComponent<IHealthCallbacks>()?.DamageDealtCallback(notifData);
		notifData = new HealthChangeNotificationData(overallSource, localSource, gameObject, -data.delta, aggroValue);
		healthChangeEvent(notifData);
		
		if(currentHealth <= 0)
		{
			Die(overallSource);			
		}
	}

	public void Heal(float delta, GameObject localSource, GameObject overallSource = null)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTH, "healing");
		HealthChangeEventData data = new HealthChangeEventData(overallSource, localSource, gameObject, delta);
		preHealEvent(data);
		if(data.cancelled)
		{
			return;
		}
		currentHealth += data.delta;
		float aggroValue = overallSource?.GetComponent<StatBlockComponent>()?.GetStat(StatName.AggroPercentage)?.Value ?? 1;
		HealthChangeNotificationData notifData = new HealthChangeNotificationData(overallSource, localSource, gameObject, delta, aggroValue);
		postHealEvent(notifData);
		healthChangeEvent(notifData);
		overallSource?.GetComponent<IHealthCallbacks>()?.DamageHealedCallback(notifData);
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
