using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerGameplay/AbilityBucket")]
public class AbilityBucket : ScriptableObject
{
    public new string name;
    public List<AbilityBucket> subBuckets;
    public List<Ability> abilities;
}
