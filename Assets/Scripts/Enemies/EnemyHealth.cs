using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : BaseHealth
{
	public GameObject RedSplodeEffect;
	public GameObject WhiteSplodeEffect;

  	protected override void Die()
	{
		Destroy(gameObject);
    	DieEffect();
	}

	private void DieEffect()
    {
		//GameObject splode = Instantiate(RedSplodeEffect, transform.position, Quaternion.identity);
	}
}
