using System;
using UnityEngine;

public delegate void HealthValueSetDelegate(float newCurrent, float newMax);
public delegate void MutableHealthChangeDelegate(MutableHealthChangeEventData hced);
public delegate void HealthChangeNotificationDelegate(HealthChangeData hcnd);
public delegate void KilledCharacterDelegate(GameObject killed);

public class MutableHealthChangeEventData
{
    public HealthChangeData data;
    public float rawDelta;
    public float finalDelta;
    public bool cancelled;
    public bool targetHasShield;

    public MutableHealthChangeEventData(HealthChangeData data, bool targetHasShield)
    {
        this.data = data;
        this.rawDelta = data.UnmodifiedDelta;
        this.finalDelta = data.Delta;
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
    public Func<Vector3> ChangeLocation { get; private set; }
    public StatInfluenceData InstigatorStatData { get; private set; } = new StatInfluenceData();
    public StatInfluenceData ResistorStatData { get; private set; } = new StatInfluenceData();

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
            if (data.InstigatorStatData.PercentageStat == StatName.MAX)
            {
                data.InstigatorStatData.PercentageStat = StatName.DamageAmplification;
            }
            if (data.InstigatorStatData.FlatStat == StatName.MAX)
            {
                data.InstigatorStatData.FlatStat = StatName.FlatDamage;
            }
            if(data.ResistorStatData.PercentageStat == StatName.MAX)
            {
                data.ResistorStatData.PercentageStat = StatName.DamageResistance;
            }
            if (data.ResistorStatData.FlatStat == StatName.MAX)
            {
                data.ResistorStatData.FlatStat = StatName.Armor;
            }
            return this;
        }

        public HealthChangeDataBuilder Healing(float f)
        {
            if (data.InstigatorStatData.PercentageStat == StatName.MAX)
            {
                data.InstigatorStatData.PercentageStat = StatName.HealingPercentage;
            }
            if (data.InstigatorStatData.FlatStat == StatName.MAX)
            {
                data.InstigatorStatData.FlatStat = StatName.FlatHealing;
            }
            if (data.ResistorStatData.PercentageStat == StatName.MAX)
            {
                data.ResistorStatData.PercentageStat = StatName.HealingResistance;
            }
            if (data.ResistorStatData.FlatStat == StatName.MAX)
            {
                data.ResistorStatData.FlatStat = StatName.HealingArmor;
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
            data.ChangeLocation = f;
            return this;
        }

        public HealthChangeDataBuilder InstigatorPercentageStat(StatName stat)
        {
            data.InstigatorStatData.PercentageStat = stat;
            return this;
        }

        public HealthChangeDataBuilder IntigatorFlatStat(StatName stat)
        {
            data.InstigatorStatData.FlatStat = stat;
            return this;
        }

        public HealthChangeDataBuilder ResistorPercentageStat(StatName stat)
        {
            data.ResistorStatData.PercentageStat = stat;
            return this;
        }

        public HealthChangeDataBuilder ResistorFlatStat(StatName stat)
        {
            data.ResistorStatData.FlatStat = stat;
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
                data.Delta = Mathf.Abs(data.UnmodifiedDelta);
                
                float flatIncrease = stats.GetValue(data.InstigatorStatData.FlatStat);
                float percentIncrease = stats.GetValue(data.InstigatorStatData.PercentageStat);
                data.Delta = (data.Delta + flatIncrease) * (1.0f + percentIncrease);

                float flatDecrease = stats.GetValue(data.ResistorStatData.FlatStat);
                float percentDecrease = stats.GetValue(data.ResistorStatData.PercentageStat);
                data.Delta = data.Delta * (1.0f - percentDecrease) - flatDecrease;

                if(data.UnmodifiedDelta < 0)
                {
                    data.Delta = -data.Delta;
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