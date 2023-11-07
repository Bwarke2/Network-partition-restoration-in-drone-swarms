using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    private Swarm swarm;
    public Text timeText;
    public Text distText;
    public Text N_msg_Text;
    // Start is called before the first frame update
    void Start()
    {
        swarm = GameObject.FindGameObjectWithTag("Swarm").GetComponent<Swarm>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Node node in swarm.JoinedNodes)
        {
            node.gameObject.GetComponent<Renderer>().material.color = Color.green;
        }

        foreach (GameObject node in swarm.LostNodes)
        {
            node.GetComponent<Renderer>().material.color = Color.black;
        }

        swarm.Leader.gameObject.GetComponent<Renderer>().material.color = Color.red;
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
