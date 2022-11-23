using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterPad : BaseInteractable
{
    public GameObject otherSide;
    public Vector3 teleportPosition;

    public bool isPermanent = true;
    public int useNumber = 0;
    public float lifetime = 0;
    public float maxCooldown = 2.0f;
    private float currentCooldown = 0.0f;

    protected override bool CanDo()
    {
        if(currentCooldown <= 0.0f)
        {
            return true;
        }
        return false;
    }

    protected override void ToDo(GameObject user)
    {
        PlayerMovement pm = Lib.FindDownThenUpwardsInTree<PlayerMovement>(user);
        if(pm == null)
        {
            return;
        }
        currentCooldown = maxCooldown;
        pm.TeleportInput(otherSide != null ? otherSide.transform.position : teleportPosition);
        if(isPermanent)
        {
            return;
        }
        --useNumber;
        if(useNumber <= 0)
        { 
            DestroySelf();
        }
    }

    private void Update()
    {
        if (currentCooldown > 0.0f)
        {
            currentCooldown -= Time.deltaTime;
        }
        if(isPermanent)
        {
            return;
        }
        lifetime -= Time.deltaTime;
        if (lifetime <= 0.0f)
        {
            DestroySelf();
        }
    }

    public override void Reset()
    {
        base.Reset();
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
