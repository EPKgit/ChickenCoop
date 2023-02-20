using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable, CreateAssetMenu(menuName = "HitboxDataAsset")]
public class HitboxDataAsset : ScriptableObject
{
     public HitboxShape shape = HitboxShape.CIRCLE;
     public Vector2[] points = null;
     public HitboxInteractionType interactionType = HitboxInteractionType.ALL;
     public HitboxRepeatPolicy repeatPolicy = HitboxRepeatPolicy.ONLY_ONCE;
     public float repeatCooldown = -1;
     public LayerMask layerMask = 0;
     public float radius = 0.5f;
     public float duration = -1;
}