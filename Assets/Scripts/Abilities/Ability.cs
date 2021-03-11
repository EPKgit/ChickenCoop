using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void CooldownTickDelegate(float currentCooldown, float maxCooldown);
public delegate void OnCastDeleage(Ability a);

/// <summary>
/// Represents an ability in the game, that's usable by the player. They are instantiated when the player is spawned,
/// then initialized, and used repeatedly over the lifetime of the player object. The player gets a reference to a
/// saved asset AbilitySet that hold the abilities it will have, abilities are activated by PlayerAbilities.
/// </summary>
public class Ability : ScriptableObject
{
	public event CooldownTickDelegate cooldownTick = delegate { };
	/// <summary>
	/// Set true if you want the ability to be passive, meant it is never used, the only methods to be called
	/// will be Initialize, Tick, and FinishAbility. They will be initialized on spawn, ticked until they
	/// return false, and finished when the player dies or is otherwise removed from play. Passive abilities
	/// are added to a list within the AbilitySet of the player.
	/// </summary>
	public bool isPassive;
	/// <summary>
	/// Set true if you want the ability's Tick function and FinishAbility functions to get called
	/// </summary>
	public bool tickingAbility;
	/// <summary>
	/// Set true if the ability has a duration, if so the parent class's tick can be used to remove the
	/// effect once it's done, otherwise it's up to the ability to return true in Tick when it's finished
	/// </summary>
	public bool hasDuration;

	/// <summary>
	/// The amount of time the ability will remain active if hasDuration is true
	/// </summary>
	public float maxDuration;

	/// <summary>
	/// Set true if you want the ability to only respond to the button press event
	/// </summary>
	public bool pressOnly = true;

	/// <summary>
	/// The amount of time to wait after casting the ability before you can cast it again.
	/// </summary>
	public float maxCooldown;

	/// <summary>
	/// The amount of resource to remove from the player when checking the cost
	/// </summary>
	public float cost;

	/// <summary>
	/// The icon of the sprite for the UI
	/// </summary>
	public Sprite icon;

	/// <summary>
	/// Represents the current duration of the ticking ability while it is running.
	/// Resets to the max duration after the ability is finished
	/// </summary>
	protected float currentDuration;

	/// <summary>
	/// Rerpresents the amount of time remaining while the ablity is cooling down.
	/// </summary>
	protected float currentCooldown;

	protected PlayerAbilities playerAbilities;

	/// <summary>
	/// Called once when the ability is intantiated, should be used to setup references that the ability
	/// will need over its lifetime e.g. a rigidbody reference
	/// </summary>
	public virtual void Initialize(PlayerAbilities pa)
	{
		playerAbilities = pa;
		Reinitialize();
		currentCooldown = 0;
	}

	/// <summary>
	/// Called multiple times over the abilities lifetime, every time the ability is ended. Used to reset
	/// any state changes over the course of ability's use that should be reverted e.g. timers should be
	/// reset to 0. Also sets the cooldown time to its max.
	/// </summary>
	public virtual void Reinitialize()
	{
		currentDuration = maxDuration;
		currentCooldown = maxCooldown;
	}

	/// <summary>
	/// Lowers the cooldown time of the ability, potentially making it castable
	/// </summary>
	/// <param name="deltaTime">The time since the last cooldown tick</param>
	public virtual void Cooldown(float deltaTime)
	{
		if(currentCooldown <= 0)
		{
			return;
		}
		currentCooldown -= deltaTime;
		cooldownTick(currentCooldown, maxCooldown);
	}

	/// <summary>
	/// Allows the ability to be queried if it is castable without trying to use the ability.
	/// </summary>
	/// <returns>Returns true if the ability could be cast, false otherwise</returns>
	public virtual bool IsCastable()
	{
		return currentCooldown <= 0 && currentDuration == maxDuration;
	}

	/// <summary>
	/// Parental default just checks if the cost is payable, children can have more requirements
	/// </summary>
	/// <returns>
	/// Returns true if the ability is used succesfully
	/// </returns>
    public virtual bool AttemptUseAbility(InputAction.CallbackContext ctx, Vector2 inputDirection)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ABILITY, string.Format("{0} ATTEMPT USE perf:{1} strt:{2} canc:{3}", name, ctx.performed, ctx.started, ctx.canceled));
		// if the ability only wants buttondown and it wasn't or if the ability is already ticking, don't use
		// should also check cost
		if( (pressOnly && !ctx.performed) || !IsCastable() )
		{
			return false;
		}
		UseAbility(ctx, inputDirection);
		return true;
	}

	/// <summary>
	/// Actually uses the ability if AttemptUseAbility completes
	/// </summary>
	protected virtual void UseAbility(InputAction.CallbackContext ctx, Vector2 inputDirection)
	{

	}

	/// <summary>
	/// Gets called if the ability has an ongoing effect, over multiple frames
	/// </summary>
	/// <returns>
	/// Returns true if the ability wants to end (removed from the ticking list)
	/// </returns>
	public virtual bool Tick(float deltaTime)
	{
		currentDuration -= deltaTime;
		if(currentDuration <= 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Gets called when the abilitys tick returns true and the ability is finished
	/// </summary>
	public virtual void FinishAbility()
	{
		Reinitialize();
	}

	public float GetCooldownPercent()
	{
		return currentCooldown / maxCooldown;
	}

	public new virtual string ToString()
	{
		if(hasDuration)
		{
			return string.Format("{0} {1}/{2}", this.GetType().Name, currentDuration, maxDuration);
		}
		else
		{
			return this.GetType().Name;
		}
	}
}