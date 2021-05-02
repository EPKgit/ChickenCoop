using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth), typeof(Rigidbody2D))]
public abstract class BaseEnemy : MonoBehaviour
{
  	public float speed = 1f;

	/// <summary>
	/// The player that the AI will use to plan its pathing, up to the AI
	/// to use this value
	/// </summary>
	protected GameObject chosenPlayer;

	/// <summary>
	/// A sorted set of all aggrodata that the enemy has recieved from damage events. 
	/// </summary>
	protected PriorityQueue<AggroData> aggro;
	
	protected Rigidbody2D rb;
	protected StatBlockComponent stats;
	protected EnemyHealth hp;

	protected virtual void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		stats = GetComponent<StatBlockComponent>();
		hp = GetComponent<EnemyHealth>();
		aggro = new PriorityQueue<AggroData>(PlayerInitialization.all.Count, new MaxAggroComparator());
		
		hp.postDamageEvent += AddAggroEvent;
	}

	protected virtual void Start()
	{
		UpdateChosenPlayer();
	}

	protected virtual void AddAggroEvent(HealthChangeNotificationData hcnd)
	{
		AggroData temp = aggro.GetValue( (t) => t.source == hcnd.overallSource);
		if(temp == null)
		{
			DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.AGGRO, "new aggro");
			aggro.Push(new AggroData(hcnd));
			UpdateChosenPlayer();
			return;
		}
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.AGGRO, string.Format("update aggro from {0} to {1}", temp.value, temp.value + hcnd.value * hcnd.aggroPercentage));
		temp.value += hcnd.value * hcnd.aggroPercentage;
		UpdateChosenPlayer();
	}

	/// <summary>
	/// Decides which player the enemy will choose to attack
	/// </summary>
	/// <returns>Returns true if there is a player, false if one cannot be chosen</returns>
	protected virtual bool UpdateChosenPlayer()
	{
		if(PlayerInitialization.all.Count == 0)
		{
			return false;
		}
		chosenPlayer = aggro?.Peek()?.source ?? PlayerInitialization.all[Random.Range(0, PlayerInitialization.all.Count)].gameObject;
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.AGGRO, chosenPlayer.name);
		return true;
	}

	//Not that performant, can be updated if we need to
	//Should add an aggro range 
	protected virtual bool Update()
	{
		if(chosenPlayer == null)
		{
			return UpdateChosenPlayer();
		}
		return true;
	}

	public AggroData[] GetAggroDataArray()
	{
		return aggro?.ToArray();
	}
}