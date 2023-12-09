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
    private int Term = 0;

    //Metrics
    public float TotalDistance = 0;
    public int NumSentMsgs = 0;

    public Node Leader;

    [SerializeField]
    private PartitionPolicy CurrentPartitionPolicy = PartitionPolicy.PRP1;

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
        CurrentPartitionPolicy = ChoosePartitionPolicy();
    }
    void Start()
    {
        timer.StartTimer();
        _simmulationRunning = true;
        StartCoroutine(UpdateCentralPosition());
    }

    private PartitionPolicy ChoosePartitionPolicy()
    {
        // Find shortest file in folder
        string path = "Assets/Results_LE/";
        path += SceneManager.GetActiveScene().name;
        DirectoryInfo di = new DirectoryInfo(path);
        FileInfo[] files = di.GetFiles();
        string shortest_file = "";
        long shortest_file_length = long.MaxValue;

         // Get a reference to each file in that directory.
        foreach (FileInfo file in files)
        {
            if (file.Name.Contains("meta") == false)
            {
                //Debug.Log("File: " + file.Name + " Length: " + file.Length);
                if (file.Length < shortest_file_length)
                {
                    shortest_file = file.Name;
                    shortest_file_length = file.Length;
                }
            }  
        }
        //Debug.Log("Shortest file: " + shortest_file + " Length: " + shortest_file_length);
        // Select partition policy based on shortest file
        if (shortest_file.Contains("PRP1"))
        {
            Debug.Log("PRP1");
            return PartitionPolicy.PRP1;
        }
        else if (shortest_file.Contains("PRP2"))
        {
            Debug.Log("PRP2");
            return PartitionPolicy.PRP2;
        }
        else if (shortest_file.Contains("PRP3"))
        {
            Debug.Log("PRP3");
            return PartitionPolicy.PRP3;
        }
        else
        {
            Debug.Log("No partition policy found");
            return PartitionPolicy.PRP1;
        }
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

    public void SetLeader(int l_id, int new_term)
    {
        //Debug.Log("New term is: " + new_term + "With leader: " + l_id);
        if (new_term > Term)
        {
            Term = new_term;
            //Find leader with id
            foreach (Node node in GetMembers())
            {
                if (node.ID == l_id)
                {
                    Leader = node;
                    //Debug.Log("New leader is: " + Leader.ID);
                    return;
                }
            }
        }
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
        string path = "Assets/Results_LE/";
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
        writer.WriteLine(timer.GetTimer() + ";" + NumSentMsgs + ";" + TotalDistance + ";" + nodes_lost + ";" + RemainingTargets.Count);
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

    public void AddMember(int node_id)
    {
        //Add node to member list
        foreach (GameObject node in swarm)
        {
            if (node.GetComponent<Node>().ID == node_id)
            {
                AddMember(node.GetComponent<Node>());
                return;
            }
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

    public void RemoveMember(int node_id)
    {
        //Remove node from member list
        foreach (GameObject node in swarm)
        {
            if (node.GetComponent<Node>().ID == node_id)
            {
                RemoveMember(node.GetComponent<Node>());
                return;
            }
        }
    }

    public void ClearMembers()
    {
        for (int i = JoinedNodes.Count - 1; i >= 0; i--)
        {
            RemoveMember(JoinedNodes[i]);
        }
    }

    public List<Node> GetMembers()
    {
        return JoinedNodes;
    }

    public List<GameObject> GetTargets()
    {
        return RemainingTargets;
    }

    public PartitionPolicy GetPartitionPolicy()
    {
        return CurrentPartitionPolicy;
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
