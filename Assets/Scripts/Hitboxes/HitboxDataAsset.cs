using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable, CreateAssetMenu(menuName = "Hitboxes/HitboxDataAsset")]
public class HitboxDataAsset : ScriptableObject
{
    public HitboxShapeAsset ShapeAsset { get => _shape; }
    [SerializeField] HitboxShapeAsset _shape;
    public HitboxInteractionType InteractionType { get => _interactionType; }
    [SerializeField] HitboxInteractionType _interactionType = HitboxInteractionType.ALL;
    public HitboxRepeatPolicy RepeatPolicy { get => _repeatPolicy; }
    [SerializeField] HitboxRepeatPolicy _repeatPolicy = HitboxRepeatPolicy.ONLY_ONCE;
    public float RepeatCooldown { get => _repeatCooldown; }
    [SerializeField] float _repeatCooldown = -1;
    public LayerMask LayerMask { get => _layerMask; }
    [SerializeField] LayerMask _layerMask = 0;
    public float Duration { get => _duration; }
    [SerializeField] float _duration = 0;
}