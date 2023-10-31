using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TargetReachedByNode() {
        //Debug.Log("Target reached");
        Swarm MySwarm = GameObject.FindGameObjectWithTag("Swarm").GetComponent<Swarm>();
        MySwarm.TargetReached(this.gameObject);
        Destroy(this.gameObject);
    }
}
