using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Runtime.Remoting.Lifetime;
using GameplayTagInternals;

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

    public event AbilityCastingDelegate OnAbilityCasting = delegate { };
    public event AbilityCastingDelegate OnAbilityCastTick = delegate { };
    public event AbilityCastingDelegate OnAbilityCasted = delegate { };
    public event AbilityCastingDelegate OnAbilityActivating = delegate { };
    public event AbilityCastingDelegate OnAbilityActivated = delegate { };

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

    public AbilitySetContainer Abilities { get => _abilities; }
    private AbilitySetContainer _abilities;

    public IList<Ability> Passives { get => _passives.AsReadOnly(); }
    private List<Ability> _passives = new List<Ability>();

    private AbilityQueue abilityQueue;

    private bool initialized = false;
    private GameplayTagID castingTag;

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
        OnAbilityCasting(aed);
        castingTag = tagComponent.tags.AddTag(GameplayTagFlags.ABILITY_CASTING);
    }

    private void AbilityCastTickEvent(AbilityEventData aed)
    {
        OnAbilityCastTick(aed);
    }

    private void PostCastEvent(AbilityEventData aed)
    {
        OnAbilityCasted(aed);
        tagComponent.tags.RemoveFirstTagWithID(castingTag);
        castingTag = null;
    }
    
    private void PreActivateEvent(AbilityEventData aed)
    {
        OnAbilityActivating(aed);
    }

    private void PostActivateEvent(AbilityEventData aed)
    {
        OnAbilityActivated(aed);
    }

    public void Initialize(AbilitySetAsset abilitySet)
	{
        abilityQueue = new AbilityQueue(this);
        abilityQueue.OnAbilityCasting += PreCastEvent;
        abilityQueue.OnAbilityCasted += PostCastEvent;
        abilityQueue.OnAbilityCastTick += AbilityCastTickEvent;
        abilityQueue.OnAbilityActivating += PreActivateEvent;
        abilityQueue.OnAbilityActivated += PostActivateEvent;
        _passives = new List<Ability>();

        Ability[] abilities = new Ability[(int)AbilitySlot.MAX];
        for (int x = 0; x < abilitySet.abilityIDs.Length; ++x)
        {
            // 0 is an invalid ID for ability IDs
            if (abilitySet.abilityIDs[x] == 0)
            {
#if UNITY_EDITOR_WIN
                Debug.LogError($"ERROR: ability set {abilitySet.name} has an invalid ID");
                return;
#else
                continue;
#endif
            }
            abilities[x] = AbilityLookup.CreateAbilityFromId(abilitySet.abilityIDs[x]);
        }
        _abilities = new AbilitySetContainer(abilities);
        for (int x = 0; x < abilitySet.passiveEffectIDs.Count; ++x)
        {
            if (abilitySet.passiveEffectIDs[x] != 0)
            {
                continue;
            }
            //_passives.Add(AbilityLookup.CreateAbilityFromId(abilitySet.abilityIDs[x]));
        }

        foreach (Ability a in _abilities)
        {
            a.Initialize(this);
        }
        foreach (Ability a in _passives)
        {
			a.Initialize(this);
            if(!a.isPassive)
            {
                Debug.LogError("Non-passive ability added to passive ability list of " + abilitySet.name);
            }
            abilityQueue.AbilityStarted(a);
        }
        initialized = true;
		initializedEvent(_abilities);
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
            if(callbacks[x] == null || Abilities[x] == null)
            {
                callbacks[x](new CooldownTickData(-1, -1, -1, -1));
                continue;
            }
            Abilities[x].cooldownTick += callbacks[x];
            callbacks[x](new CooldownTickData(Abilities[x].currentCooldownTimer, Abilities[x].maxCooldown, Abilities[x].currentRecastTimer, Abilities[x].recastWindow));
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
            if(Abilities[x] == null)
            {
                continue;
            }
            Abilities[x].cooldownTick -= callbacks[x];
        }
    }

    public List<string> GetCurrentlyInstantiatedAbilities()
	{
        if(!initialized)
        {
            return new List<string>();
        }
		List<string> ret = new List<string>();
        foreach(Ability a in Abilities)
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
        foreach (Ability a in Abilities)
        {
            a.Cooldown(Time.deltaTime);
        }
        abilityQueue.Update(gameObject);
	}

    

	public Sprite GetIcon(int index)
	{
		if(index >= (int)AbilitySlot.MAX || Abilities[index] == null)
        {
            return null;
        }
        return Abilities[index].icon;
	}

    public void AbilityInput(AbilitySlot index, InputAction.CallbackContext ctx, Vector2 point)
    {
        AbilityInput(Abilities[index], ctx, point);
    }

    public void AbilityInput(int index, InputAction.CallbackContext ctx, Vector2 point)
    {
        AbilityInput(Abilities[index], ctx, point);
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

    /// <summary>
    /// Called by external sources to notify that an ability wants to be ended before it normally would
    /// </summary>
    /// <param name="a">the ability to end</param>
    public void AbilityEndedExternal(Ability a)
    {
        abilityQueue.AbilityEndedExternal(a);
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
        if(!index.ValidSlot() || Abilities[index] == null)
        {
            return false;
        }
        abilityChanged.Invoke(_abilities[index], null, index, AbilityChangeType.REMOVED);
        _abilities[index] = null;
        initializedEvent(Abilities);
        return true;
    }

    /// <summary>
    /// Remove an ability from the set properly updating
    /// </summary>
    /// <param name="a">The abilitiy to remove</param>
    /// <returns>True if the ability was found and removed, false if it wasn't found</returns>
    public bool RemoveAbility(Ability a)
    {
        for(int x = 0; x < Abilities.Length; ++x)
        {
            if(Abilities[x] == a)
            {
                abilityChanged.Invoke(_abilities[x], null, (AbilitySlot)x, AbilityChangeType.REMOVED);
                _abilities[x] = null;
                initializedEvent(Abilities);
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
        if(!index.ValidSlot() || Abilities[index] != null)
        {
            return false;
        }
        abilityChanged.Invoke(_abilities[index], null, index, AbilityChangeType.ADDED);
        _abilities[index] = a;
        initializedEvent(Abilities);
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
        initializedEvent(Abilities);
        return true;
    }


    /// <summary>
    /// Create or destroy gameobjects. Necessary because abilities aren't a monobheaviour or scriptable object so they don't have access
    /// to these methods
    /// </summary>
    /// <param name="g">The prefab to create or gameobject to destroy</param>
    /// <param name="create">Whether to create or destroy</param>
    /// <returns></returns>
    public GameObject GameObjectManipulation(GameObject g, bool create)
    {
        if (create)
        {
            return Instantiate(g);
        }
        else
        {
            Destroy(g);
            return null;
        }
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
            Lib.FindInHierarchy<UI_PlayerInventory>(inventoryGO).Setup(this);
        }
        inventoryGO.SetActive(inventoryOpen);
    }

    public bool InventoryOpen()
    {
        return inventoryOpen;
    }
}