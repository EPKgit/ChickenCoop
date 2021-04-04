using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
    public delegate void AbilityInitializationDelegate(Ability a1, Ability a2, Ability a3, Ability attack);
    public event AbilityInitializationDelegate initializedEvent = delegate { };

    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public CircleCollider2D col;
    [HideInInspector]
    public StatBlock stats;
    [HideInInspector]
    public BaseHealth hp;

    private AbilitySet abilitySet;

    public Ability ability1 { get; private set; }
    public Ability ability2 { get; private set; }
    public Ability ability3 { get; private set; }
    public Ability attack { get; private set; }

    private List<Ability> passives = new List<Ability>();

    private AbilityQueue abilityQueue;

    private bool initialized = false;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		col = transform.Find("Colliders").GetComponent<CircleCollider2D>();
		stats = GetComponent<StatBlockComponent>().GetStatBlock();
		hp = GetComponent<BaseHealth>();
	}

	public void Initialize(PlayerClass pc)
	{
		Initialize(pc.abilities);
	}

	public void Initialize(AbilitySet abilitySet)
	{
        abilityQueue = new AbilityQueue();
        passives = new List<Ability>();
		abilitySet = Instantiate(abilitySet);
		attack = ScriptableObject.Instantiate(abilitySet.attack);
		ability1 = ScriptableObject.Instantiate(abilitySet.ability1);
		ability2 = ScriptableObject.Instantiate(abilitySet.ability2);
		ability3 = ScriptableObject.Instantiate(abilitySet.ability3);
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
		attack.Initialize(this);
		ability1.Initialize(this);
		ability2.Initialize(this);
		ability3.Initialize(this);
		foreach(Ability a in passives)
		{
			a.Initialize(this);
            abilityQueue.AbilityStarted(a);
        }
        initialized = true;
		initializedEvent(ability1, ability2, ability3, attack);
	}

    /// <summary>
    /// Register cooldown callbacks for all abilities, will be called immediately with the starting values. These values may be 0
    /// </summary>
    /// <param name="at">Attack callback</param>
    /// <param name="a1">Ability 1 callback</param>
    /// <param name="a2">Ability 2 callback</param>
    /// <param name="a3">Ability 3 callback</param>
	public void RegisterAbilityCooldownCallbacks(CooldownTickDelegate at, CooldownTickDelegate a1, CooldownTickDelegate a2, CooldownTickDelegate a3)
	{
		attack.cooldownTick += at;
		ability1.cooldownTick += a1;
		ability2.cooldownTick += a2;
		ability3.cooldownTick += a3;
        at(attack.currentCooldown, attack.maxCooldown);
        a1(ability1.currentCooldown, ability1.maxCooldown);
        a2(ability2.currentCooldown, ability2.maxCooldown);
        a3(ability3.currentCooldown, ability3.maxCooldown);
	}

	public List<string> GetCurrentlyInstantiatedAbilities()
	{
        if(!initialized)
        {
            return new List<string>();
        }
		List<string> ret = new List<string>();
		ret.Add(attack.ToString());
		ret.Add(ability1.ToString());
		ret.Add(ability2.ToString());
		ret.Add(ability3.ToString());
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
		ability1.Cooldown(Time.deltaTime);
		ability2.Cooldown(Time.deltaTime);
		ability3.Cooldown(Time.deltaTime);
		attack.Cooldown(Time.deltaTime);

        abilityQueue.Update(gameObject);
	}

    

	public Sprite GetIcon(int index)
	{
		switch(index)
		{
			case 0:
				return attack.icon;
			case 1:
				return ability1.icon;
			case 2:
				return ability2.icon;
			case 3:
				return ability3.icon;
		}
		return null;
	}

	public void Attack(InputAction.CallbackContext ctx, Vector2 dir)
	{
        AbilityInput(attack, ctx, dir);
	}

	public void Ability1(InputAction.CallbackContext ctx, Vector2 dir)
	{
        AbilityInput(ability1, ctx, dir);
	}

	public void Ability2(InputAction.CallbackContext ctx, Vector2 dir)
	{
        AbilityInput(ability2, ctx, dir);
	}

	public void Ability3(InputAction.CallbackContext ctx, Vector2 dir)
	{
        AbilityInput(ability3, ctx, dir);
	}

    public void AbilityInput(Ability a, InputAction.CallbackContext ctx, Vector2 point)
    {
        if (!initialized)
        {
            return;
        }
        switch(ctx.phase) //on press
        {
            case InputActionPhase.Started: //on press
            {
                abilityQueue.AbilityStarted(a);
            } break;
            case InputActionPhase.Performed: //on finish
            {
            }
            break;
            case InputActionPhase.Canceled: //cleanup
            {
                abilityQueue.AbilityRecieveInput(a, point);

                //Debug.LogError("Input Cancelled");
                //throw new System.NotImplementedException();
            } break;
        }
    }

    public bool IsInitialized()
    {
        return initialized;
    }
}