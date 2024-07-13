using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using Targeting;

public class MutableCooldownTickData
{
    public Stat tickDelta;

    public MutableCooldownTickData(float startingValue)
    {
        tickDelta = new Stat(StatName.MAX, startingValue);
    }

    public MutableCooldownTickData() : this(Time.deltaTime) { }
}

public class CooldownTickData
{
    public float currentCooldown;
    public float maxCooldown;
    public float currentRecast;
    public float maxRecast;
    public CooldownTickData(float currentCooldown, float maxCooldown, float currentRecast, float maxRecast)
    {
        this.currentCooldown = currentCooldown;
        this.maxCooldown = maxCooldown;
        this.currentRecast = currentRecast;
        this.maxRecast = maxRecast;
    }
}
public delegate void MutableCooldownTickDelegate(MutableCooldownTickData data);
public delegate void CooldownTickDelegate(CooldownTickData data);
public delegate void AbilityCastingDelegate(AbilityEventData data);

/// <summary>
/// Represents an ability in the game, that's usable by the player. They are instantiated when the player is spawned,
/// then initialized, and used repeatedly over the lifetime of the player object. The player gets a reference to a
/// saved asset AbilitySet that hold the abilities it will have, abilities are activated by PlayerAbilities.
/// </summary>
public abstract class Ability
{
    public event MutableCooldownTickDelegate preCooldownTick = delegate { };
    public event CooldownTickDelegate cooldownTick = delegate { };

    public enum AbilityUpgradeSlot
    {
        RED = 0,
        BLUE = 1,
        YELLOW = 2,
        MAX = 3,
        DEFAULT = 3,
    }


    /// <summary>
    /// The ID that this ability uses to identify itself in the xml, multiple instances of the same variant of this ability will share an ID. 
    /// ID 0 is an invalid ID
    /// </summary>
    public abstract uint ID
    {
        get;
    }


    #region ABILITY_ASSET_DATA
    /// <summary>
    /// The name of this ability 
    /// </summary>
    public string abilityName;

    /// <summary>
    /// The default asset library to try and pull non-scoped (.) assets from for this ability
    /// </summary>
    public string defaultAssetLibraryName;

    /// <summary>
    /// The ability tooltip to be displayed when the ability is displayed in a menu
    /// </summary>
    private string[] tooltipDescriptions = null;

    /// <summary>
    /// Tag set that apply to what this ability is
    /// </summary>
    public GameplayTagContainer abilityTags = new GameplayTagContainer();

    /// <summary>
    /// Tag set that prevent abilities that have these tags from being cast while the ability is active
    /// </summary>
    public GameplayTagContainer tagsToBlock = new GameplayTagContainer();

    /// <summary>
    /// Tag set that is applied to the caster while the ability is active
    /// </summary>
    public GameplayTagContainer tagsToApply = new GameplayTagContainer();

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
    public bool IsTickingAbility 
    { 
        get 
        {
            return isTickingAbilityOverride || maxDuration > 0;
        } 
    }

    /// <summary>
    /// Can be set to cause an ability to tick even if it doesn't have a duration set
    /// </summary>
    public bool isTickingAbilityOverride = false;

    /// <summary>
    /// The amount of time the ability will remain active
    /// </summary>
    public float maxDuration = -1;

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
    public RuntimeAbilityTargetingData targetingData
    {
        get
        {
            return _targetingData[currentTargetingType];
        }
    }

    private RuntimeAbilityTargetingData[] _targetingData;

    /// <summary>
    /// Helper property to display ranges when needed
    /// </summary>
    public float Range
    {
        get
        {
            return targetingData.Range;
        }
    }

    /// <summary>
    /// The index of what type of targeting we want to use on our ability. We can have a wide variety of targeting over a single
    /// abilities lifetime, we just need to switch it before previews start
    /// </summary>
    public int currentTargetingType = 0;

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
    public float currentCooldownTimer
    {
        get;
        protected set;
    }


    /// <summary>
    /// The number of times the ability is able to be recasted
    /// </summary>
    public int numberTimesRecastable = 0;

    public int currentRecastAmount
    {
        get;
        protected set;
    }

    /// <summary>
    /// The amount of time after casting that the ability can be recasted in
    /// </summary>
    public float recastWindow = 0;

    /// <summary>
    /// The amount of time we have remaining in the recast window
    /// </summary>
    public float currentRecastTimer
    {
        get;
        protected set;
    }

    /// <summary>
    /// The size of the ability aoe
    /// </summary>
    public float aoe = -1;

#endregion

    protected PlayerAbilities playerAbilities;
    protected bool[] upgradeStatus;

    private int InstanceID;
    private List<GameplayTagInternals.GameplayTagID> appliedTagIDs;

    /// <summary>
    /// Called once when the ability is intantiated, should be used to setup references that the ability
    /// will need over its lifetime e.g. a rigidbody reference
    /// </summary>
    public virtual void Initialize(PlayerAbilities pa)
    {
        playerAbilities = pa;
        appliedTagIDs = new List<GameplayTagInternals.GameplayTagID>();
        upgradeStatus = new bool[(int)AbilityUpgradeSlot.MAX];
        Reinitialize();
        currentCooldownTimer    = 0;
        currentRecastTimer      = 0;
        currentRecastAmount     = 0;
        SetupIDNumber();
        AbilityDataXMLParser.instance.UpdateAbilityData(this);
    }

    public virtual void OnAbilityDataUpdated()
    {

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
        appliedTagIDs.Clear();
    }

    /// <summary>
    /// Upgrades the ability on the slot passed in, will fail if it's already upgraded on that slot
    /// </summary>
    /// <param name="slot">The slot of the upgrade to give the ability</param>
    /// <returns>True if the ability was upgraded, false if it already was</returns>
    public virtual bool UpgradeAbility(AbilityUpgradeSlot slot)
    {
        if (upgradeStatus[(int)slot])
        {
            Debug.LogError("Error: ability attempted to be upgraded in the same fashion twice");
            return false;
        }
        upgradeStatus[(int)slot] = true;
        OnUpgrade(slot);
        return true;
    }

    public virtual void OnUpgrade(AbilityUpgradeSlot slot)
    {

    }

    /// <summary>
    /// Removes the upgrade of the ability on the slot passed in, will fail if it wasn't upgraded on that slot
    /// </summary>
    /// <param name="slot">The slot of the upgrade to remove</param>
    /// <returns>True if the ability was downgraded, false if it already was</returns>
    public virtual bool DownGradeAbility(AbilityUpgradeSlot slot)
    {
        if (!upgradeStatus[(int)slot])
        {
            Debug.LogError("Error: ability attempted to be downgraded in the same fashion twice");
            return false;
        }
        upgradeStatus[(int)slot] = false;
        return true;
    }

    public virtual void OnDowngrade(AbilityUpgradeSlot slot)
    {

    }

    /// <summary>
    /// Swaps the ability upgrade status on the upgrade slot passed in
    /// </summary>
    /// <param name="slot">The upgrade slot to swap</param>
    public virtual void ToggleAbilityUpgrade(AbilityUpgradeSlot slot)
    {
        if (upgradeStatus[(int)slot])
        {
            OnDowngrade(slot);
            upgradeStatus[(int)slot] = false;
        } 
        else
        {
            OnUpgrade(slot);
            upgradeStatus[(int)slot] = true;
        }
    }

    /// <summary>
    /// Get the upgrade status of the abilty on a certain slot
    /// </summary>
    /// <param name="slot">The slot to check</param>
    /// <returns>True if the ability is upgraded on that slot, false otherwise</returns>
    public bool GetAbilityUpgradeStatus(AbilityUpgradeSlot slot)
    {
        return upgradeStatus[(int)slot];
    }

    /// <summary>
    /// Get if the ability is red upgraded
    /// </summary>
    /// <returns>True if the ability is red upgraded, false otherwise</returns>
    public bool RedUpgraded()
    {
        return upgradeStatus[(int)AbilityUpgradeSlot.RED];
    }

    /// <summary>
    /// Get if the ability is yellow upgraded
    /// </summary>
    /// <returns>True if the ability is yellow upgraded, false otherwise</returns>
    public bool YellowUpgraded()
    {
        return upgradeStatus[(int)AbilityUpgradeSlot.YELLOW];
    }

    /// <summary>
    /// Get if the ability is blue upgraded
    /// </summary>
    /// <returns>True if the ability is blue upgraded, false otherwise</returns>
    public bool BlueUpgraded()
    {
        return upgradeStatus[(int)AbilityUpgradeSlot.BLUE];
    }

    /// <summary>
    /// Lowers the cooldown time of the ability, potentially making it castable
    /// </summary>
    /// <param name="deltaTime">The time since the last cooldown tick</param>
    public virtual void Cooldown(float deltaTime)
    {
        bool ticked = false;
        if (currentCooldownTimer > 0)
        {
            ticked = true;
            MutableCooldownTickData mpctd = new MutableCooldownTickData(deltaTime);
            preCooldownTick(mpctd);
            currentCooldownTimer -= mpctd.tickDelta.Value;
            if(currentCooldownTimer <= 0)
            {
                currentCooldownTimer = 0;
            }
        }
        if(currentRecastTimer > 0)
        {
            ticked = true;
            currentRecastTimer -= deltaTime;
            if(currentRecastTimer <= 0)
            {
                currentRecastTimer = 0;
                currentRecastAmount = 0;
                GoOnCooldown();
            }
        }
        if (ticked)
        {
            cooldownTick(new CooldownTickData(currentCooldownTimer, maxCooldown, currentRecastTimer, recastWindow));
        }
    }

    /// <summary>
    /// Allows the ability to be queried if it is castable without trying to use the ability.
    /// </summary>
    /// <returns>Returns true if the ability could be cast, false otherwise</returns>
    public virtual bool IsCastable()
    {
        bool castable = IsDurationFinished() && (IsOffCooldown() || IsRecastable());
        if(!castable)
        {
            return false;
        }
        return !playerAbilities.tagComponent.blockedTags.ContainsAny(abilityTags);
    }

    public bool IsDurationFinished()
    {
        return currentDuration == maxDuration;
    }

    public bool IsOffCooldown()
    {
        return currentCooldownTimer <= 0;
    }

    public bool IsRecastable()
    {
        return numberTimesRecastable != 0 && (currentRecastAmount != 0 && currentRecastTimer > 0);
    }


    /// <summary>
    /// Parental default just checks if the cost is payable, children can have more requirements
    /// </summary>
    /// <param name="targetPoint">The point at which the ability was targeted</param>
    /// <returns>Returns true if the ability is used succesfully</returns>
    public virtual bool AttemptUseAbility()
    {
        DebugFlags.Log(DebugFlags.Flags.ABILITY, string.Format("{0} ATTEMPT USE AT {1}", abilityName, targetingData.inputPoint));
        // if the ability is already ticking or on cooldown
        // should also check cost
        if (!IsCastable())
        {
            return false;
        }
        if(currentRecastAmount != 0)
        {
            ReuseAbility(numberTimesRecastable - currentRecastAmount);
        }
        else
        {
            UseAbility();
        }
        return true;
    }

    /// <summary>
    /// Actually uses the ability if AttemptUseAbility completes
    /// </summary>
    protected virtual void UseAbility()
    {
        foreach(var tag in tagsToApply.Tags)
        {
            appliedTagIDs.Add(playerAbilities.tagComponent.tags.AddTag(tag.Flag));
        }
        foreach (var tag in tagsToBlock.Tags)
        {
            appliedTagIDs.Add(playerAbilities.tagComponent.blockedTags.AddTag(tag.Flag));
        }
    }

    /// <summary>
    /// Called on recast when AttemptUseAbility completes. Should be override if reusing the ability has a different effect
    /// that just normally casting the ability
    /// </summary>
    /// <param name="recastNumber">The number of times the ability has been recast, starting at 0 for the first recast</param>
    protected virtual void ReuseAbility(int recastNumber)
    {
        UseAbility();
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
        foreach(GameplayTagInternals.GameplayTagID i in appliedTagIDs)
        {
            playerAbilities.tagComponent.blockedTags.RemoveFirstTagWithID(i);
            playerAbilities.tagComponent.tags.RemoveFirstTagWithID(i);
        }
        Reinitialize();
        if (CheckRecastConditions())
        {
            return;
        }
        GoOnCooldown();
    }
    
    protected bool CheckRecastConditions()
    {
        if (numberTimesRecastable != 0)
        {
            if (currentRecastAmount == 0) //this is the initial cast
            {
                currentRecastAmount = numberTimesRecastable;
            }
            else if (currentRecastAmount - 1 <= 0) //this is final recast allowed
            {
                currentRecastAmount = 0;
                return false;
            }            
            else //this is one of (multiple) recasts
            {
                --currentRecastAmount;
            }
            currentRecastTimer = Mathf.Max(recastWindow, float.Epsilon);
            return true;
        }
        return false;
    }
    protected void GoOnCooldown()
    {
        currentRecastTimer = 0;
        currentRecastAmount = 0;
        float cdrPercent = playerAbilities.stats.GetValue(StatName.CooldownReduction);
        currentCooldownTimer = maxCooldown * cdrPercent;
#if UNITY_EDITOR
        if (playerAbilities.DEBUG_LOW_COOLDOWN)
        {
            currentCooldownTimer = Mathf.Min(0.1f, maxCooldown);
        }
#endif
    }

    public void SetupTargetingData(AbilityTargetingData[] newTargetingData)
    {
        _targetingData = new RuntimeAbilityTargetingData[newTargetingData.Length];
        
        int index = 0;
        foreach(AbilityTargetingData data in newTargetingData)
        {
            _targetingData[index++] = new RuntimeAbilityTargetingData(data);
        }
    }

    protected void SwitchTargetingType(int newIndex)
    {
        if(newIndex < 0 || newIndex >= _targetingData.Length)
        {
            throw new System.Exception("ERROR: ATTEMPTING TO SWITCH TO TARGETING DATA OUT OF BOUNDS" + newIndex);
        }
        currentTargetingType = newIndex;
    }

    /// <summary>
    /// Called by child class to move the target data index forward
    /// </summary>
    protected void IncrementTargetingType()
    {
        currentTargetingType = (++currentTargetingType + _targetingData.Length) % _targetingData.Length;
    }

    protected void DecrementTargetingType()
    {
        currentTargetingType = (--currentTargetingType + _targetingData.Length) % _targetingData.Length;
    }

    public void ResetDuration()
    {
        currentDuration = maxDuration;
    }

    public float GetCooldownPercent()
    {
        return currentCooldownTimer / maxCooldown;
    }

#region ABILITY_HELPERS
    /// <summary>
    /// Helper for the targetting data to be a non-monobehaviour but still instantiate
    /// </summary>
    /// <param name="g">Prefab to instance</param>
    /// <param name="create">To instantiate or destroy</param>
    /// <returns></returns>
    public GameObject GameObjectManipulation(GameObject g, bool create)
    {
        return playerAbilities.GameObjectManipulation(g, create);
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
            return direction.normalized * range + (Vector2)playerAbilities.transform.position;
        }
        return point;
    }

    public Vector2 ClampPointWithinRange(Vector2 point)
    {
        return ClampPointWithinRange(point, Range);
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

    public void CleanupAllTargeting(GameObject user)
    {
        foreach(RuntimeAbilityTargetingData atd in _targetingData)
        {
            atd.Cleanup(this, user);
        }
    }
#endregion


#region OPERATORS
    /// <summary>
    /// Returns a tooltip with all of the values plugged in, child classes are expected to override this with a string.format of their own variables setup
    /// </summary>
    /// <returns>the tooltip </returns>
    public string GetTooltip(AbilityUpgradeSlot slot)  
    {
        if (tooltipDescriptions == null || (int)slot > tooltipDescriptions.Length || tooltipDescriptions[(int)slot] == null)
        {
            return "";
        }
        return PopulateTooltip(slot);
    }

    public void SetTooltips(string[] tooltips)
    {
        tooltipDescriptions = tooltips;
    }

    public void SetTooltip(AbilityUpgradeSlot slot, string s)
    {
        if(tooltipDescriptions == null || (int)slot >= tooltipDescriptions.Length)
        {
            tooltipDescriptions = new string[(int)slot + 1];
        }
        tooltipDescriptions[(int)slot] = s;
    }

    private string PopulateTooltip(AbilityUpgradeSlot slot)
    {
        string s = tooltipDescriptions[(int)slot];
        int i1 = s.IndexOf('{');
        int i2;
        int failSafe = -2;
        while(i1 != -1 && failSafe != i1)
        {
            i2 = s.IndexOf('}', i1);
            string variableName = s.Substring(i1, i2 - i1 + 1);

            s = s.Replace(variableName, GetStringValueOfMember(variableName));

            failSafe = i1;
            i1 = s.IndexOf('{');
            if(failSafe == i1)
            {
                throw new System.Exception("ERROR: TOOLTIP FOR ABILITY WITH ID:" + ID + "INVALID");
            }
        }
        return s;
    }

    private string GetStringValueOfMember(string variableName)
    {
        object result = "ERROR";
        Action<string, System.Type> attemptLambda = (name, type) =>
        {
            result = type.InvokeMember
            (
                name,
                BindingFlags.GetField | BindingFlags.GetProperty,
                null,
                this,
                null
            );
        };
        var nameArray = AbilityDataXMLParser.CommonRenames(variableName.Substring(1, variableName.Length - 2));
        foreach (var s in nameArray)
        {
            try
            {
                attemptLambda(s, GetType());
                //we can break if we haven't excepted at this point
                break;
            }
            #pragma warning disable 0168
            catch (System.MissingMethodException e)
            #pragma warning restore 0168
            {
            }
        }
        return result.ToString();
    }

    public new virtual string ToString()
    {
        if (maxDuration >= 0)
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
#endregion
}