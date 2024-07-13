using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable, CreateAssetMenu(menuName = "Hitboxes/HitboxShapeAsset")]
public class HitboxShapeAsset : ScriptableObject
{
    public HitboxShapeType Type { get => _type; }
    [SerializeField] HitboxShapeType _type = HitboxShapeType.CIRCLE;
    public Vector2[] Points { get => _points; }
    [SerializeField] Vector2[] _points = null;
    public float Radius { get => _radius; }
    [SerializeField] float _radius = -1.0f;
    public float Length { get => _length; }
    [SerializeField] float _length = -1.0f;
}