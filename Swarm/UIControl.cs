using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIControl : MonoBehaviour
{
    private Swarm swarm;
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
}
