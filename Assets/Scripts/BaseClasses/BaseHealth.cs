using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHealth : MonoBehaviour, IHealable, IDamagable
{
	public event HealthValueSetDelegate healthValueUpdateEvent = delegate { };
	public event MutableHealthChangeDelegate preDamageEvent = delegate { };
	public event MutableHealthChangeDelegate preHealEvent = delegate { };
	public event HealthChangeNotificationDelegate postDamageEvent = delegate { };
	public event HealthChangeNotificationDelegate postHealEvent = delegate { };
	public event HealthChangeNotificationDelegate healthChangeEvent = delegate { };
	
	public float maxHealth;

	private float currentHealth;
	private StatBlock stats;

    void Awake()
    {
		stats = GetComponent<StatBlock>();
		maxHealth = stats?.GetValue(StatName.Toughness) ?? maxHealth;
		currentHealth = maxHealth;
		stats?.RegisterInitializationCallback(UpdateMaxHealth);
    }

	void OnDisable()
	{
		stats?.GetStat(StatName.Toughness)?.UnregisterStatChangeCallback(UpdateMaxHealth);	
		stats?.DeregisterInitializationCallback(UpdateMaxHealth);
	}

	public void UpdateMaxHealth(float value)
	{
		currentHealth = currentHealth / maxHealth * value;
		maxHealth = value;
		healthValueUpdateEvent(currentHealth, maxHealth);
	}

	public void UpdateMaxHealth(StatBlock s)
	{
		float value = s.GetValue(StatName.Toughness);
		if(value == -1)
		{
			return;
		}
		stats?.GetStat(StatName.Toughness)?.RegisterStatChangeCallback(UpdateMaxHealth);
		stats = s;
		UpdateMaxHealth(value);
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

	public void Damage(float delta, GameObject localSource, GameObject overallSource = null)
	{
		if(DEBUGFLAGS.HEALTH) Debug.Log("taking damage");
		HealthChangeEventData data = new HealthChangeEventData(overallSource, localSource, gameObject, delta);
		if(DEBUGFLAGS.HEALTH) Debug.Log("pre damage");
		preDamageEvent(data);
		if(data.cancelled)
		{
			if(DEBUGFLAGS.HEALTH) Debug.Log("cancelled");
			return;
		}
		if(DEBUGFLAGS.HEALTH) Debug.Log("not cancelled");
		currentHealth -= data.delta;
		float aggroValue = overallSource?.GetComponent<StatBlock>()?.GetStat(StatName.AggroPercentage)?.value ?? 1;
		HealthChangeNotificationData notifData = new HealthChangeNotificationData(overallSource, localSource, gameObject, data.delta, aggroValue);
		postDamageEvent(notifData);
		overallSource?.GetComponent<IHealthCallbacks>()?.DamageDealtCallback(notifData);
		notifData = new HealthChangeNotificationData(overallSource, localSource, gameObject, -data.delta, aggroValue);
		healthChangeEvent(notifData);
		
		if(currentHealth <= 0)
		{
			Die();
		}
	}

	public void Heal(float delta, GameObject localSource, GameObject overallSource = null)
	{
		if(DEBUGFLAGS.HEALTH) Debug.Log("healing");
		HealthChangeEventData data = new HealthChangeEventData(overallSource, localSource, gameObject, delta);
		preHealEvent(data);
		if(data.cancelled)
		{
			return;
		}
		currentHealth += data.delta;
		float aggroValue = overallSource?.GetComponent<StatBlock>()?.GetStat(StatName.AggroPercentage)?.value ?? 1;
		HealthChangeNotificationData notifData = new HealthChangeNotificationData(overallSource, localSource, gameObject, delta, aggroValue);
		postHealEvent(notifData);
		healthChangeEvent(notifData);
		overallSource?.GetComponent<IHealthCallbacks>()?.DamageHealedCallback(notifData);
		if(currentHealth > maxHealth)
		{
			currentHealth = maxHealth;
		}
	}

	protected virtual void Die()
	{
		transform.position = Vector3.zero;
		currentHealth = maxHealth;
		healthValueUpdateEvent(currentHealth, maxHealth);
	}

	public float GetCurrentHealth()
	{
		return currentHealth;
	}

	public float GetMaxHealth()
	{
		return maxHealth;
	}
}

public delegate void HealthValueSetDelegate(float newCurrent, float newMax);
public delegate void MutableHealthChangeDelegate(HealthChangeEventData hced);
public delegate void HealthChangeNotificationDelegate(HealthChangeNotificationData hcnd);

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
