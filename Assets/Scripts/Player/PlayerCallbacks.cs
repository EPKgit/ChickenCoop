using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCallbacks : MonoBehaviour, IHealthCallbacks
{
	public event HealthChangeNotificationDelegate OnDealDamage = delegate { };
	public event HealthChangeNotificationDelegate OnHealDamage = delegate { };
	public event KilledCharacterDelegate OnKill = delegate { };

    public void DamageDealtCallback(HealthChangeNotificationData hcnd)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTHCALLBACKS, gameObject.name + " dealt " + hcnd.value + " to " + hcnd.target);
		OnDealDamage(hcnd);
	}

	public void DamageHealedCallback(HealthChangeNotificationData hcnd)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTHCALLBACKS, gameObject.name + " healed " + hcnd.value + " to " + hcnd.target);
		OnHealDamage(hcnd);
	}

	public void KillCallback(GameObject killed)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTHCALLBACKS, gameObject.name + " killed " + killed.name);
        OnKill(killed);
    }
}
