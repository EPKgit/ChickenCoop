using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCallbacks : MonoBehaviour, IHealthCallbacks
{
	public event HealthChangeNotificationDelegate OnDealDamage = delegate { };
	public event HealthChangeNotificationDelegate OnHealDamage = delegate { };
	public event KilledCharacterDelegate OnKill = delegate { };

    public void DamageDealtCallback(HealthChangeData hcd)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTHCALLBACKS, gameObject.name + " dealt " + -hcd.delta + " to " + hcd.target);
		OnDealDamage(hcd);
	}

	public void DamageHealedCallback(HealthChangeData hcd)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTHCALLBACKS, gameObject.name + " healed " + hcd.delta + " to " + hcd.target);
		OnHealDamage(hcd);
	}

	public void KillCallback(GameObject killed)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.HEALTHCALLBACKS, gameObject.name + " killed " + killed.name);
        OnKill(killed);
    }
}
