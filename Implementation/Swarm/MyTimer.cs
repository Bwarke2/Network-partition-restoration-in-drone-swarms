using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTimer : MonoBehaviour
{
    public float timeRun = 0;
    public bool timerActive = false;
    
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

    
}