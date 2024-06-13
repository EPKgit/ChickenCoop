using UnityEngine;

/// <summary>
/// Contains data necessary for an effect that has both an instigator
/// and a target
/// </summary>
public class BilateralData
{
    public GameObject OverallSource = null;
    public GameObject LocalSource = null;
    public GameObject Target = null;
}

public class StatInfluencedData
{
    public StatName FlatStat = StatName.MAX;
    public StatName PercentageStat = StatName.MAX;
}