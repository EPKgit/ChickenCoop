using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructTeleporter_Ability : Ability
{
    public GameObject teleporterPadPrefab;
    private int tpUses;
    private GameObject pad1;
    private GameObject pad2;

    public override void Initialize(PlayerAbilities pa)
    {
        PoolManager.instance.AddPoolSize(teleporterPadPrefab, 2, true);
        base.Initialize(pa);
    }

    public override void Cleanup(PlayerAbilities pa)
    {
        PoolManager.instance.RemovePoolSize(teleporterPadPrefab, 2);
        base.Cleanup(pa);
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        pad1 = PoolManager.instance.RequestObject(teleporterPadPrefab);
        pad2 = PoolManager.instance.RequestObject(teleporterPadPrefab);
        TeleporterPad pad1Script = pad1.GetComponent<TeleporterPad>();
        TeleporterPad pad2Script = pad2.GetComponent<TeleporterPad>();
        pad1Script.isPermanent = pad2Script.isPermanent = false;
        pad1Script.useNumber = pad2Script.useNumber = tpUses;
        pad1Script.lifetime = pad2Script.lifetime = maxDuration;
        pad1Script.otherSide = pad2;
        pad2Script.otherSide = pad1;
        pad1.GetComponent<SpriteRenderer>().color = Color.red;
        pad2.GetComponent<SpriteRenderer>().color = Color.blue;

        pad1.transform.position = playerAbilities.transform.position;
        pad2.transform.position = ClampPointWithinRange(targetingData.inputPoint);
        pad1Script.onPoolableReturned += OnPadDestroyed;
        pad2Script.onPoolableReturned += OnPadDestroyed;
    }

    public void OnPadDestroyed(GameObject pad)
    {
        currentDuration = 0;
        TeleporterPad pad1Script = pad1.GetComponent<TeleporterPad>();
        TeleporterPad pad2Script = pad2.GetComponent<TeleporterPad>();
        pad1Script.onPoolableReturned -= OnPadDestroyed;
        pad2Script.onPoolableReturned -= OnPadDestroyed;
        pad1Script.DestroySelf();
        pad2Script.DestroySelf();
    }
}
