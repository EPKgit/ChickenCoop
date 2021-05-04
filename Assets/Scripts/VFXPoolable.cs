using UnityEngine;
using UnityEngine.VFX;

public class VFXPoolable : Poolable
{
    private VisualEffect effect;
    private bool active = false;
    private bool initialized = false;
    void Awake()
    {
        effect = GetComponent<VisualEffect>();
    }

    public override void Reset()
    {
        base.Reset();
        active = true;
        initialized = false;
        effect.Play();
    }

    void Update()
    {
        if(!active)
        {
            return;
        }
        if (effect.aliveParticleCount != 0)
        {
            if (!initialized)
            {
                initialized = true;
            }
        }
        else
        {
            if (initialized)
            {
                active = false;
                initialized = false;
                effect.Stop();
                DestroySelf();
            }
        }
        
    }
}
