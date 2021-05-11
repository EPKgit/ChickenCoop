using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayTagComponent : MonoBehaviour
{
    //[Header("NOTE: THESE TAGS ARE PERMANENT UNLESS REMOVED BY ANOTHER EFFECT")]
    //[Header("ONLY EDIT THE TAGS ON THE COMPONENT FOR TAGS INTRINSIC TO THE OBJECT")]
    public GameplayTagContainer tags;
    public GameplayTagContainer blockedTags;
}
