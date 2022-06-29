using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth), typeof(Rigidbody2D), typeof(GameplayTagComponent))]
public abstract class BaseEnemy : MonoBehaviour
{
    public EnemyType type = EnemyType.MAX;
    public float speed = 1f;

	/// <summary>
    /// Describes whether the enemy has 
    /// </summary>
	protected bool enemyEnabled = true;

	/// <summary>
	/// The player that the AI will use to plan its pathing, up to the AI
	/// to use this value
	/// </summary>
	protected GameObject chosenPlayer;

	/// <summary>
	/// A sorted set of all aggrodata that the enemy has recieved from damage events. 
	/// </summary>
	protected PriorityQueue<AggroData> aggro;

    protected Collider2D col;
    protected Rigidbody2D rb;
	protected StatBlockComponent stats;
	protected EnemyHealth hp;
    protected GameplayTagComponent tagComponent;


    protected virtual void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		stats = GetComponent<StatBlockComponent>();
		hp = GetComponent<EnemyHealth>();
		col = GetComponent<Collider2D>();
		tagComponent = GetComponentInChildren<GameplayTagComponent>();
        aggro = new PriorityQueue<AggroData>(PlayerInitialization.all.Count, new MaxAggroComparator());
		
		hp.postDamageEvent += AddAggroEvent;
	}

	protected virtual void Start()
	{
		UpdateChosenPlayer();
	}

	public virtual void SetEnemyEnabled(bool enabled)
	{
        enemyEnabled = enabled;
        col.enabled = enabled;
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
	// returns false if we shouldn't do anything
	protected virtual bool Update()
	{
		if(!enemyEnabled)
		{
            return false;
        }
		if(chosenPlayer == null)
		{
			return UpdateChosenPlayer();
		}
		return true;
	}

	protected virtual bool CanMove()
	{
		if(tagComponent?.tags.Contains(GameplayTagFlags.MOVEMENT_DISABLED) ?? false)
		{
            return false;
        }
        return true;
    }

	public AggroData[] GetAggroDataArray()
	{
		return aggro?.ToArray();
	}
}