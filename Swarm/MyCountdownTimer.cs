using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MyCountdownTimer : MonoBehaviour
{
    public float timeLeft = 0;
    public bool timerActive = false;
    
    void Update()
    {
        if (timerActive)
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                StopTimer();
            }
    }

    public float GetTimer()
    {
        return timeLeft;
    }

    public void StartTimer(float time)
    {
        timerActive = true;
        timeLeft = time;
    }

    public void StopTimer()
    {
        timerActive = false;
    }   

    private void OnTimerEndEvent()
    {
        Debug.Log("Timer ended");
    }
    
}