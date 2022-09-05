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

    public MutableHealthChangeEventData(HealthChangeData data)
    {
        this.data = data;
        this.delta = data.delta;
        this.cancelled = false;
    }
}

public class HealthChangeData
{
    public GameObject overallSource { get; private set; }
    public GameObject localSource { get; private set; }
    public GameObject target { get; private set; }
    public float delta { get; private set; }
    public float unmodifiedDelta { get;  private set; }
    public KnockbackData knockbackData { get; private set; }
    public Func<Vector3> damageLocation { get; private set; }
    public StatName flatStat { get => _flatStat; private set => _flatStat = value; }
    private StatName _flatStat = StatName.MAX;
    public StatName percentageStat { get => _percentageStat; private set => _percentageStat = value; }
    private StatName _percentageStat = StatName.MAX;
    public class HealthChangeDataBuilder
    {
        public HealthChangeData data;
        public HealthChangeDataBuilder(HealthChangeData data)
        {
            this.data = data;
        }

        public HealthChangeDataBuilder OverallSource(GameObject g)
        {
            data.overallSource = g;
            return this;
        }

        public HealthChangeDataBuilder LocalSource(GameObject g)
        {
            data.localSource = g;
            return this;
        }

        public HealthChangeDataBuilder BothSources(GameObject g)
        {
            data.localSource = g;
            data.overallSource = g;
            return this;
        }

        public HealthChangeDataBuilder Target(GameObject g)
        {
            data.target = g;
            return this;
        }

        public HealthChangeDataBuilder Target(IDamagable i)
        {
            data.target = i.attached;
            return this;
        }

        public HealthChangeDataBuilder Target(IHealable i)
        {
            data.target = i.attached;
            return this;
        }

        public HealthChangeDataBuilder Value(float f)
        {
            data.unmodifiedDelta = f;
            return this;
        }

        public HealthChangeDataBuilder Damage(float f)
        {
            data.unmodifiedDelta = -f;
            if (data.percentageStat == StatName.MAX)
            {
                data.percentageStat = StatName.DamagePercentage;
            }
            if (data.flatStat == StatName.MAX)
            {
                data.flatStat = StatName.FlatDamage;
            }
            return this;
        }

        public HealthChangeDataBuilder Healing(float f)
        {
            if (data.percentageStat == StatName.MAX)
            {
                data.percentageStat = StatName.HealingPercentage;
            }
            if (data.flatStat == StatName.MAX)
            {
                data.flatStat = StatName.FlatHealing;
            }
            return Value(f);
        }

        public HealthChangeDataBuilder KnockbackData(KnockbackData kbd)
        {
            data.knockbackData = kbd;
            return this;
        }

        public HealthChangeDataBuilder KnockbackData(KnockbackPreset preset)
        {
            data.knockbackData = PresetKnockbackData.GetKnockbackPreset(preset);
            return this;
        }

        public HealthChangeDataBuilder LocationFunction(Func<Vector3> f)
        {
            data.damageLocation = f;
            return this;
        }

        public HealthChangeDataBuilder PercentageStat(StatName stat)
        {
            data.percentageStat = stat;
            return this;
        }

        public HealthChangeDataBuilder FlatStat(StatName stat)
        {
            data.flatStat = stat;
            return this;
        }

        private bool Valid()
        {
            if(data.overallSource == null)
            {
                return false;
            }
            if(data.target == null)
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
            StatBlockComponent stats = Lib.FindDownwardsInTree<StatBlockComponent>(data.overallSource);
            if (stats == null)
            {
                data.delta = data.unmodifiedDelta;
            }
            else
            {
                bool negative = data.unmodifiedDelta < 0;
                float flatIncrease = stats.GetValueOrDefault(data.flatStat);
                float percentIncrease = stats.GetValueOrDefault(data.percentageStat);
                if(negative)
                {
                    data.delta = (data.unmodifiedDelta - flatIncrease) * percentIncrease;
                }
                else
                {
                    data.delta = (data.unmodifiedDelta + flatIncrease) * percentIncrease;
                }
            }
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