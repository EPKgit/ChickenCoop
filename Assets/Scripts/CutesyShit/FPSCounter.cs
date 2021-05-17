using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    private Text text;
    private const int NUM_FRAMES_AVG = 20;
    private float[] frameTimes;
    private int frameIndex;

    private void Awake()
    {
        text = GetComponent<Text>();
        frameTimes = new float[NUM_FRAMES_AVG];
    }

    private void Update()
    {
        frameTimes[frameIndex++ % NUM_FRAMES_AVG] = Time.deltaTime;
        float cumulative = 0;
        Array.ForEach(frameTimes, (float f) => { cumulative += f; });
        text.text = string.Format("CUR:{0,3:F2}\nAVG:{1,3:F2}", 1.0f / Time.deltaTime, (float)NUM_FRAMES_AVG / cumulative);
    }
}
