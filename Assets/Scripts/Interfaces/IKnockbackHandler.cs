using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackData
{
    public Vector2 direction;
    public float force;
    public float duration;
}
public enum KnockbackPreset
{
    TINY,
    LITTLE,
    MEDIUM,
    BIG,
    MAX,
}
public static class PresetKnockbackData
{
    
    public static Dictionary<KnockbackPreset, KnockbackData> presets = new Dictionary<KnockbackPreset, KnockbackData>()
    {
        { KnockbackPreset.TINY,     new KnockbackData() { duration = 0.1f,    force = 0.2f }},
        { KnockbackPreset.LITTLE,   new KnockbackData() { duration = 0.1f,    force = 0.5f }},
        { KnockbackPreset.MEDIUM,   new KnockbackData() { duration = 0.2f,    force = 0.75f }},
        { KnockbackPreset.BIG,      new KnockbackData() { duration = 0.3f,    force = 1.0f }},
    };
    public static KnockbackData GetKnockbackPreset(KnockbackPreset preset, Vector2? direction = null)
    {
        KnockbackData data = presets[preset];
        if(direction != null)
        {
            data.direction = direction.Value.normalized;
        }
        return data;
    }
}
public interface IKnockbackHandler 
{
    public Vector3 position { get; }

    public abstract void DoKnockback(KnockbackData data);
}
