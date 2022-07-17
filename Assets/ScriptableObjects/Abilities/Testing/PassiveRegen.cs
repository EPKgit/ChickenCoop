using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveRegen : Ability
{
	public float hpPerSecond;

	private float healInterval;
	private float timeSinceLastHeal;
	private IHealable hp;
	private GameObject owner;

    public override string GetTooltip()
    {
        return string.Format(tooltipDescription);
    }
    public override void Initialize(PlayerAbilities pa)
	{
		hp = Lib.FindUpwardsInTree<IHealable>(pa.gameObject);
		owner = pa.gameObject;
		healInterval = 1f / hpPerSecond;
	}

	public override bool Tick(float deltaTime)
	{
		timeSinceLastHeal += deltaTime;
		if(timeSinceLastHeal >= healInterval)
		{
			hp.Heal(1, owner);
			timeSinceLastHeal = 0;
		}
		return false;
	}   
}
