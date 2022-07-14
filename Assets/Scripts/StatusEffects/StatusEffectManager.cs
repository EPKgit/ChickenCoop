using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Statuses;

public class StatusEffectManager : MonoSingleton<StatusEffectManager>
{
    private class StatusBarData
    {
        public GameObject barObject;
        public Vector3 offset;
        public StatusEffectContainerComponent container;
        public List<StatusEffectPopup> popups;
    }

    public GameObject effectBarPrefab;
    public GameObject effectPopupPrefab;
    public Sprite[] effectSprites;

    private PoolLoanToken barToken;
    private PoolLoanToken popupToken;
    private GameObject UIRoot;
    private Dictionary<GameObject, StatusBarData> activeStatuses;
    private const int MAX_NUMBER_EFFECTS_DISPLAYED = 4;
    protected override void Awake()
    {
        base.Awake();
        barToken = PoolManager.instance.RequestLoan(effectBarPrefab, 40, true);
        popupToken = PoolManager.instance.RequestLoan(effectPopupPrefab, 20, true);
        UIRoot = GameObject.Find("ScreenToWorldCanvas");
        PoolManager.instance.ChangeParentOfPool(effectPopupPrefab, UIRoot.transform);
        PoolManager.instance.ChangeParentOfPool(effectBarPrefab, UIRoot.transform);
        activeStatuses = new Dictionary<GameObject, StatusBarData>();

        if(effectSprites.Length != (int)StatusEffectType.MAX)
        {
            Debug.Log("ERROR: STATUS EFFECT ICONS NOT SET");
            throw new System.Exception();
        }
    }

    void Update()
    {
        for (int x = activeStatuses.Count - 1; x >= 0; --x)
        {
            KeyValuePair<GameObject, StatusBarData> pair = activeStatuses.ElementAt(x);
            GameObject applied = pair.Key;
            StatusBarData barData = pair.Value;
            barData.barObject.transform.position = Camera.main.WorldToScreenPoint(applied.transform.position + barData.offset);
            for (int y = barData.popups.Count - 1; y >= 0; --y)
            {
                if (barData.popups[y].EffectUpdate(Time.deltaTime))
                {
                    barData.popups.RemoveAt(y);
                }
            }
            if(barData.popups.Count == 0)
            {
                barData.barObject.GetComponent<Poolable>().DestroySelf();
                activeStatuses.Remove(applied);
            }
        }
    }

    public void ApplyEffect(GameObject toApply, StatusEffectType type, float duration)
    {
        StatusBarData statusData;
        if (activeStatuses.ContainsKey(toApply)) //if we already have a bar for the effect
        {
            statusData = activeStatuses[toApply];
            ApplyEffectAndCreatePopup(statusData, type, duration);
        }
        else //if we don't have a bar
        {
            ApplyEffectAndCreatePopup(CreateBar(toApply), type, duration);
        }
    }

    void ApplyEffectAndCreatePopup(StatusBarData bar, StatusEffectType type, float duration)
    {
        bool newStatusCreated;
        var effect = StatusEffectBase.GetStatusObjectByType(type, duration);
        bar.container.AddStatus(effect, out newStatusCreated);
        if (newStatusCreated)
        {
            var popup = PoolManager.instance.RequestObject(effectPopupPrefab).GetComponent<StatusEffectPopup>();
            popup.gameObject.transform.SetParent(bar.barObject.transform);
            popup.Setup(effectSprites[(int)type], effect);
            bar.popups.Add(popup);
        }
    }

    StatusBarData CreateBar(GameObject toApply)
    {
        StatusEffectContainerComponent container = Lib.FindDownThenUpwardsInTree<StatusEffectContainerComponent>(toApply);
        if (container == null)
        {
            GameplayTagComponent tagComponent = Lib.FindDownThenUpwardsInTree<GameplayTagComponent>(toApply);
            if (tagComponent == null)
            {
                throw new System.Exception("ERROR: Status effect appllied to object without tag component");
            }
            container = tagComponent.gameObject.AddComponent<StatusEffectContainerComponent>();
        }
        StatusBarData barData = new StatusBarData();
        barData.barObject = PoolManager.instance.RequestObject(effectBarPrefab);
        barData.offset = Vector3.up * (Lib.FindDownwardsInTree<SpriteRenderer>(toApply)?.bounds.extents.y ?? 0) * toApply.transform.localScale.y;
        barData.barObject.transform.position = Camera.main.WorldToScreenPoint(toApply.transform.position + barData.offset);
        barData.popups = new List<StatusEffectPopup>();
        barData.container = container;
        activeStatuses.Add(toApply, barData);
        return barData;
    }
}
