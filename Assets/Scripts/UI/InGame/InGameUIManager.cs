using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUIManager : MonoSingleton<InGameUIManager>
{
    public GameObject playerUIPrefab;

    private List<InGamePlayerUI> UIObjects;
    private Transform layoutGroup;

    private const int MAX_PLAYERS = 4;

    protected override void Awake()
    {
        base.Awake();
        UIObjects = new List<InGamePlayerUI>();
        layoutGroup = GameObject.Find("InGameCanvas").transform.Find("PlayerUI");
        for (int x = 0; x < MAX_PLAYERS; ++x)
        {
            UIObjects.Add(Instantiate(playerUIPrefab, Vector3.zero, Quaternion.identity, layoutGroup).GetComponent<InGamePlayerUI>());
        }
    }

    void Start()
    {
        PlayerInitialization.OnPlayerNumberChanged += UpdateUI;
        UpdateUI();
    }

    private void OnDisable()
    {
        PlayerInitialization.OnPlayerNumberChanged -= UpdateUI;
    }

    public void UpdateUI()
    {
        int x;
        for (x = 0; x < PlayerInitialization.all.Count; ++x)
        {
            UIObjects[x].Initialize(PlayerInitialization.all[x].gameObject);
        }
        for (; x < MAX_PLAYERS; ++x)
        {
            UIObjects[x].HideSelf();
        }
    }

    public void RevokeCallbacks()
    {
        foreach (InGamePlayerUI i in UIObjects)
        {
            i.RevokeCallbacks();
        }
    }

    public void ReinitializeUI()
    {
        foreach(InGamePlayerUI i in UIObjects)
        {
            i.Reinitialize();
        }
    }
}
