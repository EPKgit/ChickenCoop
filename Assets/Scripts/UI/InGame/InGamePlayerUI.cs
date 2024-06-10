using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGamePlayerUI : MonoBehaviour
{
	private float maxHealth;
	private float currentHealth;

	private GameObject player;
	private BaseHealth playerHealth;
	private PlayerAbilities playerAbilities;

	private GameObject UIActive;
	private Slider healthSlider;
	private Image[] icons = new Image[(int)AbilitySlot.MAX];
	private Image[] cds = new Image[(int)AbilitySlot.MAX];
    private TextMeshProUGUI[] cdTexts = new TextMeshProUGUI[(int)AbilitySlot.MAX];

    void Awake()
	{
		UIActive = transform.Find("UI").gameObject;
		healthSlider = UIActive.transform.Find("Health").Find("Slider").GetComponent<Slider>();
        string[] arr = new string[] { "Attack", "Ability1", "Ability2", "Ability3" };
        for (int x = 0; x < arr.Length; ++x)
        {
            icons[x] = UIActive.transform.Find("Abilities").Find("Square").Find(arr[x]).Find("Icon").GetComponent<Image>();
            cds[x] = UIActive.transform.Find("Abilities").Find("Square").Find(arr[x]).Find("CooldownOverlay").GetComponent<Image>();
            cdTexts[x] = UIActive.transform.Find("Abilities").Find("Square").Find(arr[x]).Find("Timer").GetComponent<TextMeshProUGUI>();
        }
	}

	public void Initialize(GameObject g)
	{
		UIActive.SetActive(true);
		player = g;

		playerHealth = Lib.FindUpwardsInTree<BaseHealth>(player);
		currentHealth = playerHealth.currentHealth;
		maxHealth = playerHealth.maxHealth;
		playerHealth.healthValueUpdateEvent += UpdateCachedHealthValues;
		playerHealth.healthChangeEvent += HealthChange;

		playerAbilities = Lib.FindUpwardsInTree<PlayerAbilities>(player);
        playerAbilities.initializedEvent += OnAbilityInitialized;
        if(playerAbilities.IsInitialized())
        {
            SetUIIcons();
            OnAbilityInitialized(playerAbilities.Abilities);
        }

        UpdateHealthUI();
    }

    public void RevokeCallbacks()
    {
        if (player == null)
        {
            return;
        }
        playerHealth.healthValueUpdateEvent -= UpdateCachedHealthValues;
        playerHealth.healthChangeEvent -= HealthChange;
        playerAbilities.initializedEvent -= OnAbilityInitialized;
        playerAbilities.UnregisterAbilityCooldownCallbacks(new CooldownTickDelegate[] { UpdateAttackUI, UpdateAbility1UI, UpdateAbility2UI, UpdateAbility3UI });
    }

    public void Reinitialize()
    {
        if(player == null)
        {
            return;
        }
        RevokeCallbacks();
        Initialize(player);
    }

	public void SetUIIcons()
	{
        for(int x = 0; x < icons.Length; ++x)
        {
            var icon = playerAbilities.GetIcon(x);
            if (icon != null)
            {
                icons[x].sprite = icon;
                icons[x].enabled = true;
            }
            else
            {
                icons[x].enabled = false;
            }
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

	public void HealthChange(HealthChangeData hcd)
	{
		DebugFlags.Log(DebugFlags.Flags.INGAMEUI, "UI has registered a heal/damage event for " + hcd.Delta);
		currentHealth += hcd.Delta;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
		UpdateHealthUI();
	}

	void UpdateHealthUI()
	{
        if(maxHealth <= 0)
        {
            healthSlider.value = 0;
            return;
        }
		healthSlider.value = currentHealth / maxHealth;
	}

	void UpdateAttackUI(CooldownTickData data)
	{
        UpdateAbilityUI(AbilitySlot.SLOT_ATTACK, data);
    }

    void UpdateAbility1UI(CooldownTickData data)
	{
        UpdateAbilityUI(AbilitySlot.SLOT_1, data);
    }

    void UpdateAbility2UI(CooldownTickData data)
	{
        UpdateAbilityUI(AbilitySlot.SLOT_2, data);
    }

	void UpdateAbility3UI(CooldownTickData data)
	{
        UpdateAbilityUI(AbilitySlot.SLOT_3, data);
    }

    private void UpdateAbilityUI(AbilitySlot index, CooldownTickData data)
    {
        UpdateAbilityUI((int)index, data);
    }

    private void UpdateAbilityUI(int index, CooldownTickData data)
    {
        float t = 0;
        float text = 0;
        if (data.maxCooldown == 0)
        {
            t = 0;
        }
        else if(data.currentRecast > 0)
        {
            t = data.currentRecast / data.maxRecast;
            cds[index].color = new Color(1.0f, 0, 0, cds[index].color.a);
            text = data.currentRecast;
        }
        else
        {
            t = data.currentCooldown / data.maxCooldown;
            cds[index].color = new Color(0, 0, 0, cds[index].color.a);
            text = data.currentCooldown;
        }
        t = Mathf.Clamp(t, 0, 1);
        cds[index].fillAmount = t;
        cdTexts[index].text = text <= 0 ? "" : string.Format("{0,0:F1}", text);
    }
	
	public void HideSelf()
	{
        if (UIActive != null)
        {
            UIActive.SetActive(false);
        }
    }
}
