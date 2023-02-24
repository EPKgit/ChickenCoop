using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable, CreateAssetMenu(menuName = "HitboxDataAsset")]
public class HitboxDataAsset : ScriptableObject
{
    public HitboxShape Shape { get => _shape; }
    [SerializeField] HitboxShape _shape = HitboxShape.CIRCLE;
    public Vector2[] Points { get => _points; }
    [SerializeField] Vector2[] _points = null;
    public HitboxInteractionType InteractionType { get => _interactionType; }
    [SerializeField] HitboxInteractionType _interactionType = HitboxInteractionType.ALL;
    public HitboxRepeatPolicy RepeatPolicy { get => _repeatPolicy; }
    [SerializeField] HitboxRepeatPolicy _repeatPolicy = HitboxRepeatPolicy.ONLY_ONCE;
    public float RepeatCooldown { get => _repeatCooldown; }
    [SerializeField] float _repeatCooldown = -1;
    public LayerMask LayerMask { get => _layerMask; }
    [SerializeField] LayerMask _layerMask = 0;
    public float Radius { get => _radius; }
    [SerializeField] float _radius = 0.5f;
    public float Duration { get => _duration; }
    [SerializeField] float _duration = 0;
}