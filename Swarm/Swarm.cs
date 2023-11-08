using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;

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

    private bool _simmulationRunning = false;

    //Metrics
    public float TotalDistance = 0;
    public int NumSentMsgs = 0;

    public Node Leader;

    public PartitionPolicy CurrentPartitionPolicy = PartitionPolicy.PRP1;

    //UI
    public UIControl UIControl = null;

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
        UIControl = this.GetComponent<UIControl>();
    }
    void Start()
    {
        timer.StartTimer();
        _simmulationRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(_simmulationRunning)
        {
            UIControl.DisplayTime(timer.GetTimer());
            TotalDistance = CalTotalDistance();
            UIControl.DisplayDistance(TotalDistance);
            NumSentMsgs = CalTotalSentMsgs();
            UIControl.DisplayNumSentMsgs(NumSentMsgs);
        }
    }

    private float CalTotalDistance()
    {
        float distance = 0;
        foreach (GameObject node_go in swarm)
        {
            distance += node_go.GetComponent<Node>().totalDistance;
        }
        return distance;
    }

    private int CalTotalSentMsgs()
    {
        int msgs = 0;
        foreach (GameObject node_go in swarm)
        {
            msgs += node_go.GetComponent<Node>().NumSentMsgs;
        }
        return msgs;
    }


    public void TargetReached(GameObject target)
    {
        RemainingTargets.Remove(target);
        if (RemainingTargets.Count == 0)
        {
            timer.StopTimer();
            Debug.Log("All targets reached");
            _simmulationRunning = false;

            //Save results to file
            SaveResults();

            //Restart scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void SaveResults()
    {
        string path = "Assets/Results/test.txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(timer.GetTimer() + ";" + NumSentMsgs + ";" + TotalDistance);
        writer.Close();
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
