using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectPopup : Poolable
{
    public Image image;
    private StatusEffectBase effect;
    private float maxDuration;
    private bool effectDestroyed = false;

    private void Awake()
    {
        if (image == null)
        {
            image = Lib.FindDownwardsInTree<Image>(gameObject);
        }
    }

    public override void Reset()
    {
        effect = null;
        image.sprite = null;
        effectDestroyed = false;
    }

    public void Setup(Sprite icon, StatusEffectBase effect)
    {
        image.sprite = icon;
        this.effect = effect;
        maxDuration = effect.duration;
        effectDestroyed = false;
        effect.OnRemoved += OnEffectDestroyed; //doesn't need to be removed because the effect is GCed
        effect.OnDurationChanged += OnEffectDurationChanged; //doesn't need to be removed because the effect is GCed
    }

    void OnEffectDestroyed(StatusEffectBase e)
    {
        effectDestroyed = true;
    }

    void OnEffectDurationChanged(float f)
    {
        maxDuration = f;
    }

    public bool EffectUpdate(float dt)
    {
        if(maxDuration <= 0 || effectDestroyed)
        {
            DestroySelf();
            return true;
        }
        float t = effect.duration / maxDuration;
        if (t <= 0)
        {
            DestroySelf();
            return true;
        }
        image.fillAmount = Mathf.Min(t, 1.0f);
        return false;
    }
}

