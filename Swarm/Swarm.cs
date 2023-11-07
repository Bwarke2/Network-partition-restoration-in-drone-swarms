using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PartitionPolicy
{
    PRP1,
    PRP2,
    PRP3
}

public class Swarm : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> swarm = new List<GameObject>();
    public MyTimer timer;
    public List<Node> JoinedNodes = new List<Node>();
    public List<GameObject> LostNodes = new List<GameObject>();
    public List<GameObject> RemainingTargets = new List<GameObject>();

    public Node Leader;

    public PartitionPolicy CurrentPartitionPolicy = PartitionPolicy.PRP1;

    void Awake()
    {
        swarm.AddRange(GameObject.FindGameObjectsWithTag("Node"));
        foreach (GameObject node in swarm)
        {
            //Debug.Log("Adding node to swarm");
            JoinedNodes.Add(node.GetComponent<Node>());
        }
        timer = this.GetComponent<MyTimer>();
        RemainingTargets.AddRange(GameObject.FindGameObjectsWithTag("Target"));
    }
    void Start()
    {
        timer.StartTimer();
    }

    // Update is called once per frame
    void Update()
    {
        timer.DisplayTime(timer.GetTimer());
    }

    public void TargetReached(GameObject target)
    {
        RemainingTargets.Remove(target);
        if (RemainingTargets.Count == 0)
        {
            timer.StopTimer();
            Debug.Log("All targets reached");
        }
    }

    public void AddMember(Node node)
    {
        if (JoinedNodes.Contains(node) == false)
            JoinedNodes.Add(node);
        if (LostNodes.Contains(node.gameObject) == true)
            LostNodes.Remove(node.gameObject);
    }

    public void RemoveMember(Node node)
    {
        //Remove node from member list
        if (JoinedNodes.Contains(node) == true)
            JoinedNodes.Remove(node);
        if (LostNodes.Contains(node.gameObject) == false)
            LostNodes.Add(node.gameObject);
    }

    public List<Node> GetMembers()
    {
        return JoinedNodes;
    }

    public List<GameObject> GetTargets()
    {
        return RemainingTargets;
    }
}
