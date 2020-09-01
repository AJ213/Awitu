using System;
using TMPro;
using UnityEngine;

public class DifficultlyHandler : MonoBehaviour
{
    [SerializeField] TMP_Text timer = default;
    public static float difficulty = 1;
    [SerializeField] float baseDifficulty = 1;
    [SerializeField] float timeFactor = default;
    void Update()
    {
        UpdateTimer(Time.timeSinceLevelLoad);
    }

    void UpdateTimer(float time)
    {
        string minutes, seconds;
        minutes = Mathf.Floor(time / 60).ToString("00");
        seconds = Mathf.Floor(time % 60).ToString("00");

        timer.text = minutes + ":" + seconds;
        CalculateDifficulty(time);
    }
    [SerializeField] int difficultySpike = 1;
    [SerializeField] int spikeDuration = 600;
    void CalculateDifficulty(float time)
    {
        if(time <= 0)
        {
            difficulty = 0.01f;
        }
        difficultySpike = Mathf.CeilToInt(time / spikeDuration);
        difficulty = baseDifficulty + (difficultySpike * difficultySpike * timeFactor * time/60);
    }
}
