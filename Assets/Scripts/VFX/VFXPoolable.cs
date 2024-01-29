using UnityEngine;
using UnityEngine.VFX;

public class VFXPoolable : Poolable
{
    public bool looping = false;
    public float fixedDuration = -1;

    protected VisualEffect effect;
    protected bool active = false;
    protected bool initialized = false;
    protected float fixedTimer = 0;
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
        fixedTimer = 0;
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;
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
        if (fixedDuration > 0)
        {
            fixedTimer += Time.deltaTime;
            if (fixedTimer > fixedDuration)
            {
                Cleanup();
            }
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
