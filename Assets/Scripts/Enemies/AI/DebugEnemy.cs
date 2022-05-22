using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DebugEnemy : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override bool Update()
	{
        if (!base.Update())
        {
            return false;
        }
        return true;
	}
}
