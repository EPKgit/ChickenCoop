using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DamageFeedbackOverlay : MonoBehaviour
{
    private Image image;
    private Material overlayMaterial;
    private int intensityPropertyID;
    private Vector4 currentIntensity
    {
        get => _currentIntensity;
        set
        {
            _currentIntensity = value;
            intensityDirty = true;
        }
    }
    private Vector4 _currentIntensity;
    private bool intensityDirty = false;
    private List<PlayerInitialization> subscribedDamageProducers;
    void Awake()
    {
        image = GetComponent<Image>();
        overlayMaterial = image.material;
        intensityPropertyID = Shader.PropertyToID("_MinMaxIntensity");
        currentIntensity = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        overlayMaterial.SetVector(intensityPropertyID, currentIntensity);
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
                hp.postDamageEvent -= OnDamageTaken;
                subscribedDamageProducers.Remove(player);
            }
            hp.postDamageEvent += OnDamageTaken;
            subscribedDamageProducers.Add(player);
        }
    }

    void OnDamageTaken(HealthChangeNotificationData hcnd)
    {
        intensityDirty = true;
        currentIntensity = new Vector4(0.4f, 0.4f, 0, 0);
    }

    void Update()
    {
        if(intensityDirty)
        {
            overlayMaterial.SetVector(intensityPropertyID, currentIntensity);
            intensityDirty = false;
        }
        if(currentIntensity.magnitude > 0)
        {
            currentIntensity = new Vector4(currentIntensity.x - Time.deltaTime, currentIntensity.y - Time.deltaTime, 0, 0);
            if (currentIntensity.x < 0)
            {
                currentIntensity = new Vector4(0, currentIntensity.y, 0, 0);
            }
            if (currentIntensity.y < 0)
            {
                currentIntensity = new Vector4(currentIntensity.x, 0, 0, 0);
            }
        }
    }
}
