using UnityEngine;
public class DebugFlags
{
    public enum FLAGS
    {
        MENU = 0,
        MOVEMENT = 0,
        LIB = 0,
        COLLISIONS = 0,
        HEALTH = 0,
        ENEMYHEALTH = 0,
        STATS = 0,
        ABILITY = 0,
        POOLMANAGER = 0,
        AGGRO = 0,
        INTERACTABLES = 0,
        AIMING = 0,
        HEALTHCALLBACKS = 0,
        INGAMEUI = 0,
        RENDER = 0,
        ABILITYQUEUE = 0,
        HITBOXES_EDITOR_ALWAYS = 0,
        HITBOXES_IN_GAME = 1,
    }

    public static void Log(FLAGS flag, string printString)
    {
        if((int)flag == 1)
        {
            Debug.Log(printString);
        }
    }
}
