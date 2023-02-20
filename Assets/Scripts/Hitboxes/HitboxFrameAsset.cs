using System;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "HitboxFrameAsset")]
public class HitboxFrameAsset : ScriptableObject
{
    public float delay;
    public HitboxDataAsset[] hitboxes;
}
