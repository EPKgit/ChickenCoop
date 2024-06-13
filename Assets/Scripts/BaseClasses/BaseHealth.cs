using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(GameplayTagComponent), typeof(StatBlockComponent))]
public class BaseHealth : MonoBehaviour, IHealable, IDamagable, IShieldable
{
    public event HealthValueSetDelegate healthValueUpdateEvent = delegate { };

	public event MutableHealthChangeDelegate preDamageEvent = delegate { };
	public event MutableHealthChangeDelegate preHealEvent = delegate { };
	public event MutableShieldApplicationDelegate preShieldEvent = delegate { };

	public event ShieldAbsorbedDamageNotificationDelegate shieldAbsorbedDamageNotification = delegate { };

    public event ShieldApplicationNotificationDelegate postShieldAppliedNotification = delegate { };
	public event HealthChangeNotificationDelegate postDamageNotification = delegate { };
	public event HealthChangeNotificationDelegate postHealNotification = delegate { };
	public event HealthChangeNotificationDelegate healthChangeNotification = delegate { };
	
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

	public float currentShield
	{
		get;
		protected set;
	} = -1;

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

	public void Damage(HealthChangeData hcData)
	{
		if(!hcData.Valid)
        {
			throw new System.Exception("ERROR Invalid HealthChangeData used in damage method");
        }

		if(tagComponent?.tags.Contains(GameplayTagFlags.INVULNERABLE) ?? false)
		{
			DebugFlags.Log(DebugFlags.Flags.HEALTH, "DAMAGE CANCELED FROM INVULN");
			return;
		}

		DebugFlags.Log(DebugFlags.Flags.HEALTH, "pre damage");

		MutableHealthChangeEventData mutableEventData = new MutableHealthChangeEventData(hcData, HasShield());
		preDamageEvent(mutableEventData); //this is sent out to see if anyone wants to cancel the damage for any reason e.g. invuln
		if(mutableEventData.cancelled)
		{
			DebugFlags.Log(DebugFlags.Flags.HEALTH, "cancelled");
			return;
		}

		if(HasShield())
		{
            ShieldAbsorbtionData saData = new ShieldAbsorbtionData();
			saData.amountAbsorbed = Math.Min(currentShield, mutableEventData.delta);

            DebugFlags.Log(DebugFlags.Flags.HEALTH, $"hitting shield of {currentShield} for {mutableEventData.delta}");
            currentShield += mutableEventData.delta;

			

            if (currentShield <= 0)
			{
				DebugFlags.Log(DebugFlags.Flags.HEALTH, $"broke shield");

				saData.remainingValue = 0;
				saData.broken = true;
				currentShield = -1;
			}
			else
			{
                DebugFlags.Log(DebugFlags.Flags.HEALTH, $"shield survived with {currentShield}");

				saData.remainingValue = currentShield;
				saData.broken = false;
            }
			shieldAbsorbedDamageNotification(saData);
			return;
        }

        DebugFlags.Log(DebugFlags.Flags.HEALTH, $"taking {mutableEventData.delta}");
		currentHealth += mutableEventData.delta;
        
		if (hcData.KnockbackData != null && knockbackHandler != null)
        {
			if(hcData.KnockbackData.direction == Vector2.zero)
			{
                hcData.KnockbackData.direction = (knockbackHandler.position - hcData.BilateralData.LocalSource.transform.position).normalized;
            }
            knockbackHandler.ApplyKnockback(hcData.KnockbackData);
        }
		
		postDamageNotification(hcData);
		hcData.BilateralData.OverallSource?.GetComponent<IHealthCallbacks>()?.DamageDealtCallback(hcData);
		healthChangeNotification(hcData);
		
		if(currentHealth <= 0)
		{
			Die(hcData.BilateralData.OverallSource);			
		}
	}

	public void Heal(HealthChangeData data)
	{
		DebugFlags.Log(DebugFlags.Flags.HEALTH, "healing");
		MutableHealthChangeEventData mutableEventData = new MutableHealthChangeEventData(data, HasShield());
		preHealEvent(mutableEventData);
		if(mutableEventData.cancelled)
		{
			return;
		}
		currentHealth += mutableEventData.delta;
		float aggroValue = data.BilateralData.OverallSource?.GetComponent<StatBlockComponent>()?.GetValue(StatName.AggroPercentage) ?? 1;
		postHealNotification(data);
		healthChangeNotification(data);
		data.BilateralData.OverallSource?.GetComponent<IHealthCallbacks>()?.DamageHealedCallback(data);
		if(currentHealth > maxHealth)
		{
			currentHealth = maxHealth;
		}
	}

    public void ApplyShield(ShieldApplicationData data)
    {
        DebugFlags.Log(DebugFlags.Flags.HEALTH, "applying shield");
        MutableShieldApplicationEventData mutableEventData = new MutableShieldApplicationEventData(data);
        preShieldEvent(mutableEventData);
        if (mutableEventData.cancelled)
        {
            return;
        }

		if(HasShield())
		{
			if(currentShield > data.Value)
			{
				return;
			}
		}

        currentShield = mutableEventData.value;

        postShieldAppliedNotification(data);

		// TODO do shielding callbacks
    }

    protected virtual void Die(GameObject killer = null)
	{
		currentHealth = maxHealth;
		healthValueUpdateEvent(currentHealth, maxHealth);
		killer?.GetComponent<IHealthCallbacks>()?.KillCallback(gameObject);
	}    

	public bool HasShield()
	{
		return currentShield > 0;
	}
}
