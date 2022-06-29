using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Targeting;

public delegate void CooldownTickDelegate(float currentCooldown, float maxCooldown);
public delegate void AbilityCastingDelegate(AbilityEventData data);

/// <summary>
/// Represents an ability in the game, that's usable by the player. They are instantiated when the player is spawned,
/// then initialized, and used repeatedly over the lifetime of the player object. The player gets a reference to a
/// saved asset AbilitySet that hold the abilities it will have, abilities are activated by PlayerAbilities.
/// </summary>
public abstract class Ability : ScriptableObject
{
    public event CooldownTickDelegate cooldownTick = delegate { };


    /// <summary>
    /// The ID that this ability uses to identify itself in the xml, multiple instances of the same variant of this ability will share an ID. 
    /// ID 0 is an invalid ID
    /// </summary>
    public uint ID = 0;

    /// <summary>
    /// The name of this ability 
    /// </summary>
    public string abilityName;

    /// <summary>
    /// The ability tooltip to be displayed when the ability is moused over in a menu
    /// </summary>
    public string tooltipDescription;

    /// <summary>
    /// Tag set that apply to what this ability is
    /// </summary>
    public GameplayTagContainer abilityTags;

    /// <summary>
    /// Tag set that prevent abilities that have these tags from being cast while the ability is active
    /// </summary>
    public GameplayTagContainer tagsToBlock;

    /// <summary>
    /// Tag set that is applied to the caster while the ability is active
    /// </summary>
    public GameplayTagContainer tagsToApply;

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
    /// The targeting information e.g. range type
    /// </summary>
    public AbilityTargetingData targetingData;

    /// <summary>
    /// Represents the current duration of the ticking ability while it is running.
    /// Resets to the max duration after the ability is finished
    /// </summary>
    public float currentDuration
    {
        get;
        protected set;
    }

    /// <summary>
    /// Rerpresents the amount of time remaining while the ablity is cooling down.
    /// </summary>
    public float currentCooldown
    {
        get;
        protected set;
    }

    protected PlayerAbilities playerAbilities;

    private int InstanceID;
    private List<uint> appliedTagIDs;

    /// <summary>
    /// Called once when the ability is intantiated, should be used to setup references that the ability
    /// will need over its lifetime e.g. a rigidbody reference
    /// </summary>
    public virtual void Initialize(PlayerAbilities pa)
    {
        playerAbilities = pa;
        appliedTagIDs = new List<uint>();
        Reinitialize();
        currentCooldown = 0;
        SetupIDNumber();
        AbilityDataXMLParser.instance.UpdateAbilityData(this);
    }


    /// <summary>
    /// Called when the ability has left the scope it exists in, should be used to cleanup any pools allocated
    /// </summary>
    /// <param name="pa">The player abilites it was included in</param>
    public virtual void Cleanup(PlayerAbilities pa)
    {

    }

    private static int counter = 0;
    private void SetupIDNumber()
    {
        InstanceID = counter++;
    }

    /// <summary>
    /// Called multiple times over the abilities lifetime, every time the ability is ended. Used to reset
    /// any state changes over the course of ability's use that should be reverted e.g. timers should be
    /// reset to 0. Also sets the cooldown time to its max.
    /// </summary>
    public virtual void Reinitialize()
    {
        currentDuration = maxDuration;
#if UNITY_EDITOR
        if(playerAbilities.DEBUG_LOW_COOLDOWN)
        {
            currentCooldown = Mathf.Min(0.1f, maxCooldown);
        }
#else
        currentCooldown = maxCooldown;
#endif
            appliedTagIDs.Clear();
    }

    /// <summary>
    /// Lowers the cooldown time of the ability, potentially making it castable
    /// </summary>
    /// <param name="deltaTime">The time since the last cooldown tick</param>
    public virtual void Cooldown(float deltaTime)
    {
        if (currentCooldown <= 0)
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
        bool castable = currentCooldown <= 0 && currentDuration == maxDuration;
        if(!castable)
        {
            return false;
        }
        return !playerAbilities.tagComponent.blockedTags.Matches(abilityTags);
    }


    /// <summary>
    /// Parental default just checks if the cost is payable, children can have more requirements
    /// </summary>
    /// <param name="targetPoint">The point at which the ability was targeted</param>
    /// <returns>Returns true if the ability is used succesfully</returns>
    public virtual bool AttemptUseAbility()
    {
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ABILITY, string.Format("{0} ATTEMPT USE AT {1}", name, targetingData.inputPoint));
        // if the ability is already ticking or on cooldown
        // should also check cost
        if (!IsCastable())
        {
            return false;
        }
        UseAbility();
        return true;
    }

    /// <summary>
    /// Actually uses the ability if AttemptUseAbility completes
    /// </summary>
    protected virtual void UseAbility()
    {
        foreach(var tag in tagsToApply.GetGameplayTags())
        {
            appliedTagIDs.Add(playerAbilities.tagComponent.tags.AddTag(tag.Flag));
        }
        foreach (var tag in tagsToBlock.GetGameplayTags())
        {
            appliedTagIDs.Add(playerAbilities.tagComponent.blockedTags.AddTag(tag.Flag));
        }
    }

    /// <summary>
    /// Gets called if the ability has an ongoing effect, over multiple frames
    /// </summary>
    /// <returns>
    /// Returns true if the ability wants to end (removed from the ticking list)
    /// </returns>
    public virtual bool Tick(float deltaTime)
    {
        if(isPassive)
        {
            return false;
        }
        currentDuration -= deltaTime;
        if (currentDuration <= 0)
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
        foreach(uint i in appliedTagIDs)
        {
            playerAbilities.tagComponent.blockedTags.RemoveTagWithID(i);
            playerAbilities.tagComponent.tags.RemoveTagWithID(i);
        }
        Reinitialize();
    }

    public float GetCooldownPercent()
    {
        return currentCooldown / maxCooldown;
    }

    /// <summary>
    /// Helper for the targetting data to be a non-monobehaviour but still instantiate
    /// </summary>
    /// <param name="g">Prefab to instance</param>
    /// <param name="create">To instantiate or destroy</param>
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

    /// <summary>
    /// Helper method for children to convert a mouse position to a direction
    /// </summary>
    /// <param name="point">The input mouse position</param>
    /// <returns>Vector2 represetning the direction towards the mouse from the player</returns>
    public Vector2 GetDirectionTowardsTarget(Vector2 point)
    {
        return point - (Vector2)playerAbilities.transform.position;
    }

    /// <summary>
    /// Helper method for children to convert a mouse position to a normalized direction
    /// </summary>
    /// <param name="point">The input mouse position</param>
    /// <returns>Normalized Vector2 represetning the direction towards the mouse from the player</returns>
    public Vector2 GetNormalizedDirectionTowardsTarget(Vector2 point)
    {
        return GetDirectionTowardsTarget(point).normalized;
    }

    /// <summary>
    /// Helper method to clamp a point within the circle range of an ability
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public Vector2 ClampPointWithinRange(Vector2 point, float range)
    {
        Vector2 direction = point - (Vector2)playerAbilities.transform.position;
        float magnitude = direction.magnitude;
        if(magnitude > range)
        {
            return direction.normalized * targetingData.range + (Vector2)playerAbilities.transform.position;
        }
        return point;
    }

    public Vector2 ClampPointWithinRange(Vector2 point)
    {
        return ClampPointWithinRange(point, targetingData.range);
    }

    /// <summary>
    /// Helper method to find an implementer of ITargetable centered on a point, returns the object of highest priority obeying the afilliation flag
    /// </summary>
    /// <param name="point">Point to raycast to</param>
    /// <param name="affiliation">The afilliation mask to search for</param>
    /// <returns></returns>
    static public ITargetable FindTargetable(Vector2 point, Affiliation affiliation)
    {
        var raycastResults = Physics2D.OverlapPointAll(point, LayerMask.GetMask("Targeting"));
        ITargetable target = null;
        foreach (var i in raycastResults)
        {
            ITargetable potential = i.gameObject.GetComponentInParent<ITargetable>();
            if (potential != null && potential.TargetAffiliation.HasFlag(affiliation))
            {
                if (target == null || target.Priority < potential.Priority)
                {
                    target = potential;
                }
            }
        }
        return target;
    }

    /// <summary>
    /// Returns a tooltip with all of the values plugged in, child classes are expected to override this with a string.format of their own variables setup
    /// </summary>
    /// <returns>the tooltip </returns>
    public abstract string GetTooltip();

    public new virtual string ToString()
    {
        if (hasDuration)
        {
            return string.Format("{0} {1}/{2}", this.GetType().Name, currentDuration, maxDuration);
        }
        else
        {
            return this.GetType().Name;
        }
    }

    public static bool operator ==(Ability lhs, Ability rhs)
    {
        if(lhs is null || rhs is null)
        {
            return lhs is null && rhs is null;
        }
        return lhs.InstanceID == rhs.InstanceID;
    }

    public static bool operator !=(Ability lhs, Ability rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object other)
    {
        if(other is Ability)
        {
            return (Ability)other == this;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return InstanceID;
    }
}