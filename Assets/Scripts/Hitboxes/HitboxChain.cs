using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxChain
{
    public HitboxChainAsset chainData;
    public Func<Vector2> positionCallback;
    public Func<float> rotationCallback;
    public Action<Collider2D, Hitbox, int> onHitCallback;
    public Action<int> onSpawnHitboxCallback;

    private float timer = 0;
    private int index = 0;

    public HitboxChain(HitboxChainAsset d, Func<Vector2> p, Func<float> r, Action<Collider2D, Hitbox, int> onHitCallback, Action<int> onSpawnHitboxCallback = null)
    {
        chainData = d;
        positionCallback = p;
        rotationCallback = r;
        this.onHitCallback = onHitCallback;
        this.onSpawnHitboxCallback = onSpawnHitboxCallback;
        timer = -Time.deltaTime;
        index = 0;
        UpdateChain();
    }

    public bool UpdateChain()
    {
        if (index >= chainData.Chain.Length)
        {
            return false;
        }
        timer += Time.deltaTime;
        while (timer >= chainData.Chain[index].StartAt)
        {
            var currentFrame = chainData.Chain[index];
            Dictionary<GameObject, float> its = null;
            if(currentFrame.Policy == HitboxFrameInteractionPolicy.SINGLE)
            {
                its = new Dictionary<GameObject, float>();
            }
            foreach (var hitboxWithOffset in currentFrame.Hitboxes)
            {
                float rotZ = currentFrame.RotationZ + rotationCallback();
                Vector2 position = positionCallback() + (Vector2)(Quaternion.Euler(0, 0, rotZ) * (currentFrame.Offset + hitboxWithOffset.Offset));
                HitboxDataAsset assetData = hitboxWithOffset.Hitbox != null ? hitboxWithOffset.Hitbox : chainData.DefaultHitbox;
                HitboxData hitboxData = HitboxData.GetBuilder(assetData)
                                                    .Callback((collider, hitbox) => { onHitCallback(collider, hitbox, index); })
                                                    .Duration(currentFrame.Duration < 0 ? chainData.DefaultDuration : currentFrame.Duration)
                                                    .StartRotationZ(rotZ)
                                                    .StartPosition(position)
                                                    .Radius(hitboxWithOffset.SizeOverride < 0 ? assetData.ShapeAsset.Radius : hitboxWithOffset.SizeOverride)
                                                    .InteractionTimeStamps(its)
                                                    .Finalize();
                HitboxManager.instance.SpawnHitbox(hitboxData);
            }
            onSpawnHitboxCallback(index);
            ++index;
            if (index >= chainData.Chain.Length)
            {
                return false;
            }
        }
        return true;
    }
}
