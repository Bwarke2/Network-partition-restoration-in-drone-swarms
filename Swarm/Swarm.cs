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
    public List<GameObject> DroppedNodes = new List<GameObject>();
    public int NumDroppedNodes = 0;
    public List<GameObject> RemainingTargets = new List<GameObject>();

    private bool _simmulationRunning = false;
    private Queue<Vector2> _lastPositions = new Queue<Vector2>();

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
        StartCoroutine(UpdateCentralPosition());
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

            if (timer.GetTimer() > 600)
            {
                timer.StopTimer();
                Debug.Log("Time limit reached");
                _simmulationRunning = false;

                //Save results to file
                SaveResults();

                //Restart scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    private IEnumerator UpdateCentralPosition()
    {
        while (_simmulationRunning)
        {
            Vector2 newCenter = FindCenterPositionOfSwarm();
            _lastPositions.Enqueue(newCenter);
            if (_lastPositions.Count >= 5)
            {
                if (CheckIfMoved(newCenter,_lastPositions) == false)
                {
                    Debug.Log("Swarm did not move");
                    Leader.RaiseNoSwarmMovementEvent();
                }
                _lastPositions.Clear();
            }
            //Debug.Log("RP updated");
            yield return new WaitForSeconds(1);
        }
    }

    private bool CheckIfMoved(Vector2 currentCenter, Queue<Vector2> lastPositions)
    {
        Vector2 average = Vector2.zero;
        foreach (Vector2 pos in lastPositions)
        {
            average += pos;
        }
        average /= lastPositions.Count;

        float dist = Vector2.Distance(average, currentCenter);
        //Debug.Log("Distance moved by swarm: " + dist);
        //Debug.Log("Average position: " + average + " Current position: " + currentCenter);
        if (dist < 0.2)
        {
            Debug.Log("Swarm did not move");
            return false;
        }
        return true;
    }

    private float CalTotalDistance()
    {
        float distance = 0;
        foreach (Node node_go in GetMembers())
        {
            distance += node_go.totalDistance;
        }
        return distance;
    }

    private int CalTotalSentMsgs()
    {
        int msgs = 0;
        foreach (Node node_go in GetMembers())
        {
            msgs += node_go.NumSentMsgs;
        }
        return msgs;
    }


    public void TargetReached(GameObject target)
    {
        RemainingTargets.Remove(target);
        if (RemainingTargets.Count > 0)
        {
            return;
        }

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
        string path = "Assets/Results/";
        path += SceneManager.GetActiveScene().name;
        switch (CurrentPartitionPolicy)
        {
            case PartitionPolicy.PRP1:
                path += "/PRP1.txt";
                break;
            case PartitionPolicy.PRP2:
                path += "/PRP2.txt";
                break;
            case PartitionPolicy.PRP3:
                path += "/PRP3.txt";
                break;
            default:
                break;
        }
        StreamWriter writer = new StreamWriter(path, true);
        int nodes_lost = swarm.Count - GetMembers().Count;
        if (timer.GetTimer() > 600)
            nodes_lost = swarm.Count;
        writer.WriteLine(timer.GetTimer() + ";" + NumSentMsgs + ";" + TotalDistance + ";" + nodes_lost);
        writer.Close();
    }

    public void AddMember(Node node)
    {
        if (JoinedNodes.Contains(node) == false)
            JoinedNodes.Add(node);
        if (LostNodes.Contains(node.gameObject) == true)
            LostNodes.Remove(node.gameObject);
        if (DroppedNodes.Contains(node.gameObject) == true)
        {
            DroppedNodes.Remove(node.gameObject);
            NumDroppedNodes = DroppedNodes.Count;
        }
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

    public void AddDroppedNode()
    {
        DroppedNodes.AddRange(LostNodes);
        LostNodes.Clear();
        NumDroppedNodes = DroppedNodes.Count;
        Debug.Log("Dropped nodes: " + NumDroppedNodes);
    }

    public Vector2 FindCenterPositionOfSwarm()
    {
        Vector2 center = Vector2.zero;
        foreach (Node node in GetMembers())
        {
            center += (Vector2)node.transform.position;
        }
        center /= GetMembers().Count;
        return center;
    }
}
