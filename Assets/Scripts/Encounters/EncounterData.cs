using System;
using System.Collections.Generic;
using UnityEngine;

namespace Encounters
{
    public enum SpawnType
    {
        RANDOM,
        SEQUENTIAL,
        MAX,
    }

    [System.Serializable]
    public class WaveData
    {
        public EnemyType enemyType;
        public int maxAmount;
        public SpawnType spawnType;
    }

    public enum EncounterState
    {
        NOT_IN_ENCOUNTER,
        ENCOUNTER_STARTED,
        ENCOUNTER_ENDED,
    }
}
