using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxChain
{
    public HitboxChainAsset chainData;
    public Func<Vector2> positionCallback;
    public Func<float> rotationCallback;
    public Action<Collider2D> onHitCallback;

    private float timer = 0;
    private int index = 0;

    public HitboxChain(HitboxChainAsset d, Func<Vector2> p, Func<float> r, Action<Collider2D> o)
    {
        chainData = d;
        positionCallback = p;
        rotationCallback = r;
        onHitCallback = o;
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
        while (timer >= chainData.Chain[index].Delay)
        {
            var currentFrame = chainData.Chain[index];
            foreach (var hitboxWithOffset in currentFrame.Hitboxes)
            {
                float rotZ = currentFrame.RotationZ + rotationCallback();
                Vector2 position = positionCallback() + (Vector2)(Quaternion.Euler(0, 0, rotZ) * (currentFrame.Offset + hitboxWithOffset.Offset));
                HitboxDataAsset assetData = hitboxWithOffset.Hitbox != null ? hitboxWithOffset.Hitbox : chainData.DefaultHitbox;
                HitboxData hitboxData = HitboxData.GetBuilder(assetData)
                                                    .Callback(onHitCallback)
                                                    .Duration(currentFrame.Duration < 0 ? chainData.DefaultDuration : currentFrame.Duration)
                                                    .StartRotationZ(rotZ)
                                                    .StartPosition(position)
                                                    .Radius(hitboxWithOffset.SizeOverride < 0 ? assetData.Radius : hitboxWithOffset.SizeOverride)
                                                    .Finalize();
                HitboxManager.instance.SpawnHitbox(hitboxData);
            }
            ++index;
            if (index >= chainData.Chain.Length)
            {
                return false;
            }
        }
        return true;
    }
}
