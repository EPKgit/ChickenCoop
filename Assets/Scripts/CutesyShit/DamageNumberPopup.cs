using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumberPopup : Poolable
{
    public TextMeshProUGUI tmp;

    public float risingSpeed = 0.8f;
    public float fadeOutTime = 2.0f;
    public AnimationCurve alphaCurve;
    public AnimationCurve scaleCurve;

    private RectTransform rectTransform;

    
    private float currentTime;

    private void Awake()
    {
        if(tmp != null)
        {
            tmp = GetComponent<TextMeshProUGUI>();
        }
        rectTransform = tmp.rectTransform;
    }

    public override void Reset()
    {
        currentTime = 0;
    }

    public void Setup(float dmg, Vector3 position)
    {
        const float COLOR_RANGE = 0.2f;
        tmp.text = string.Format("{0,0:F2}", dmg);
        float t = Mathf.Clamp(dmg, 0, DamageNumbersManager.instance.highEndDamage) / DamageNumbersManager.instance.highEndDamage;
        t = Mathf.Clamp(t, 0, 1 - COLOR_RANGE);
        Color low = DamageNumbersManager.instance.lowDamageColor;
        Color high = DamageNumbersManager.instance.highDamageColor;
        tmp.colorGradient = new VertexGradient(Color.Lerp(low, high, t), Color.Lerp(low, high, t + COLOR_RANGE / 2), Color.Lerp(low, high, t + COLOR_RANGE / 2), Color.Lerp(low, high, t + COLOR_RANGE)); //3 = bottom right
        rectTransform.localPosition = Vector3.zero;
        transform.position = position;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime > fadeOutTime)
        {
            DestroySelf();
        }
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, alphaCurve.Evaluate(currentTime / fadeOutTime));
        rectTransform.localScale = Vector3.one * scaleCurve.Evaluate(currentTime / fadeOutTime);
        transform.position += Vector3.up * Time.deltaTime * risingSpeed;
    }
}
