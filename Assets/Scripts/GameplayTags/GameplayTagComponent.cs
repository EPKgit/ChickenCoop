using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayTagComponent : MonoBehaviour
{
    [Header("ONLY EDIT THE TAGS ON THE COMPONENT FOR TAGS INTRINSIC TO THE OBJECT")]
    [Header("NOTE: THESE TAGS ARE PERMANENT UNLESS REMOVED BY ANOTHER EFFECT")]
    public GameplayTagContainer tags;
    public GameplayTagContainer blockedTags;
}
