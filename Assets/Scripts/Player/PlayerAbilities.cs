using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
    public delegate void AbilityInitializationDelegate(AbilitySetContainer abilities);
    public event AbilityInitializationDelegate initializedEvent = delegate { };

    public event AbilityCastingDelegate preAbilityCastEvent = delegate { };
    public event AbilityCastingDelegate postAbilityCastEvent = delegate { };

    public GameObject inventoryUIPrefab;
    private GameObject inventoryGO;

    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public CircleCollider2D col;
    [HideInInspector]
    public StatBlock stats;
    [HideInInspector]
    public BaseHealth hp;
    [HideInInspector]
    public GameplayTagComponent tagComponent;
    [HideInInspector]
    public PlayerMovement movement;
    [HideInInspector]
    public PlayerCollision collision;

    private AbilitySetAsset abilitySet;

    public AbilitySetContainer abilities { get; private set; }

    private List<Ability> passives = new List<Ability>();

    private AbilityQueue abilityQueue;

    private bool initialized = false;

	void Awake()
	{
        rb = GetComponent<Rigidbody2D>();
		col = transform.Find("Colliders").GetComponent<CircleCollider2D>();
		stats = GetComponent<StatBlockComponent>().GetStatBlock();
		hp = GetComponent<BaseHealth>();
		tagComponent = GetComponent<GameplayTagComponent>();
		movement = GetComponent<PlayerMovement>();
        collision = GetComponent<PlayerCollision>();
        inventoryGO = Instantiate(inventoryUIPrefab, GameObject.FindGameObjectWithTag("PlayerUI").transform);
        ToggleInventory();
	}

	public void Initialize(PlayerClass pc)
	{
		Initialize(pc.abilities);
	}


    private void temp1(AbilityEventData aed)
    {
        preAbilityCastEvent(aed);
    }

    public void Initialize(AbilitySetAsset abilitySet)
	{
        abilityQueue = new AbilityQueue();
        abilityQueue.preAbilityCastEvent += (data) => { preAbilityCastEvent(data); };
        abilityQueue.postAbilityCastEvent += (data) => { postAbilityCastEvent(data); };
        passives = new List<Ability>();

        abilitySet = Instantiate(abilitySet);
        for(int x = 0; x < abilitySet.abilities.Length; ++x)
        {
            abilitySet.abilities[x] = ScriptableObject.Instantiate(abilitySet.abilities[x]);
        }
        abilities = new AbilitySetContainer(abilitySet.abilities);
		foreach(Ability a in abilitySet.passiveEffects)
		{
			if(a.isPassive)
			{
				passives.Add(ScriptableObject.Instantiate(a));
			}
			else
			{
				Debug.LogError("Non-passive ability added to passive ability list of " + abilitySet.name);
			}
			
		}
        foreach (Ability a in abilities)
        {
            a.Initialize(this);
        }
        foreach (Ability a in passives)
		{
			a.Initialize(this);
            abilityQueue.AbilityStarted(a);
        }
        initialized = true;
		initializedEvent(abilities);
	}

    /// <summary>
    /// Register cooldown callbacks for all abilities, will be called immediately with the starting values. These values may be 0
    /// </summary>
    /// <param name="callbacks">Array of callbacks. Length should be equals to or less than the number of slots</param>
	public void RegisterAbilityCooldownCallbacks(CooldownTickDelegate[] callbacks)
	{
        if(callbacks.Length > (int)AbilitySlots.MAX)
        {
            throw new Exception("ERROR: Cooldown callback size is greater than the number of slots");
        }
        for(int x = 0; x < callbacks.Length; ++x)
        {
            abilities[x].cooldownTick += callbacks[x];
            callbacks[x](abilities[x].currentCooldown, abilities[x].maxCooldown);
        }
	}

    /// <summary>
    /// Unregister cooldown callbacks for all abilities, will be called immediately with the starting values. These values may be 0
    /// </summary>
    /// <param name="callbacks">Array of callbacks. Length should be equals to or less than the number of slots</param>
	public void UnregisterAbilityCooldownCallbacks(CooldownTickDelegate[] callbacks)
    {
        if (callbacks.Length > (int)AbilitySlots.MAX)
        {
            throw new Exception("ERROR: Cooldown callback size is greater than the number of slots");
        }
        for (int x = 0; x < callbacks.Length; ++x)
        {
            abilities[x].cooldownTick -= callbacks[x];
        }
    }

    public List<string> GetCurrentlyInstantiatedAbilities()
	{
        if(!initialized)
        {
            return new List<string>();
        }
		List<string> ret = new List<string>();
        foreach(Ability a in abilities)
        {
            ret.Add(a.ToString());
        }
		return ret;
	}

	public List<string> GetCurrentlyTickingAbilities()
	{
        return abilityQueue.GetCurrentlyTickingAbilities();
	}

	void Update()
	{
        if(!initialized)
        {
            return;
        }
        foreach (Ability a in abilities)
        {
            a.Cooldown(Time.deltaTime);
        }
        abilityQueue.Update(gameObject);
	}

    

	public Sprite GetIcon(int index)
	{
		if(index >= (int)AbilitySlots.MAX)
        {
            return null;
        }
        return abilities[index].icon;
	}

    public void AbilityInput(AbilitySlots index, InputAction.CallbackContext ctx, Vector2 point)
    {
        AbilityInput(abilities[index], ctx, point);
    }

    public void AbilityInput(int index, InputAction.CallbackContext ctx, Vector2 point)
    {
        AbilityInput(abilities[index], ctx, point);
    }


    /// <summary>
    /// Called from player input, attempts to perform the ability, may fail becdause it's not castable or other reasons. Will add to queue if another ability is in the pipe
    /// </summary>
    /// <param name="a">The ability to use</param>
    /// <param name="ctx">Input context</param>
    /// <param name="point">The mouse position</param>
    public void AbilityInput(Ability a, InputAction.CallbackContext ctx, Vector2 point)
    {
        if (!initialized)
        {
            return;
        }
        switch(ctx.phase) //on press
        {
            case InputActionPhase.Started: //on key press
            {
                abilityQueue.AbilityStarted(a);
            } break;
            case InputActionPhase.Performed: //not sure?? I think it has to deal with interactions
            {
            }
            break;
            case InputActionPhase.Canceled: //On key up
            {
                abilityQueue.AbilityRecieveInput(a, point);

            } break;
        }
    }

    public bool IsInitialized()
    {
        return initialized;
    }


    private bool inventoryOpen = true;
    
    public void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;
        if(inventoryGO == null)
        {
            inventoryGO = GameObject.FindWithTag("PlayerInventory");
        }
        if (inventoryOpen)
        {
            var cols = Physics2D.OverlapCircleAll(transform.position, 3, LayerMask.GetMask("GroundAbilities"));
            List<GameObject> groundAbilities = new List<GameObject>();
            foreach (var c in cols)
            {
                groundAbilities.Add(c.gameObject);
            }
            Lib.FindInHierarchy<UI_PlayerInventory>(inventoryGO).Setup(this, groundAbilities);
        }
        inventoryGO.SetActive(inventoryOpen);
    }
}