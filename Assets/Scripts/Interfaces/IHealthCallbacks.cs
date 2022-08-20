using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealthCallbacks
{
	event HealthChangeNotificationDelegate OnDealDamage;
	event HealthChangeNotificationDelegate OnHealDamage;
    event KilledCharacterDelegate OnKill;
    void DamageDealtCallback(HealthChangeData hcnd);
	void DamageHealedCallback(HealthChangeData hcnd);

    void KillCallback(GameObject killed);
}
