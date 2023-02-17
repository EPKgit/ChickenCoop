using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
    public event Action<AbilitySetContainer> initializedEvent = delegate { };

    public enum AbilityChangeType
    {
        ADDED,
        REMOVED,
        OVERRIDDEN,

    }
    public delegate void AbilityChangedDelegate(Ability previousAbility, Ability newAbility, AbilitySlot slot, AbilityChangeType type);
    public event AbilityChangedDelegate abilityChanged = delegate { };

    public event AbilityCastingDelegate preAbilityCastEvent = delegate { };
    public event AbilityCastingDelegate postAbilityCastEvent = delegate { };

#if UNITY_EDITOR
    public bool DEBUG_LOW_COOLDOWN = false;
#endif
    public GameObject inventoryUIPrefab;

    public float pickupRadius = 3.0f;

    private GameObject inventoryGO;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public CircleCollider2D col;
    [HideInInspector] public StatBlock stats;
    [HideInInspector] public PlayerHealth hp;
    [HideInInspector] public GameplayTagComponent tagComponent;
    [HideInInspector] public PlayerMovement movement;
    [HideInInspector] public PlayerCollision collision;

    private AbilitySetAsset abilitySet;

    public AbilitySetContainer abilities { get { return _abilities; } }

    private AbilitySetContainer _abilities;

    private List<Ability> passives = new List<Ability>();

    private AbilityQueue abilityQueue;

    private bool initialized = false;

	void Awake()
	{
        rb = GetComponent<Rigidbody2D>();
		col = transform.Find("DefaultCollider").GetComponent<CircleCollider2D>();
		stats = GetComponent<StatBlockComponent>().GetStatBlock();
		hp = GetComponent<PlayerHealth>();
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


    private void PreCastEvent(AbilityEventData aed)
    {
        preAbilityCastEvent(aed);
    }

    private void PostCastEvent(AbilityEventData aed)
    {
        postAbilityCastEvent(aed);
    }

    public void Initialize(AbilitySetAsset abilitySet)
	{
        abilityQueue = new AbilityQueue(this);
        abilityQueue.preAbilityCastEvent += PreCastEvent;
        abilityQueue.postAbilityCastEvent += PostCastEvent;
        passives = new List<Ability>();

        abilitySet = Instantiate(abilitySet);
        for(int x = 0; x < abilitySet.abilities.Length; ++x)
        {
            if (abilitySet.abilities[x] != null)
            {
                abilitySet.abilities[x] = ScriptableObject.Instantiate(abilitySet.abilities[x]);
            }
        }
        _abilities = new AbilitySetContainer(abilitySet.abilities);
		foreach(Ability a in abilitySet.passiveEffects)
		{
            if(a == null)
            {
                continue;
            }
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
        for (int x = 0; x < _abilities.Length; ++x)
        {
            abilityChanged.Invoke(null, _abilities[x], (AbilitySlot)x, AbilityChangeType.ADDED);
        }

    }

    /// <summary>
    /// Register cooldown callbacks for all abilities, will be called immediately with the starting values. These values may be 0
    /// </summary>
    /// <param name="callbacks">Array of callbacks. Length should be equals to or less than the number of slots</param>
	public void RegisterAbilityCooldownCallbacks(CooldownTickDelegate[] callbacks)
	{
        if(callbacks.Length > (int)AbilitySlot.MAX)
        {
            throw new Exception("ERROR: Cooldown callback size is greater than the number of slots");
        }
        for(int x = 0; x < callbacks.Length; ++x)
        {
            if(callbacks[x] == null || abilities[x] == null)
            {
                callbacks[x](new CooldownTickData(-1, -1, -1, -1));
                continue;
            }
            abilities[x].cooldownTick += callbacks[x];
            callbacks[x](new CooldownTickData(abilities[x].currentCooldownTimer, abilities[x].maxCooldown, abilities[x].currentRecastTimer, abilities[x].recastWindow));
        }
	}

    /// <summary>
    /// Unregister cooldown callbacks for all abilities, will be called immediately with the starting values. These values may be 0
    /// </summary>
    /// <param name="callbacks">Array of callbacks. Length should be equals to or less than the number of slots</param>
	public void UnregisterAbilityCooldownCallbacks(CooldownTickDelegate[] callbacks)
    {
        if (callbacks.Length > (int)AbilitySlot.MAX)
        {
            throw new Exception("ERROR: Cooldown callback size is greater than the number of slots");
        }
        for (int x = 0; x < callbacks.Length; ++x)
        {
            if(abilities[x] == null)
            {
                continue;
            }
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
		if(index >= (int)AbilitySlot.MAX || abilities[index] == null)
        {
            return null;
        }
        return abilities[index].icon;
	}

    public void AbilityInput(AbilitySlot index, InputAction.CallbackContext ctx, Vector2 point)
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
        if (!initialized || a == null)
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

    /// <summary>
    /// Remove an ability from the set properly updating
    /// </summary>
    /// <param name="index">The slot</param>
    /// <returns>True if the slot wasn't empty and an ability was removed, and false otherwise</returns>
    public bool RemoveAbility(AbilitySlot index)
    {
        if(!index.ValidSlot() || abilities[index] == null)
        {
            return false;
        }
        abilityChanged.Invoke(_abilities[index], null, index, AbilityChangeType.REMOVED);
        _abilities[index] = null;
        initializedEvent(abilities);
        return true;
    }

    /// <summary>
    /// Remove an ability from the set properly updating
    /// </summary>
    /// <param name="a">The abilitiy to remove</param>
    /// <returns>True if the ability was found and removed, false if it wasn't found</returns>
    public bool RemoveAbility(Ability a)
    {
        for(int x = 0; x < abilities.Length; ++x)
        {
            if(abilities[x] == a)
            {
                abilityChanged.Invoke(_abilities[x], null, (AbilitySlot)x, AbilityChangeType.REMOVED);
                _abilities[x] = null;
                initializedEvent(abilities);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Add an ability to the set, preserving the ability already there if 
    /// </summary>
    /// <param name="index">Which slot to try to add to</param>
    /// <param name="a">The abiltiy to be added</param>
    /// <returns></returns>
    public bool AddAbility(AbilitySlot index, Ability a)
    {
        if(!index.ValidSlot() || abilities[index] != null)
        {
            return false;
        }
        abilityChanged.Invoke(_abilities[index], null, index, AbilityChangeType.ADDED);
        _abilities[index] = a;
        initializedEvent(abilities);
        return true;
    }

    /// <summary>
    /// Overrides a slot regardless of what it already in the slot
    /// </summary>
    /// <param name="index">Which slot to try to override</param>
    /// <param name="a">The abiltiy to be added</param>
    /// <returns>False if the slot is invalid, true otherwise</returns>
    public bool OverrideAbility(AbilitySlot index, Ability a)
    {
        if(!index.ValidSlot())
        {
            return false;
        }
        abilityChanged.Invoke(_abilities[index], a, index, AbilityChangeType.OVERRIDDEN);
        _abilities[index] = a;
        initializedEvent(abilities);
        return true;
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
            //TODO: clean  this shit up (individual UIs per player?)
            Lib.FindInHierarchy<UI_PlayerInventory>(inventoryGO).Setup(this, pickupRadius);
        }
        inventoryGO.SetActive(inventoryOpen);
    }

    public bool InventoryOpen()
    {
        return inventoryOpen;
    }
}