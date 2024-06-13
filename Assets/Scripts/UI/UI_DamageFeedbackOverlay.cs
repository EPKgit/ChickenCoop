using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DamageFeedbackOverlay : MonoBehaviour
{
    public Color damageColor = Color.red;
    public Color shieldDamageColor = Color.blue;
    public float fadeTime = 0.5f;

    private Material healthOverlayMaterial;
    private Material shieldOverlayMaterial;
    private int intensityPropertyID;
    private float currentHealthIntensity
    {
        get => _currentHealthIntensity;
        set
        {
            _currentHealthIntensity = value;
            healthIntensityDirty = true;
        }
    }
    private float _currentHealthIntensity;
    private bool healthIntensityDirty = false;

    private float currentShieldIntensity
    {
        get => _currentShieldIntensity;
        set
        {
            _currentShieldIntensity = value;
            shieldIntensityDirty = true;
        }
    }
    private float _currentShieldIntensity;
    private bool shieldIntensityDirty = false;


    private List<PlayerInitialization> subscribedDamageProducers;
    void Awake()
    {
        var image = transform.GetChild(0).GetComponent<Image>();
        healthOverlayMaterial = image.material = new Material(image.material);
        image = transform.GetChild(1).GetComponent<Image>();
        shieldOverlayMaterial = image.material = new Material(image.material);
        intensityPropertyID = Shader.PropertyToID("_CurrentIntensity");
        currentHealthIntensity = 0;
        currentShieldIntensity = 0;
        healthOverlayMaterial.SetFloat(intensityPropertyID, currentHealthIntensity);
        shieldOverlayMaterial.SetFloat(intensityPropertyID, currentShieldIntensity);
        subscribedDamageProducers = new List<PlayerInitialization>();
    }

    void OnEnable()
    {
        PlayerInitialization.OnPlayerNumberChanged += OnPlayerNumberChanged;
        OnPlayerNumberChanged();
    }

    void OnDisable()
    {
        PlayerInitialization.OnPlayerNumberChanged -= OnPlayerNumberChanged;
    }

    void OnPlayerNumberChanged()
    {
        foreach(var player in PlayerInitialization.all)
        {
            BaseHealth hp = player.GetComponent<BaseHealth>();
            if(hp == null)
            {
                continue;
            }
            if(subscribedDamageProducers.Contains(player))
            {
                hp.postDamageNotification -= OnDamageTaken;
                hp.shieldAbsorbedDamageNotification -= OnShieldAbsorbed;
                subscribedDamageProducers.Remove(player);
            }
            hp.postDamageNotification += OnDamageTaken;
            hp.shieldAbsorbedDamageNotification += OnShieldAbsorbed;
            subscribedDamageProducers.Add(player);
        }
    }

    void OnDamageTaken(HealthChangeData hcd)
    {
        healthIntensityDirty = true;
        currentHealthIntensity = 1.0f;
    }

    void OnShieldAbsorbed(ShieldAbsorbtionData sad)
    {
        shieldIntensityDirty = true;
        currentShieldIntensity = 1.0f;
    }

    void Update()
    {
        if(healthIntensityDirty)
        {
            healthOverlayMaterial.SetFloat(intensityPropertyID, currentHealthIntensity);
            healthIntensityDirty = false;
        }
        if (shieldIntensityDirty)
        {
            shieldOverlayMaterial.SetFloat(intensityPropertyID, currentShieldIntensity);
            shieldIntensityDirty = false;
        }
        if (currentHealthIntensity > 0)
        {
            currentHealthIntensity -= Time.deltaTime / fadeTime;
            if (currentHealthIntensity <= 0)
            {
                currentHealthIntensity = 0;
            }
        }
        if (currentShieldIntensity > 0)
        {
            currentShieldIntensity -= Time.deltaTime / fadeTime;
            if (currentShieldIntensity <= 0)
            {
                currentShieldIntensity = 0;
            }
        }
    }
}
