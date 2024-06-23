using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

namespace Encounters
{ 
    public enum SpawnType
    {
        USE_GENERAL_RANDOM, //use the generic spawn points set per wave
        SINGLE_POINT, //used for single enemy spawns
        RANDOM_IN_AREA, // spawn randomly within an area may overlap
        SEQUENTIAL_OVER_POINTS, // spawn at the points in order, looping if  there are more spawn than points
        RANDOM_OVER_POINTS, // spawn at random points non-repeating until we run out then start over with overlaps
        MAX,
    }

    [System.Serializable]
    public class SpawnData
    {
        public SpawnType spawnType;
        public Vector2 spawnPoint
        {
            get => spawnPoints[0];
        }
        public Vector2[] spawnPoints;
        public Rect spawnArea;
        public int spawnCount;

        public void OnDrawGizmosSelected(Vector2 encounterPosition, bool doColor)
        {
            if (doColor)
            {
                Gizmos.color = Color.red;
            }

            switch (spawnType)
            {
                case SpawnType.USE_GENERAL_RANDOM:
                    //can't draw this without spawn point data
                    break;
                case SpawnType.RANDOM_OVER_POINTS:
                {
                    int index = 0;
                    foreach (Vector2 v in spawnPoints)
                    {
                        Gizmos.DrawLine(encounterPosition, encounterPosition + v);
                        Handles.Label(encounterPosition + v, $"RandomSpawnPoint{index++}");
                    }
                } break;
                case SpawnType.SINGLE_POINT:
                    Gizmos.DrawLine(encounterPosition, encounterPosition + spawnPoint);
                    Handles.Label(encounterPosition + spawnPoint, "Single Spawn");
                    break;
                case SpawnType.SEQUENTIAL_OVER_POINTS:
                {
                    int index = 0;
                    foreach (Vector2 v in spawnPoints)
                    {
                        Gizmos.DrawLine(encounterPosition, encounterPosition + v);
                        Handles.Label(encounterPosition + v, $"Sequential{index++}");
                    }
                } break;
                case SpawnType.RANDOM_IN_AREA:
                {
                    Gizmos.DrawLine(encounterPosition, spawnArea.center + encounterPosition);
                    Color prev = Gizmos.color;
                    Gizmos.color = new Color(prev.r, prev.g, prev.b, 0.1f);
                    Gizmos.DrawCube(spawnArea.center + encounterPosition, spawnArea.size);
                    Gizmos.color = prev;
                    Handles.Label(spawnArea.center + encounterPosition, "SpawnArea");
                } break;
                default:
                    throw new System.Exception();
            }

        }
    }

    [System.Serializable]
    public class EnemyWaveData
    {
#if UNITY_EDITOR_WIN
        public bool drawGizmos = true;
#endif
        [System.Serializable]
        public struct EnemySpawnData
        {
            public EnemyType enemyType;
            public SpawnData spawnData;
        }
        public EnemySpawnData[] spawns;

        
        public void OnDrawGizmosSelected(Vector2 encounterPosition, bool doColor)
        {
            #if UNITY_EDITOR_WIN
            if (!drawGizmos)
            {
                return;
            }
            #endif
            foreach(EnemySpawnData data in spawns) 
            {
                data.spawnData.OnDrawGizmosSelected(encounterPosition, doColor);
            }
        }
    }



    [System.Serializable]
    public class GatheringWaveData
    {
        public SpawnData[] spawns;
    }

    public enum EncounterState
    {
        NOT_IN_ENCOUNTER,
        ENCOUNTER_STARTED,
        ENCOUNTER_ENDED,
    }
}
