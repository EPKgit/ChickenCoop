using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCallbacks : MonoBehaviour, IHealthCallbacks
{
	public event HealthChangeNotificationDelegate onDealDamage = delegate { };
	public event HealthChangeNotificationDelegate onHealDamage = delegate { };

	public void RegisterDealDamageDelegate(HealthChangeNotificationDelegate hcnd)
	{
		onDealDamage += hcnd;
	}

	public void RegisterHealDamageDelegate(HealthChangeNotificationDelegate hcnd)
	{
		onHealDamage += hcnd;
	}

	public void DeregisterDealDamageDelegate(HealthChangeNotificationDelegate hcnd)
	{
		onDealDamage -= hcnd;
	}

	public void DeregisterHealDamageDelegate(HealthChangeNotificationDelegate hcnd)
	{
		onHealDamage -= hcnd;
	}

    public void DamageDealtCallback(HealthChangeNotificationData hcnd)
	{
		if(DEBUGFLAGS.HEALTHCALLBACKS) Debug.Log(gameObject.name + " dealt " + hcnd.value + " to " + hcnd.target);
		onDealDamage(hcnd);
	}

	public void DamageHealedCallback(HealthChangeNotificationData hcnd)
	{
		if(DEBUGFLAGS.HEALTHCALLBACKS) Debug.Log(gameObject.name + " healed " + hcnd.value + " to " + hcnd.target);
		onHealDamage(hcnd);
	}
}
