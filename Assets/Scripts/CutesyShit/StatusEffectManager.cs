using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum StatusEffectType
{
    STUN,
    SILENCE,
    ROOT,
    BLIND,
    SLOW,
    FEAR,
    MAX,
}

public class StatusEffectManager : MonoSingleton<StatusEffectManager>
{
    private class StatusBarData
    {
        public GameObject barObject;
        public Vector3 offset;
        public List<AppliedStatusData> appliedStatuses;
    }
    private class AppliedStatusData
    {
        public StatusEffectType type;
        public float duration;
        public StatusEffectPopup effectPopup;
        public uint handleID;
    }

    public GameObject effectBarPrefab;
    public GameObject effectPopupPrefab;
    public Sprite[] effectSprites;

    private PoolLoanToken barToken;
    private PoolLoanToken popupToken;
    private GameObject UIRoot;
    private Dictionary<GameObject, StatusBarData> appliedStatuses;
    private const int MAX_NUMBER_EFFECTS_DISPLAYED = 4;
    protected override void Awake()
    {
        base.Awake();
        barToken = PoolManager.instance.RequestLoan(effectBarPrefab, 40, true);
        popupToken = PoolManager.instance.RequestLoan(effectPopupPrefab, 20, true);
        UIRoot = GameObject.Find("ScreenToWorldCanvas");
        PoolManager.instance.ChangeParentOfPool(effectPopupPrefab, UIRoot.transform);
        PoolManager.instance.ChangeParentOfPool(effectBarPrefab, UIRoot.transform);
        appliedStatuses = new Dictionary<GameObject, StatusBarData>();

        if(effectSprites.Length != (int)StatusEffectType.MAX)
        {
            Debug.Log("ERROR: STATUS EFFECT ICONS NOT SET");
            throw new System.Exception();
        }
    }

    void Update()
    {
        for (int x = appliedStatuses.Count - 1; x >= 0; --x)
        {
            KeyValuePair<GameObject, StatusBarData> pair = appliedStatuses.ElementAt(x);
            GameObject applied = pair.Key;
            StatusBarData barData = pair.Value;
            barData.barObject.transform.position = Camera.main.WorldToScreenPoint(applied.transform.position + barData.offset);
            for (int y = barData.appliedStatuses.Count - 1; y >= 0; --y)
            {
                AppliedStatusData appliedData = barData.appliedStatuses[y];
                if (appliedData.effectPopup.EffectUpdate(Time.deltaTime))
                {
                    RemoveEffectInternal(applied, barData.appliedStatuses[y]);
                    barData.appliedStatuses.RemoveAt(y);
                }
            }
            if(barData.appliedStatuses.Count == 0)
            {
                barData.barObject.GetComponent<Poolable>().DestroySelf();
                appliedStatuses.Remove(applied);
            }
        }
    }

    public void ApplyEffect(GameObject toApply, StatusEffectType type, float duration)
    {
        StatusBarData effectData;
        if (appliedStatuses.ContainsKey(toApply))
        {
            effectData = appliedStatuses[toApply];
            AppliedStatusData appliedStatus = FetchEffectFromBar(effectData, type);
            if(appliedStatus != null)
            {
                appliedStatus.duration = Mathf.Max(duration, appliedStatus.duration);
                appliedStatus.effectPopup.SetNewDuration(duration);
            }
            else
            {
                ApplyEffectInternal(toApply, CreatePopupForEffect(effectData, type, duration));
            }
        }
        else
        {
            ApplyEffectInternal(toApply, CreatePopupForEffect(CreateBar(toApply), type, duration));
        }    
    }

    void ApplyEffectInternal(GameObject toApply, AppliedStatusData data)
    {
        GameplayTagFlags flag = GameplayTagFlags.NONE;
        switch(data.type)
        {
            case StatusEffectType.STUN:
                flag = GameplayTagFlags.STUN;
                break;
            case StatusEffectType.ROOT:
                flag = GameplayTagFlags.ROOT;
                break;
            case StatusEffectType.SILENCE:
                flag = GameplayTagFlags.SILENCE;
                break;
            case StatusEffectType.BLIND:
                flag = GameplayTagFlags.BLIND;
                break;
            default:
                Debug.LogError("ERROR: Attempt to apply status that hasn't been implemented yet " + data.type);
                throw new System.Exception();
        }
        if(flag != GameplayTagFlags.NONE)
        {
            var tagComponent = Lib.FindDownwardsInTree<GameplayTagComponent>(toApply);
            if (tagComponent != null)
            {
                data.handleID = tagComponent.tags.AddTag(flag);
            }
        }
    }

    void RemoveEffectInternal(GameObject toApply, AppliedStatusData data)
    {
        switch (data.type)
        {
            case StatusEffectType.STUN:
            case StatusEffectType.ROOT:
            case StatusEffectType.BLIND:
            case StatusEffectType.SILENCE:
            {
                Lib.FindDownwardsInTree<GameplayTagComponent>(toApply)?.tags.RemoveTagWithID(data.handleID);
            } break;
            default:
                Debug.LogError("ERROR: Attempt to remove status that hasn't been implemented yet " + data.type);
                throw new System.Exception();
        }
    }

    AppliedStatusData CreatePopupForEffect(StatusBarData bar, StatusEffectType type, float duration)
    {
        AppliedStatusData data = new AppliedStatusData();
        data.duration = duration;
        data.type = type;
        data.effectPopup = PoolManager.instance.RequestObject(effectPopupPrefab).GetComponent<StatusEffectPopup>();
        data.effectPopup.transform.SetParent(bar.barObject.transform);
        bar.appliedStatuses.Add(data);
        StatusEffectPopup popup = data.effectPopup.GetComponent<StatusEffectPopup>();
        popup.Setup(effectSprites[(int)type], duration);
        return data;
    }

    StatusBarData CreateBar(GameObject toApply)
    {
        StatusBarData barData = new StatusBarData();
        barData.barObject = PoolManager.instance.RequestObject(effectBarPrefab);
        barData.offset = Vector3.up * (Lib.FindDownwardsInTree<SpriteRenderer>(toApply)?.bounds.extents.y ?? 0) * toApply.transform.localScale.y;
        barData.barObject.transform.position = Camera.main.WorldToScreenPoint(toApply.transform.position + barData.offset);
        barData.appliedStatuses = new List<AppliedStatusData>();
        appliedStatuses.Add(toApply, barData);
        return barData;
    }

    AppliedStatusData FetchEffectFromBar(StatusBarData bar, StatusEffectType type)
    {
        foreach(var v in bar.appliedStatuses)
        {
            if(v.type == type)
            {
                return v;
            }
        }
        return null;
    }
}
