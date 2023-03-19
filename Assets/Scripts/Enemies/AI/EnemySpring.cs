using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EnemyBehaviourSyntax
{
    public enum SpringForceType
    {
        LINEAR,
        INVERSE_CUBE,
        MAX,
    }

    public enum SpringType
    {
        PUSH,
        PULL,
    }

    public class SpringData
    {
        public GameObject attached;
        public GameObject targetedPlayer;
        public List<PlayerInitialization> allPlayers;
        public Collider2D[] inRange;
    }

    public abstract class EnemySpring
    {
        //public abstract SpringForceType forceType { get;}
        //public SpringType type { get; }
        public abstract Vector2 EvaluateDirection(SpringData data);

#if UNITY_EDITOR
        public abstract void DrawGizmo();
#endif

    }

    public class MoveTowardsTargetPlayerSpring : EnemySpring
    {
        public override Vector2 EvaluateDirection(SpringData data)
        {
            Vector3 start = data.attached.transform.position;
            Vector3 end = data.targetedPlayer.transform.position;
            //float dist = start.GetDistanceTo(end);
            float dist = 1.0f;
            Vector2 direction = start.GetDirectionToNormalized(end);

#if UNITY_EDITOR
            debugInfo = (start, end, dist);
#endif
            return direction * dist;
        }

#if UNITY_EDITOR
        private (Vector3, Vector3, float) debugInfo;
        public override void DrawGizmo()
        {
            Vector3 start = debugInfo.Item1;
            Vector3 end = debugInfo.Item2;
            float dist = debugInfo.Item3;
            if (dist > 0)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.grey;
            }

            Gizmos.DrawLine(start, end);
            Handles.Label((start + end) * 0.5f, dist.ToString());
        }
#endif
    }

    public class SeperateEnemiesSpring : EnemySpring
    {
        public override Vector2 EvaluateDirection(SpringData data)
        {
#if UNITY_EDITOR
            debugInfo.Clear();
#endif
            Vector2 final = Vector2.zero;
            foreach (Collider2D col in data.inRange)
            {
                GameObject curr = col.gameObject;
                if (Lib.FindUpwardsInTree<BaseEnemy>(curr) != null)
                {
                    Vector3 start = data.attached.transform.position;
                    Vector3 end = curr.transform.position;
                    float dist = Mathf.Clamp(start.GetDistanceTo(end), 0, float.MaxValue);
                    Vector2 direction = data.attached.transform.position.GetDirectionToNormalized(curr.transform.position);

                    float springStrength = 1.0f / (1.0f + dist * dist * dist);
                    final += -direction * springStrength;
#if UNITY_EDITOR
                    debugInfo.Add((start, end, springStrength));
#endif
                }
            }
            return final;
        }

#if UNITY_EDITOR
        private List<(Vector3, Vector3, float)> debugInfo = new List<(Vector3, Vector3, float)>();
        public override void DrawGizmo()
        {
            foreach (var data in debugInfo)
            {
                Vector3 start = data.Item1;
                Vector3 end = data.Item2;
                float springStrength = data.Item3;
                if (springStrength > 0)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.grey;
                }
                Gizmos.DrawLine(start, end);
                Handles.Label((start + end) * 0.5f, springStrength.ToString());
            } 
        }
#endif
    }
}
