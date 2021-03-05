using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealthCallbacks
{
	event HealthChangeNotificationDelegate onDealDamage;
	event HealthChangeNotificationDelegate onHealDamage;

	void RegisterDealDamageDelegate(HealthChangeNotificationDelegate hcnd);
	void RegisterHealDamageDelegate(HealthChangeNotificationDelegate hcnd);
	void DeregisterDealDamageDelegate(HealthChangeNotificationDelegate hcnd);
	void DeregisterHealDamageDelegate(HealthChangeNotificationDelegate hcnd);
    void DamageDealtCallback(HealthChangeNotificationData hcnd);
	void DamageHealedCallback(HealthChangeNotificationData hcnd);
}
