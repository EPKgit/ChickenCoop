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

    private PoolLoanToken token;
    private GameObject UIRoot;

    //private List<IDamagable> registeredCallbacks;
    //private int numGameObjectsOnLastCache;

    protected override void Awake()
    {
        base.Awake();
        token = PoolManager.instance.RequestLoan(textPrefab, 30, true);
        UIRoot = GameObject.Find("WorldSpaceCanvas");
        PoolManager.instance.ChangeParentOfPool(textPrefab, UIRoot.transform);
        //registeredCallbacks = new List<IDamagable>();
        //numGameObjectsOnLastCache = 0;
    }

    //private void Update()
    //{
    //    var allGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
    //    if(num)
    //}

    public void CreateNumber(float i, Vector3 position)
    {
        var gameObject = PoolManager.instance.RequestObject(textPrefab);
        if(gameObject == null) return;
        DamageNumberPopup popup = gameObject.GetComponent<DamageNumberPopup>();
        if (popup == null) return;
        popup.Setup(i, position);
    }
}
