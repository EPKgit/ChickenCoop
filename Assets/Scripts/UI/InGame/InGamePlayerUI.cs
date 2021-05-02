using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGamePlayerUI : MonoBehaviour
{
	private float maxHealth;
	private float currentHealth;

	private GameObject player;
	private BaseHealth playerHealth;
	private PlayerAbilities playerAbilities;

	private GameObject UIActive;
	private Slider healthSlider;
	private Image[] icons = new Image[(int)AbilitySlots.MAX];
	private Image[] cds = new Image[(int)AbilitySlots.MAX];

	void Awake()
	{
		UIActive = transform.Find("UI").gameObject;
		healthSlider = UIActive.transform.Find("Health").Find("Slider").GetComponent<Slider>();
        string[] arr = new string[] { "Attack", "Ability1", "Ability2", "Ability3" };
        for (int x = 0; x < arr.Length; ++x)
        {
            icons[x] = UIActive.transform.Find("Abilities").Find(arr[x]).Find("Icon").GetComponent<Image>();
            cds[x] = UIActive.transform.Find("Abilities").Find(arr[x]).Find("CooldownOverlay").GetComponent<Image>();
        }
	}

	public void Initialize(GameObject g)
	{
		UIActive.SetActive(true);
		player = g;

		playerHealth = Lib.FindInHierarchy<BaseHealth>(player);
		currentHealth = playerHealth.currentHealth;
		maxHealth = playerHealth.maxHealth;
		playerHealth.healthValueUpdateEvent += UpdateCachedHealthValues;
		playerHealth.healthChangeEvent += HealthChange;

		playerAbilities = Lib.FindInHierarchy<PlayerAbilities>(player);
        playerAbilities.initializedEvent += OnAbilityInitialized;
        if(playerAbilities.IsInitialized())
        {
            SetUIIcons();
            OnAbilityInitialized(playerAbilities.abilities);
        }

        UpdateHealthUI();
    }

	public void SetUIIcons()
	{
        for(int x = 0; x < icons.Length; ++x)
        {
            icons[x].sprite = playerAbilities.GetIcon(x);
        }
	}

	public void OnAbilityInitialized(AbilitySetContainer abilities)
	{
		playerAbilities.RegisterAbilityCooldownCallbacks(new CooldownTickDelegate[]{ UpdateAttackUI, UpdateAbility1UI, UpdateAbility2UI, UpdateAbility3UI });
		SetUIIcons();
	}

	public void UpdateCachedHealthValues(float newCurrent, float newMax)
	{
		currentHealth = newCurrent;
		maxHealth = newMax;
		UpdateHealthUI();
	}

	public void HealthChange(HealthChangeNotificationData hcnd)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.INGAMEUI, "UI has registered a heal/damage event for " + hcnd.value);
		currentHealth += hcnd.value;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
		UpdateHealthUI();
	}

	void UpdateHealthUI()
	{
		healthSlider.value = currentHealth / maxHealth;
	}

	void UpdateAttackUI(float currentCD, float maxCD)
	{
        UpdateAbilityUI(AbilitySlots.SLOT_ATTACK, currentCD, maxCD);
    }

    void UpdateAbility1UI(float currentCD, float maxCD)
	{
        UpdateAbilityUI(AbilitySlots.SLOT_1, currentCD, maxCD);
    }

    void UpdateAbility2UI(float currentCD, float maxCD)
	{
        UpdateAbilityUI(AbilitySlots.SLOT_2, currentCD, maxCD);
    }

	void UpdateAbility3UI(float currentCD, float maxCD)
	{
        UpdateAbilityUI(AbilitySlots.SLOT_3, currentCD, maxCD);
    }

    private void UpdateAbilityUI(AbilitySlots index, float currentCD, float maxCD)
    {
        UpdateAbilityUI((int)index, currentCD, maxCD);
    }

    private void UpdateAbilityUI(int index, float currentCD, float maxCD)
    {
        if (maxCD == 0)
        {
            maxCD = 1;
            currentCD = 0;
        }
        cds[index].fillAmount = currentCD / maxCD;
    }
	
	public void HideSelf()
	{
		UIActive.SetActive(false);
	}
}
