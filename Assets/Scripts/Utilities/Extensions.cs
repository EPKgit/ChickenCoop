using UnityEngine;
using static Ability;

public static class Extensions
{
    public static float GetDistanceTo(this Vector3 v, Vector3 other)
    {
        return (v - other).magnitude;
    }

    public static Vector3 GetDirectionTo(this Vector3 v, Vector3 other)
    {
        return other - v;
    }

    public static Vector3 GetDirectionToNormalized(this Vector3 v, Vector3 other)
    {
        return (other - v).normalized;
    }

    public static int intValue(this AbilityUpgradeSlot type)
    {
        return (int)type;
    }
}