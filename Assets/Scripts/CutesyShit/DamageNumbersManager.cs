using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DamageNumbersManager : MonoSingleton<DamageNumbersManager>
{
    public GameObject textPrefab;
    public Color lowDamageColor;
    public Color highDamageColor;
    public float highEndDamage = 10;
    public float sizeModifier = 0.75f;

    private PoolLoanToken token;
    private GameObject UIRoot;

    protected override void OnCreation()
    {
        base.OnCreation();
        token = PoolManager.instance.RequestLoan(textPrefab, 30, true);
        UIRoot = GameObject.Find("ScreenToWorldCanvas");
        PoolManager.instance.ChangeParentOfPool(textPrefab, UIRoot.transform);
    }

    public void CreateNumber(float i, Vector3 position, float LRWiggleAmount = 0)
    {
        var gameObject = PoolManager.instance.RequestObject(textPrefab);
        if (gameObject == null)
        {
            return;
        }
        DamageNumberPopup popup = gameObject.GetComponent<DamageNumberPopup>();
        if (popup == null)
        {
            return;
        }
        float rand = Random.Range(0, LRWiggleAmount) * 2 - LRWiggleAmount;
        popup.Setup(i, position + Vector3.left * rand);
    }
}
