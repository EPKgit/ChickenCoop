using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
	public delegate void AbilityInitializationDelegate(Ability a1, Ability a2, Ability a3, Ability attack);
	public event AbilityInitializationDelegate initializedEvent = delegate { };

	public PlayerClass playerClass;

	[HideInInspector]
	public Rigidbody2D rb;
	[HideInInspector]
	public CircleCollider2D col;
	[HideInInspector]
	public StatBlock stats;
	[HideInInspector]
	public BaseHealth hp;

	private AbilitySet abilitySet;

	private Ability ability1;
	private Ability ability2;
	private Ability ability3;
	private Ability attack;

	private List<Ability> passives = new List<Ability>();
    private List<Ability> currentlyTicking = new List<Ability>();

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		col = transform.Find("Colliders").GetComponent<CircleCollider2D>();
		stats = GetComponent<StatBlock>();
		hp = GetComponent<BaseHealth>();
	}

	void OnEnable()
	{
		if(abilitySet)
		{
			Initialize(abilitySet);
		}
		if(playerClass)
		{
			Initialize(playerClass);
		}
	}

	void OnDisable()
	{
		foreach(Ability a in currentlyTicking)
		{
			a.FinishAbility();
		}
	}

	public void Initialize(PlayerClass pc)
	{
		playerClass = pc;
		Initialize(pc.abilities);
	}

	public void Initialize(AbilitySet _as)
	{
		currentlyTicking = new List<Ability>();
        passives = new List<Ability>();
		abilitySet = _as;
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
				Debug.Log("Non-passive ability added to passive ability list of " + abilitySet.name);
			}
			
		}
		attack.Initialize(this);
		ability1.Initialize(this);
		ability2.Initialize(this);
		ability3.Initialize(this);
		foreach(Ability a in passives)
		{
			a.Initialize(this);
			currentlyTicking.Add(a);
		}
		initializedEvent(ability1, ability2, ability3, attack);
	}

	public void RegisterAbilityCooldownCallbacks(CooldownTickDelegate at, CooldownTickDelegate a1, CooldownTickDelegate a2, CooldownTickDelegate a3)
	{
		attack.cooldownTick += at;
		ability1.cooldownTick += a1;
		ability2.cooldownTick += a2;
		ability3.cooldownTick += a3;
	}

	public List<string> GetCurrentlyInstantiatedAbilities()
	{
		List<string> ret = new List<string>();
		ret.Add(attack.ToString());
		ret.Add(ability1.ToString());
		ret.Add(ability2.ToString());
		ret.Add(ability3.ToString());
		return ret;
	}

	public List<string> GetCurrentlyTickingAbilities()
	{
		if(currentlyTicking == null)
		{
			return null;
		}
		List<string> ret = new List<string>();
		foreach(Ability a in currentlyTicking)
		{
			ret.Add(a.ToString());
		}
		return ret;
	}

	void Update()
	{
		for(int x = currentlyTicking.Count - 1; x >= 0; --x)
		{
			if(currentlyTicking[x].Tick(Time.deltaTime))
			{
				currentlyTicking[x].FinishAbility();
				currentlyTicking.RemoveAt(x);
			}
		}
		ability1.Cooldown(Time.deltaTime);
		ability2.Cooldown(Time.deltaTime);
		ability3.Cooldown(Time.deltaTime);
		attack.Cooldown(Time.deltaTime);
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
		UseAbility(attack, ctx, dir);
	}

	public void Ability1(InputAction.CallbackContext ctx, Vector2 dir)
	{
		UseAbility(ability1, ctx, dir);
	}

	public void Ability2(InputAction.CallbackContext ctx, Vector2 dir)
	{
		UseAbility(ability2, ctx, dir);
	}

	public void Ability3(InputAction.CallbackContext ctx, Vector2 dir)
	{
		UseAbility(ability3, ctx, dir);
	}

	public void UseAbility(Ability a, InputAction.CallbackContext ctx, Vector2 dir)
	{
		if(a.AttemptUseAbility(ctx, dir))
		{
			if(a.tickingAbility)
			{
				currentlyTicking.Add(a);
			}
			else
			{
				a.FinishAbility();
			}
		}
	}
}