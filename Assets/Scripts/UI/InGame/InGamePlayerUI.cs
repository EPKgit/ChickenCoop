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
	private Image attackIcon;
	private Image attackCD;
	private Image ability1Icon;
	private Image ability1CD;
	private Image ability2Icon;
	private Image ability2CD;
	private Image ability3Icon;
	private Image ability3CD;

	void Awake()
	{
		UIActive = transform.Find("UI").gameObject;
		healthSlider = UIActive.transform.Find("Health").Find("Slider").GetComponent<Slider>();
		attackIcon = UIActive.transform.Find("Abilities").Find("Attack").Find("Icon").GetComponent<Image>();
		attackCD = UIActive.transform.Find("Abilities").Find("Attack").Find("CooldownOverlay").GetComponent<Image>();

		ability1Icon = UIActive.transform.Find("Abilities").Find("Ability1").Find("Icon").GetComponent<Image>();
		ability1CD = UIActive.transform.Find("Abilities").Find("Ability1").Find("CooldownOverlay").GetComponent<Image>();
		
		ability2Icon = UIActive.transform.Find("Abilities").Find("Ability2").Find("Icon").GetComponent<Image>();
		ability2CD = UIActive.transform.Find("Abilities").Find("Ability2").Find("CooldownOverlay").GetComponent<Image>();

		ability3Icon = UIActive.transform.Find("Abilities").Find("Ability3").Find("Icon").GetComponent<Image>();
		ability3CD = UIActive.transform.Find("Abilities").Find("Ability3").Find("CooldownOverlay").GetComponent<Image>();
	}

	public void Initialize(GameObject g)
	{
		UIActive.SetActive(true);
		player = g;

		playerHealth = Lib.FindInHierarchy<BaseHealth>(player);
		currentHealth = playerHealth.GetCurrentHealth();
		maxHealth = playerHealth.GetMaxHealth();
		playerHealth.healthValueUpdateEvent += UpdateCachedHealthValues;
		playerHealth.healthChangeEvent += HealthChange;

		playerAbilities = Lib.FindInHierarchy<PlayerAbilities>(player);
		playerAbilities.initializedEvent += OnAbilityInitialized;
	}

	public void SetUIIcons()
	{
		ability1Icon.sprite = playerAbilities.GetIcon(1);
		ability2Icon.sprite = playerAbilities.GetIcon(2);
		ability3Icon.sprite = playerAbilities.GetIcon(3);
		attackIcon.sprite = playerAbilities.GetIcon(0);
	}

	public void OnAbilityInitialized(Ability a1, Ability a2, Ability a3, Ability attack)
	{
		playerAbilities.RegisterAbilityCooldownCallbacks(UpdateAttackUI, UpdateAbility1UI, UpdateAbility2UI, UpdateAbility3UI);
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
		attackCD.fillAmount = currentCD / maxCD;
	}

	void UpdateAbility1UI(float currentCD, float maxCD)
	{
		ability1CD.fillAmount = currentCD / maxCD;
	}

	void UpdateAbility2UI(float currentCD, float maxCD)
	{
		ability2CD.fillAmount = currentCD / maxCD;
	}

	void UpdateAbility3UI(float currentCD, float maxCD)
	{
		ability3CD.fillAmount = currentCD / maxCD;
	}
	
	public void HideSelf()
	{
		UIActive.SetActive(false);
	}
}
