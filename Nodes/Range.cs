using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Collision detected");
        if (collider.gameObject.tag == "Node")
        {
            Debug.Log("Node is in range");
        }
    }
}
