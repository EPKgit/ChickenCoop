using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth), typeof(Rigidbody2D))]
public abstract class BaseEnemy : BaseMovement
{
    public abstract EnemyType type { get; }
    public float speed = 1f;

	/// <summary>
    /// Setting to true will enable default 
    /// </summary>
    public bool doesContactDamage = true;


    /// <summary>
    /// Describes whether the enemy has been disabled for any reason or another. This will en/disable colliders
    /// and the update method
    /// </summary>
    protected bool enemyEnabled = true;

	/// <summary>
	/// The player that the AI will use to plan its pathing, up to the AI
	/// to use this value
	/// </summary>
	protected GameObject chosenPlayer;

#if !REMOVE_AGGRO_DATA
	/// <summary>
	/// A sorted set of all aggrodata that the enemy has recieved from damage events. 
	/// </summary>
	protected PriorityQueue<AggroData> aggro;
#endif

    protected Collider2D col;
	protected EnemyHealth hp;
    protected TargetingController targetingController;


    protected override void Awake()
	{
        base.Awake();
		hp = GetComponent<EnemyHealth>();
		col = GetComponent<Collider2D>();
        targetingController = Lib.FindUpwardsInTree<TargetingController>(gameObject);

#if !REMOVE_AGGRO_DATA
        aggro = new PriorityQueue<AggroData>(PlayerInitialization.all.Count, new MaxAggroComparator());

        hp.postDamageEvent += AddAggroEvent;
#endif
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


#if !REMOVE_AGGRO_DATA
    protected virtual void AddAggroEvent(HealthChangeData hcd)
	{
		AggroData temp = aggro.GetValue( (t) => t.source == hcd.overallSource);
		if(temp == null)
		{
			DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.AGGRO, "new aggro");
			aggro.Push(new AggroData(hcd));
			UpdateChosenPlayer();
			return;
		}
        float aggroValue = StatBlockComponent.GetValueOrDefault(hcd.overallSource, StatName.AggroPercentage);
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.AGGRO, string.Format("update aggro from {0} to {1}", temp.value, temp.value + -hcd.delta * aggroValue));
		temp.value += -hcd.delta * aggroValue;
		UpdateChosenPlayer();
	}
#endif

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
#if REMOVE_AGGRO_DATA
		float closestDistance = float.MaxValue;
        PlayerInitialization bestOption = null;
        foreach(PlayerInitialization player in PlayerInitialization.all)
        {
			float dist = Vector2.Distance(transform.position, player.transform.position);
			if (dist < closestDistance)
			{
				bestOption = player;
				closestDistance = dist;
			}
        }
		chosenPlayer = bestOption.gameObject;
#else
        chosenPlayer = aggro?.Peek()?.source ?? PlayerInitialization.all[Random.Range(0, PlayerInitialization.all.Count)].gameObject;
#endif
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.AGGRO, chosenPlayer?.name ?? "NO AGGRO");
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
        CheckKnockbackInput();
        return true;
	}

	private void OnCollisionEnter2D(Collision2D other) 
	{
        Attack(other.gameObject);
    }

	private void OnCollisionStay2D(Collision2D other) 
	{
        Attack(other.gameObject);
    }

	void Attack(GameObject g)
	{
		TargetingController controller = Lib.FindUpwardsInTree<TargetingController>(g);
        if (controller != null)
        {
            if ((controller.TargetAffiliation & targetingController.TargetAffiliation) == 0)
            {
                IDamagable damagable = Lib.FindDownThenUpwardsInTree<IDamagable>(g);
                if (damagable != null)
                {
                    damagable.Damage
					(
                        HealthChangeData.GetBuilder()
                            .Damage(1)
                            .BothSources(gameObject)
                            .KnockbackData(KnockbackPreset.MEDIUM)
                    		.Target(damagable)
                            .Finalize()
                    );
                }
            }
        }
	}

#if !REMOVE_AGGRO_DATA
	public AggroData[] GetAggroDataArray()
	{
		return aggro?.ToArray();
	}
#endif
}