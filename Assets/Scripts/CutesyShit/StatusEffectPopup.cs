using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectPopup : Poolable
{
    public Image image;
    private float currentDuration;
    private float maxDuration;

    private void Awake()
    {
        if (image == null)
        {
            image = Lib.FindDownwardsInTree<Image>(gameObject);
        }
    }

    public override void Reset()
    {
        currentDuration = 0;
        image.sprite = null;
    }

    public void Setup(Sprite icon, float duration)
    {
        image.sprite = icon;
        currentDuration = maxDuration = duration;
    }

    public void SetNewDuration(float newDuration)
    {
        maxDuration = currentDuration = newDuration;
    }

    public bool EffectUpdate(float dt)
    {
        if(maxDuration <= 0)
        {
            DestroySelf();
            return true;
        }
        currentDuration -= Time.deltaTime;
        float t = currentDuration / maxDuration;
        if (t <= 0)
        {
            DestroySelf();
            return true;
        }
        image.fillAmount = t;
        return false;
    }
}

