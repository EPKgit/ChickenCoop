using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CastBar : MonoBehaviour
{
    public Slider slider;

    private PlayerAbilities playerAbilities;

    void Start()
    {
        playerAbilities = PlayerInitialization.all[0].gameObject.GetComponent<PlayerAbilities>();
        playerAbilities.OnAbilityCasting += (AbilityEventData aed) => SetEnabled(true);
        playerAbilities.OnAbilityActivated += (AbilityEventData aed) => SetEnabled(false);
        playerAbilities.OnAbilityCastTick += (AbilityEventData aed) => SetProgress(aed.currentCastProgress);
        SetEnabled(false);
    }

    void SetEnabled(bool enabled)
    {
        slider.value = 0;
        slider.gameObject.SetActive(enabled);
    }

    void SetProgress(float percent)
    {
        slider.value = percent;
    }
}
