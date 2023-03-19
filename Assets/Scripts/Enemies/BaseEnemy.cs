using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyBehaviourSyntax;

[RequireComponent(typeof(EnemyHealth))]
public abstract class BaseEnemy : BaseMovement
{
    public abstract EnemyType type { get; }
    public float speed = 1f;

	[HideInInspector]
	public bool isRegistered;

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

	/// <summary>
	/// List of commands that are run every frame for the enemy, empty by default but dervied classes can fill it
	/// see NormalEnemy.cs
	/// </summary>
	protected List<EnemyBehaviourAction> updateList;

	/// <summary>
	/// List of springs that affect default movement for the enemy, empty by default but derived classes can fill it
	/// </summary>
	protected List<EnemySpring> movementSprings;

	protected SpringData currentFrameSpringData;

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
		isRegistered = EnemyManager.instance.IsRegistered(gameObject);
		if (!isRegistered)
		{
            EnemyManager.instance.RegisterEnemy(gameObject);
        }
        base.Awake();
		hp = GetComponent<EnemyHealth>();
		col = GetComponent<Collider2D>();
        targetingController = Lib.FindUpwardsInTree<TargetingController>(gameObject);
		updateList = new List<EnemyBehaviourAction>();
		movementSprings = new List<EnemySpring>();

#if !REMOVE_AGGRO_DATA
        aggro = new PriorityQueue<AggroData>(PlayerInitialization.all.Count, new MaxAggroComparator());

        hp.postDamageEvent += AddAggroEvent;
#endif
	}

	protected virtual void Start()
	{
		UpdateChosenPlayer();
	}

	protected override void Update()
	{
		base.Update();
		ProcessUpdateList();
	}

	protected void ProcessUpdateList()
	{
		for (int x = 0; x < updateList.Count; ++x)
		{
			EnemyBehaviourAction action = updateList[x];
			if (!action.Evaluate())
			{
				break;
			}
		}
	}

	protected virtual void OnDisable()
	{
        EnemyManager.instance?.UnregisterEnemy(gameObject);
    }

	public virtual void SetEnemyEnabled(bool enabled)
	{
		if(enabled)
		{
            UpdateChosenPlayer();
        }
        enemyEnabled = enabled;
        col.enabled = enabled;
		rb.velocity = Vector2.zero;
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
	public virtual bool UpdateChosenPlayer()
	{
		if(PlayerInitialization.all.Count == 0)
		{
			return true;
		}
#if REMOVE_AGGRO_DATA
		float closestDistance = float.MaxValue;
        PlayerInitialization bestOption = null;
        foreach(PlayerInitialization player in PlayerInitialization.all)
        {
            if (!player.GetComponent<GameplayTagComponent>().tags.Contains(GameplayTagFlags.INVISIBLE))
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist < closestDistance)
                {
                    bestOption = player;
                    closestDistance = dist;
                }
            }
        }
		chosenPlayer = bestOption?.gameObject;
#else
        chosenPlayer = aggro?.Peek()?.source ?? PlayerInitialization.all[Random.Range(0, PlayerInitialization.all.Count)].gameObject;
#endif
        DebugFlags.Log(DebugFlags.Flags.AGGRO, chosenPlayer?.name ?? "NO AGGRO");
		return chosenPlayer == null;
	}

	protected virtual bool IsDisabled()
    {
		return !enemyEnabled;
	}

	protected virtual bool HasNoValidTarget()
    {
		if (chosenPlayer == null)
		{
			return UpdateChosenPlayer();
		}
		return false;
	}

	protected virtual void AnimationUpdate()
    {
		animator.SetBool(animatorMovingHashCode, movingAtStartOfFrame);
		if (movingAtStartOfFrame)
		{
			sprite.flipX = rb.velocity.x < 0;
		}
	}


	//Not that performant, can be updated if we need to
	//Should add an aggro range 
	// returns false if we shouldn't do anything
	protected virtual bool UpdateKnockback()
	{
        CheckKnockbackInput();
		if (tagComponent.tags.Contains(GameplayTagFlags.KNOCKBACK))
		{
			DebugFlags.Log(DebugFlags.Flags.MOVEMENT, "MOVEMENT CANCELED FROM KNOCKBAC");
			return false;
		}
		return true;
	}

	protected virtual void Move()
	{
		currentFrameSpringData = new SpringData()
		{
			allPlayers = PlayerInitialization.all,
			targetedPlayer = chosenPlayer,
			attached = gameObject,
			inRange = Physics2D.OverlapCircleAll(transform.position, 20)
		};
		Vector2 dir = Vector2.zero;
		foreach(EnemySpring spring in movementSprings)
        {
			dir += spring.EvaluateDirection(currentFrameSpringData);
        }
		rb.velocity = dir.normalized * speed;
	}

	private void OnCollisionEnter2D(Collision2D other) 
	{
        Attack(other.gameObject);
    }

	private void OnCollisionStay2D(Collision2D other) 
	{
        Attack(other.gameObject);
    }

	protected virtual void Attack(GameObject g)
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

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
    {
		foreach(EnemySpring spring in movementSprings)
        {
			spring.DrawGizmo();
        }
	}
#endif
}