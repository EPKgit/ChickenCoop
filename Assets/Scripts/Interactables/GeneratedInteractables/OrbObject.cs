using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbObject : BaseInteractable
{
    public GameObject otherSide;
    public Vector3 teleportPosition;

    protected override bool CanInteract()
    {
        return true;
    }

    protected override void PerformInteract(GameObject user)
    {
        PlayerAbilities pa = Lib.FindDownThenUpwardsInTree<PlayerAbilities>(user);
        if(pa == null)
        {
            return;
        }
    }

    private void Update()
    {
        
    }

    public override void Reset()
    {
        base.Reset();
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}

