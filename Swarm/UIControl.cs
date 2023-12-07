using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    private Swarm _swarm;
    public Text timeText;
    public Text distText;
    public Text N_msg_Text;

    private int _targetFPS = 30;
    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _targetFPS;
        _swarm = GameObject.FindGameObjectWithTag("Swarm").GetComponent<Swarm>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Node node in _swarm.JoinedNodes)
        {
            node.GetComponent<SpriteRenderer>().material.color = Color.blue;
            node.GetComponent<SpriteRenderer>().color = Color.blue;
        }

        foreach (GameObject node in _swarm.LostNodes)
        {
            node.GetComponent<SpriteRenderer>().material.color = Color.black;
            node.GetComponent<SpriteRenderer>().color = Color.black;
        }

        if(_swarm.Leader != null)
        {
            _swarm.Leader.GetComponent<SpriteRenderer>().material.color = Color.yellow;
            _swarm.Leader.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }

    public void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
    }

    public void DisplayDistance(float distance)
    {
        distText.text = string.Format("Distance: {0}", distance);
    }

    public void DisplayNumSentMsgs(int numSentMsgs)
    {
        N_msg_Text.text = string.Format("Messages sent: {0}", numSentMsgs);
    }
}
