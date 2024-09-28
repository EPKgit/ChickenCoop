using System;
using UnityEngine;

public delegate void MutableShieldApplicationDelegate(MutableShieldApplicationEventData msaed);
public delegate void ShieldApplicationNotificationDelegate(ShieldApplicationData sad);
public delegate void ShieldAbsorbedDamageNotificationDelegate(ShieldAbsorbtionData sad);

public class MutableShieldApplicationEventData
{
    public ShieldApplicationData data;
    public float value;
    public bool cancelled;

    public MutableShieldApplicationEventData(ShieldApplicationData data)
    {
        this.data = data;
        this.value = data.Value;
        this.cancelled = false;
    }
}

public class ShieldAbsorbtionData
{
    public float amountAbsorbed;
    public float remainingValue;
    public bool broken;
}

public class ShieldApplicationData
{
    public bool Valid { get; private set; } = false;
    public BilateralData BilateralData { get; private set; } = new BilateralData();
    public float Value { get; private set; }
    public float InitialValue { get; private set; } = -1;
    public StatInfluenceData StatInfluencedData { get; private set; } = new StatInfluenceData();

    public class ShieldApplicationDataBuilder
    {
        private ShieldApplicationData data;
        public ShieldApplicationDataBuilder(ShieldApplicationData data)
        {
            this.data = data;
        }

        public ShieldApplicationDataBuilder OverallSource(GameObject g)
        {
            data.BilateralData.OverallSource = g;
            return this;
        }

        public ShieldApplicationDataBuilder LocalSource(GameObject g)
        {
            data.BilateralData.LocalSource = g;
            return this;
        }

        public ShieldApplicationDataBuilder BothSources(GameObject g)
        {
            data.BilateralData.LocalSource = g;
            data.BilateralData.OverallSource = g;
            return this;
        }

        public ShieldApplicationDataBuilder Target(GameObject g)
        {
            data.BilateralData.Target = g;
            return this;
        }

        public ShieldApplicationDataBuilder Target(IShieldable i)
        {
            data.BilateralData.Target = i.attached;
            return this;
        }

        public ShieldApplicationDataBuilder Value(float f)
        {
            data.InitialValue = f;
            return this;
        }

        public ShieldApplicationDataBuilder PercentageStat(StatName stat)
        {
            data.StatInfluencedData.PercentageStat = stat;
            return this;
        }

        public ShieldApplicationDataBuilder FlatStat(StatName stat)
        {
            data.StatInfluencedData.FlatStat = stat;
            return this;
        }

        private bool Valid()
        {
            if (data.BilateralData.OverallSource == null || data.BilateralData.Target == null)
            {
                return false;
            }
            if (data.InitialValue < 0)
            {
                return false;
            }
            return true;
        }

        public ShieldApplicationData Finalize()
        {
            if (!Valid())
            {
                Debug.LogError("ERROR: ShieldChangeData created with invalid parameters");
                throw new Exception();
            }
            StatBlockComponent stats = Lib.FindDownwardsInTree<StatBlockComponent>(data.BilateralData.OverallSource);
            if (stats == null)
            {
                data.Value = data.InitialValue;
            }
            else
            {
                if (data.StatInfluencedData.FlatStat == StatName.MAX)
                {
                    data.StatInfluencedData.FlatStat = StatName.FlatShielding;
                }
                if (data.StatInfluencedData.PercentageStat == StatName.MAX)
                { 
                    data.StatInfluencedData.PercentageStat = StatName.ShieldingPercentage;
                }
                float flatIncrease = stats.GetValue(data.StatInfluencedData.FlatStat);
                float percentIncrease = stats.GetValue(data.StatInfluencedData.PercentageStat);
                data.Value = (data.InitialValue + flatIncrease) * percentIncrease;
            }
            data.Valid = true;
            return data;
        }
    }

    public static ShieldApplicationDataBuilder GetBuilder()
    {
        return new ShieldApplicationDataBuilder(new ShieldApplicationData());
    }
}

public interface IShieldable
{
    /// <summary>
    /// Apply a shield to the object. Implements on a per object basis
    /// </summary>
    /// <param name="data"> A conglomerate of all the parameters that could be relevant to an implementation of gaining a shield
    ///                     Recommended to use the ShieldApplicationDataBuilder for readability </param>
    void ApplyShield(ShieldApplicationData data);

    public GameObject attached
    {
        get;
    }
}