using UnityEngine;
using UnityEngine.VFX;

public class VFXPoolable : Poolable
{
    public bool looping = false;
    protected VisualEffect effect;
    protected bool active = false;
    protected bool initialized = false;
    protected virtual void Awake()
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

    public void StopParticlePlaying()
    {
        effect.Stop();
        if(!looping)
        {
            Cleanup();
        }
        else
        {
            initialized = true;
        }
    }

    protected virtual void Update()
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
                Cleanup();
            }
        }
    }

    protected virtual void Cleanup()
    {
        active = false;
        initialized = false;
        effect.Stop();
        DestroySelf();
    }
}
