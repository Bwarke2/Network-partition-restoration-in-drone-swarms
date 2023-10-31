using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MyTimer : MonoBehaviour
{
    public float timeRun = 0;
    public bool timerActive = false;
    public Text timeText;
    void Update()
    {
        if (timerActive)
            timeRun += Time.deltaTime;
    }

    public float GetTimer()
    {
        return timeRun;
    }

    public void StartTimer()
    {
        timerActive = true;
    }

    public void StopTimer()
    {
        timerActive = false;
    }   

    public void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
    }
}