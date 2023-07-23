using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerRender : MonoBehaviour
{
    [System.Serializable]
    public class PlayerRenderEffectData
    {
        public GameplayTagContainer tagsToCheck;
        public GameObject vfx;
        public UnityEvent onApplied;
        public UnityEvent onRemoved;
    }

    public PlayerRenderEffectData[] effectData;

    public float highlightRotationSpeed = 10.0f;

    private PlayerInput playerInput;
    private GameplayTagComponent tagComponent;
	private SpriteRenderer sprite;
    private Material highlightMaterial;
    private float rotationDegrees;

    private bool registeredCallbacks = false;

    IEnumerator Start()
	{
		sprite = transform.Find("Render").GetComponent<SpriteRenderer>();
		playerInput = GetComponent<PlayerInput>();
        tagComponent = GetComponent<GameplayTagComponent>();
        enabled = false;
		while(playerInput.playerID == -1)
		{
            yield return null;
        }
        enabled = true;
        highlightMaterial = sprite.transform.Find("Highlight").gameObject.GetComponent<SpriteRenderer>().material;
        Color c = Lib.GetPlayerColorByIndex(playerInput.playerID);
        c.a = highlightMaterial.GetColor("_Color").a;
        highlightMaterial.SetColor("_Color", c);
        rotationDegrees = 0;
        if(!registeredCallbacks)
        {
            OnEnable();
        }
    }

    void OnEnable()
    {
        if(tagComponent == null)
        {
            registeredCallbacks = false;
            return;
        }
        registeredCallbacks = true;
        tagComponent.tags.OnPreTagAdded += OnTagAdded;
        tagComponent.tags.OnPostTagRemoved += OnTagRemoved;
    }

    void OnDisable()
    {
        if(!registeredCallbacks)
        {
            return;
        }
        tagComponent.tags.OnPreTagAdded -= OnTagAdded;
        tagComponent.tags.OnPostTagRemoved -= OnTagRemoved;
    }

    void OnTagAdded(GameplayTagInternals.GameplayTagWrapper tag)
    {
        OnTagChanged(tag, true);
    }

    void OnTagRemoved(GameplayTagInternals.GameplayTagWrapper tag)
    {
        OnTagChanged(tag, false);
    }

    void OnTagChanged(GameplayTagInternals.GameplayTagWrapper tag, bool added)
    {
        foreach(var pred in effectData)
        {
            //if the added tag has our tag but the container doesn't
            //then we can call our routine, otherwise we already had it
            if(pred.tagsToCheck.Contains(tag))
            {
                if(!tagComponent.tags.Contains(tag))
                {
                    if(added)
                    {
                        pred.onApplied.Invoke();
                        if(pred.vfx != null)
                        {
                            PoolManager.instance.RequestObject(pred.vfx).transform.position = transform.position;
                        }
                    }
                    else
                    {
                        pred.onRemoved.Invoke();
                    }
                    break;
                }
            }
        }
    }

	void Update()
	{
        rotationDegrees += (Time.deltaTime * highlightRotationSpeed) % 360;
        highlightMaterial.SetFloat("_Rotation", rotationDegrees);
    }

	public void ReplaceModel(GameObject g)
	{
		Transform temp;
		if( (temp = transform.Find("Render").Find("Model")) != null)
		{
            DebugFlags.Log(DebugFlags.Flags.RENDER, "killing old model");
			Destroy(temp.gameObject);
		}
		GameObject newModel = Instantiate(g);
		newModel.transform.position = Vector3.zero;
		newModel.transform.SetParent(transform.Find("Render"), false);
	}

    public void OnApplyInvisibility()
    {
        sprite.color = sprite.color - new Color(0, 0, 0, 0.5f);
    }

    public void OnRemoveInvisibility()
    {
        sprite.color = sprite.color + new Color(0, 0, 0, 0.5f);
    }
}
