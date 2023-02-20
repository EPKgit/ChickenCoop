using System;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "HitboxChainAsset")]
public class HitboxChainAsset : ScriptableObject
{
    public HitboxFrameAsset[] chain;
}
