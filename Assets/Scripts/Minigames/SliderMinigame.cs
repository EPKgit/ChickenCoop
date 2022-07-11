// #define TESTING_MINIGAMES

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Minigame;

public class SliderMinigame : MinigameBase
{
    public override MinigameType type => MinigameType.SLIDER;
    protected override MinigameInputType inputType => MinigameInputType.INTERACT;

    public Sprite progressObjectImage;
    public Slider timerSlider;
    public HorizontalLayoutGroup progressBar;
    public RectTransform handleRect;
    public RectTransform fillRect;

    private float timer;
    private float lowerRange = float.MinValue;
    private float upperRange;
    private int currentSuccesses;
    private int requiredSuccesses;
    private List<Image> progressImages;

#region TUNING_VARIABLES
    private const float MIN_NEW_ZONE_DISTANCE = 0.2f;
    private const float SPEED = 4.0f;
    private const float STARTING_ZONE_SIZE = 0.2f;
    private const int REQUIRED_SUCCESSES = 10;
    private const int DIFFICULTY_MODIFIER_SUCCESS_REDUCTION = 1;
    private const float DIFFICULTY_MODIFIER_ZONE_SIZE_INCREASE = 0.05f;
    private const float MAX_TIME = 10.0f;
    #endregion

#if TESTING_MINIGAMES
    void Awake()
    {
        StartMinigame(new MinigameData()
        {
            triggeringPlayer = PlayerInitialization.all[0].GetComponent<PlayerInput>(),
            difficultyModifier = 2,
        });
    }
#endif

    protected override void StartMinigameInternal()
    {
        timer = 0;
        handleRect.anchorMin = new Vector2(0.0f, handleRect.anchorMin.y);
        handleRect.anchorMax = new Vector2(0.0f, handleRect.anchorMax.y);
        progressImages = new List<Image>();
        requiredSuccesses = REQUIRED_SUCCESSES - data.difficultyModifier * DIFFICULTY_MODIFIER_SUCCESS_REDUCTION;
        for (int x = 0; x < requiredSuccesses; ++x)
        {
            GameObject g = new GameObject();
            g.transform.SetParent(progressBar.transform);
            Image i = g.AddComponent<Image>();
            progressImages.Add(i);
            i.sprite = progressObjectImage;
            i.color = Color.black;
            i.preserveAspect = true;
        }
        NewZone();
    }

    private void NewZone()
    {
#if TESTING_MINIGAMES
        if (currentSuccesses >= requiredSuccesses)
        {
            return;
        }
#endif
        float oldLower = lowerRange;
        int range1 = 100 - (int)(STARTING_ZONE_SIZE * 100) - (int)(data.difficultyModifier * DIFFICULTY_MODIFIER_ZONE_SIZE_INCREASE * 100);
        do
        {
            lowerRange = Random.Range(0, range1) / 100.0f;
        } while (Mathf.Abs(lowerRange - oldLower) < MIN_NEW_ZONE_DISTANCE);
        upperRange = lowerRange + STARTING_ZONE_SIZE + (data.difficultyModifier * DIFFICULTY_MODIFIER_ZONE_SIZE_INCREASE);
        fillRect.anchorMin = new Vector2(lowerRange, 0.0f);
        fillRect.anchorMax = new Vector2(upperRange, 1.0f);
    }
    void Update()
    {
        timer += Time.deltaTime;
        float xVal = (Mathf.Sin(timer * SPEED) + 1.0f) * 0.5f;
        handleRect.anchorMin = new Vector2(xVal, handleRect.anchorMin.y);
        handleRect.anchorMax = new Vector2(xVal, handleRect.anchorMax.y);
        float t = 1.0f - (timer / MAX_TIME);
#if TESTING_MINIGAMES
        if (t <= 0)
        {
            this.FinishMinigame();
            Destroy(gameObject);
            Destroy(this);
        }
#endif
        timerSlider.value = t;
    }

    protected override void HandleMinigameInteractInput(bool pressed)
    {
        if(!pressed)
        {
            return;
        }
        float handlePos = handleRect.anchorMin.x;
        HandleScore(handlePos >= lowerRange && handlePos <= upperRange);
        NewZone();
    }

    private void HandleScore(bool success)
    {
        currentSuccesses += success ? 1 : 0;
        for (int x = 0; x < currentSuccesses && x < progressImages.Count; ++x)
        {
            progressImages[x].color = Color.white;
        }
#if TESTING_MINIGAMES
        if(currentSuccesses >= requiredSuccesses)
        {
            this.FinishMinigame();
            Destroy(gameObject);
            Destroy(this);
        }
#endif
    }
    public override MinigameStatus GetMinigameState()
    {
        if(currentSuccesses >= requiredSuccesses)
        {
            return MinigameStatus.FINISHED_SUCCESS;
        }
        return timer >= MAX_TIME ? MinigameStatus.FINISHED_FAILURE : MinigameStatus.IN_PROGRESS;
    }
}
