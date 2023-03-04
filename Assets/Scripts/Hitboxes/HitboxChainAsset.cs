using System;
using UnityEngine;

public enum HitboxFrameInteractionPolicy
{
    SINGLE,
    MULTIPLE,
}

[Serializable, CreateAssetMenu(menuName = "HitboxChainAsset")]
public class HitboxChainAsset : ScriptableObject
{
    public float DefaultDuration { get => _defaultDuration; }
    [SerializeField] float _defaultDuration = 0;

    public HitboxDataAsset DefaultHitbox { get => _defaultHitbox; }
    [SerializeField] HitboxDataAsset _defaultHitbox;

    public HitboxFrame[] Chain { get => _chain; }
    [SerializeField] HitboxFrame[] _chain;

    [Serializable]
    public class HitboxFrame
    {
        public float Delay { get => _delay; }
        [SerializeField] float _delay = 0;

        public float Duration { get => _duration; }
        [SerializeField] float _duration = -1;

        public Vector2 Offset { get => _offset; }
        [SerializeField] Vector2 _offset;

        public float RotationZ { get => _rotationZ; }
        [SerializeField] float _rotationZ = 0;

        public HitboxWithOffset[] Hitboxes { get => _hitboxes; }
        [SerializeField] HitboxWithOffset[] _hitboxes;

        public HitboxFrameInteractionPolicy Policy { get => _policy; }
        [SerializeField] HitboxFrameInteractionPolicy _policy = HitboxFrameInteractionPolicy.SINGLE;
    }

    [Serializable]
    public class HitboxWithOffset
    {
        public HitboxDataAsset Hitbox { get => _hitbox; }
        [SerializeField] HitboxDataAsset _hitbox;

        public Vector2 Offset { get => _offset; }
        [SerializeField] Vector2 _offset;

        public float SizeOverride { get => _sizeOverride; }
        [SerializeField] float _sizeOverride = -1;
    }
}