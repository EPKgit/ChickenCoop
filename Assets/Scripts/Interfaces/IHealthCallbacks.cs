using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealthCallbacks
{
	event HealthChangeNotificationDelegate OnDealDamage;
	event HealthChangeNotificationDelegate OnHealDamage;
    event KilledCharacterDelegate OnKill;
    void DamageDealtCallback(HealthChangeNotificationData hcnd);
	void DamageHealedCallback(HealthChangeNotificationData hcnd);

    void KillCallback(GameObject killed);
}
