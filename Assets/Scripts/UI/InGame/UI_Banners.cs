using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum BannerPriority
{
    NORMAL_PRIORITY = 0,
    HIGH_PRIORITY = 1,
    MAX,
}
public class BannerData : IComparable<BannerData>
{
    public string text;
    public float duration;
    public BannerPriority priority;
    public BannerData(string t, float d, BannerPriority p = BannerPriority.NORMAL_PRIORITY)
    {
        text = t;
        duration = d;
        priority = p;
    }

    public int CompareTo(BannerData other)
    {
        if (other == null)
        {
            return 1;
        }
        return priority.CompareTo(other.priority);
    }
}
public class UI_Banners : MonoSingleton<UI_Banners>
{
    public TMP_Text textComponent;
    public Image image;

    public AnimationClip slideOutClip;
    public AnimationClip slideInClip;

    private new Animation animation;
    private CanvasGroup canvasGroup;
    private Mesh textMesh;

    private PriorityQueue<BannerData> dataQueue;
    private BannerData activeBanner;
    private float bannerTimer;

    protected override void OnCreation()
    {
        base.OnCreation();
        animation = GetComponent<Animation>();
        dataQueue = new PriorityQueue<BannerData>(1);
        canvasGroup = GetComponent<CanvasGroup>();
        textMesh = textComponent.mesh;
    }

    void Update()
    {
        if(activeBanner != null)
        {
            bannerTimer -= Time.deltaTime;
            if(bannerTimer <= 0)
            {
                if(dataQueue.IsEmpty)
                {
                    activeBanner = null;
                    StartCoroutine(HideBanner());
                }
                else
                {
                    StartCoroutine(NextBanner());
                }
            }
        }
        //DoAnimation();
    }

    void DoAnimation()
    {
        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;
        for (int x = 0; x < textInfo.characterCount; ++x)
        {
            TMP_CharacterInfo charInfo = textComponent.textInfo.characterInfo[x];

            if (!charInfo.isVisible)
            {
                continue;
            }
            var verts = textComponent.textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            for (int y = 0; y < 4; ++y)
            {
                var orig = verts[charInfo.vertexIndex + y];
                verts[charInfo.vertexIndex + y] = orig + Vector3.up * (x % 2 == (((int)Time.time) % 2) ? 10 : 0);
            }
        }

        for (int x = 0; x < textInfo.meshInfo.Length; ++x)
        {
            var meshInfo = textInfo.meshInfo[x];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textComponent.UpdateGeometry(meshInfo.mesh, x);
        }

        textComponent.canvasRenderer.SetMesh(textMesh);
    }

    private IEnumerator HideBanner()
    {
        while(canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
        textComponent.text = "";
    }

    private IEnumerator ShowBanner()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    private IEnumerator NextBanner()
    {
        activeBanner = null;
        animation.clip = slideOutClip;
        animation.Play();
        while (animation.isPlaying)
        {
            yield return null;
        }
        SetActiveBanner(dataQueue.Pop());
        animation.clip = slideInClip;
        animation.Play();
        while (animation.isPlaying)
        {
            yield return null;
        }
        animation.clip = null;
    }
    
    private void SetActiveBanner(BannerData b)
    {
        activeBanner = b;
        textComponent.text = b.text;
        bannerTimer = b.duration;
    }

    public void PlayBanner(BannerData b)
    {
        if(activeBanner == null)
        {
            SetActiveBanner(b);
            StartCoroutine(ShowBanner());
        }
        else
        {
            dataQueue.Push(b);
        }
    }

    public void PlayBanner(string text, float d = 2, BannerPriority p = BannerPriority.NORMAL_PRIORITY)
    {
        PlayBanner(new BannerData(text, d, p));
    }
}
