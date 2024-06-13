using System;
using UnityEngine;

public delegate void HealthValueSetDelegate(float newCurrent, float newMax);
public delegate void MutableHealthChangeDelegate(MutableHealthChangeEventData hced);
public delegate void HealthChangeNotificationDelegate(HealthChangeData hcnd);
public delegate void KilledCharacterDelegate(GameObject killed);

public class MutableHealthChangeEventData
{
    public HealthChangeData data;
    public float delta;
    public bool cancelled;
    public bool targetHasShield;

    public MutableHealthChangeEventData(HealthChangeData data, bool targetHasShield)
    {
        this.data = data;
        this.delta = data.Delta;
        this.cancelled = false;
        this.targetHasShield = targetHasShield;
    }
}

public class HealthChangeData
{
    public bool Valid { get; private set; } = false;
    public BilateralData BilateralData { get; private set; } = new BilateralData(); //required so we can safely init
    public float Delta { get; private set; }
    public float UnmodifiedDelta { get;  private set; }
    public KnockbackData KnockbackData { get; private set; } = null;
    public Func<Vector3> DamageLocation { get; private set; }
    public StatInfluencedData StatInfluencedData { get; private set; } = new StatInfluencedData();

    public class HealthChangeDataBuilder
    {
        private HealthChangeData data;
        public HealthChangeDataBuilder(HealthChangeData data)
        {
            this.data = data;
        }

        public HealthChangeDataBuilder OverallSource(GameObject g)
        {
            data.BilateralData.OverallSource = g;
            return this;
        }

        public HealthChangeDataBuilder LocalSource(GameObject g)
        {
            data.BilateralData.LocalSource = g;
            return this;
        }

        public HealthChangeDataBuilder BothSources(GameObject g)
        {
            data.BilateralData.LocalSource = g;
            data.BilateralData.OverallSource = g;
            return this;
        }

        public HealthChangeDataBuilder Target(GameObject g)
        {
            data.BilateralData.Target = g;
            return this;
        }

        public HealthChangeDataBuilder Target(IDamagable i)
        {
            data.BilateralData.Target = i.attached;
            return this;
        }

        public HealthChangeDataBuilder Target(IHealable i)
        {
            data.BilateralData.Target = i.attached;
            return this;
        }

        public HealthChangeDataBuilder Value(float f)
        {
            data.UnmodifiedDelta = f;
            return this;
        }

        public HealthChangeDataBuilder Damage(float f)
        {
            data.UnmodifiedDelta = -f;
            if (data.StatInfluencedData.PercentageStat == StatName.MAX)
            {
                data.StatInfluencedData.PercentageStat = StatName.DamagePercentage;
            }
            if (data.StatInfluencedData.FlatStat == StatName.MAX)
            {
                data.StatInfluencedData.FlatStat = StatName.FlatDamage;
            }
            return this;
        }

        public HealthChangeDataBuilder Healing(float f)
        {
            if (data.StatInfluencedData.PercentageStat == StatName.MAX)
            {
                data.StatInfluencedData.PercentageStat = StatName.HealingPercentage;
            }
            if (data.StatInfluencedData.FlatStat == StatName.MAX)
            {
                data.StatInfluencedData.FlatStat = StatName.FlatHealing;
            }
            return Value(f);
        }

        public HealthChangeDataBuilder KnockbackData(KnockbackData kbd)
        {
            data.KnockbackData = kbd;
            return this;
        }

        public HealthChangeDataBuilder KnockbackData(KnockbackPreset preset)
        {
            data.KnockbackData = PresetKnockbackData.GetKnockbackPreset(preset);
            return this;
        }

        public HealthChangeDataBuilder LocationFunction(Func<Vector3> f)
        {
            data.DamageLocation = f;
            return this;
        }

        public HealthChangeDataBuilder PercentageStat(StatName stat)
        {
            data.StatInfluencedData.PercentageStat = stat;
            return this;
        }

        public HealthChangeDataBuilder FlatStat(StatName stat)
        {
            data.StatInfluencedData.FlatStat = stat;
            return this;
        }

        private bool Valid()
        {
            if(data.BilateralData.OverallSource == null || data.BilateralData.Target == null)
            {
                return false;
            }
            return true;
        }

        public HealthChangeData Finalize()
        {
            if (!Valid())
            {
                Debug.LogError("ERROR: HealthChangeBuilder created with invalid parameters");
                throw new Exception();
            }
            StatBlockComponent stats = Lib.FindDownwardsInTree<StatBlockComponent>(data.BilateralData.OverallSource);
            if (stats == null)
            {
                data.Delta = data.UnmodifiedDelta;
            }
            else
            {
                bool negative = data.UnmodifiedDelta < 0;
                float flatIncrease = stats.GetValue(data.StatInfluencedData.FlatStat);
                float percentIncrease = stats.GetValue(data.StatInfluencedData.PercentageStat);
                if(negative)
                {
                    data.Delta = (data.UnmodifiedDelta - flatIncrease) * percentIncrease;
                }
                else
                {
                    data.Delta = (data.UnmodifiedDelta + flatIncrease) * percentIncrease;
                }
            }
            data.Valid = true;
            return data;
        }
    }

    public static HealthChangeDataBuilder GetBuilder()
    {
        return new HealthChangeDataBuilder(new HealthChangeData());
    }
}

public interface IDamagable
{
    /// <summary>
    /// Damage the object. Implements on a per object basis
    /// </summary>
    /// <param name="data"> A conglomerate of all the parameters that could be relevant to an implementation of taking damage.
    ///                     Recommended to use the HealthChangeDataBuilder for readability </param>
    void Damage(HealthChangeData data);

    public GameObject attached
    {
        get;
    }
}