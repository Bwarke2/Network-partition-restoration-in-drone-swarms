using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Node : MonoBehaviour
{
    public int ID = 0;        //Node ID
    public const int Type = 0;      //Node Type
    public const float Safe = 1;   //Safe distance from other nodes
    public Vector2 Position;        //Current node position
    public Vector2 P_start;   //Starting node position
    public Transform Target = null;   //Target node position
    public Vector2 RP;       //Rendezvous Point
    public float Bat = 100;         //Battery
    private int L_id = 0;            //Leader ID
    private int Term = 0;            //Term
    public int NSN;             // Node start number
    public List<GameObject> NB = new List<GameObject>();    //Neighbouring nodes

    public int Hops = int.MaxValue;        //Number of hops to leader
    public bool ConnectedToLeader = false; //Connected to leader
    private Node ConnnectingNeighbor;       //Connecting neighbour
    private Swarm _swarm;    //Swarm object

    //Leader attributes
    //public List<Node> Members = new List<Node>(); //Number of connected nodes
    public List<Transform> Unreached_Targets = new List<Transform>();         //Number of unreached targets
    public List<Transform> Pursuing_Targets = new List<Transform>();         //Number of reached targets
    private IDictionary<int, float> Bids = new Dictionary<int, float>();

    private TaskAssignmnet _TaskAssignment = new TaskAssignmnet();

    //Constants
    public const float com_range = 10;         //Communication range
    public const float speed = 1; //Movement speed

    // Start is called before the first frame update
    void Awake()
    {
        ID = GetInstanceID();
        P_start = transform.position;   
        Position = P_start;   
        RP = P_start;
        NSN = GameObject.FindGameObjectsWithTag("Node").Length;
        _swarm = GameObject.FindGameObjectWithTag("Swarm").GetComponent<Swarm>();
        //foreach (GameObject go in GameObject.FindGameObjectsWithTag("Node"))
            //Members.Add(go.GetComponent<Node>());
    }

    void Start()
    {
        UpdateNeighbours();
        if (name == "Node_leader")
        {
            Debug.Log("Running leader code");
            L_id = ID;
            _swarm.Leader = this;
            ConnectedToLeader = true;
            Hops = 0;
            int newTerm = Term + 1;
            Broadcast_Winner(newTerm);
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Target"))
                Unreached_Targets.Add(go.transform);

            //Sort targets pased on distance
            Unreached_Targets.Sort(delegate (Transform a, Transform b)
            {
                return Vector2.Distance(a.position, transform.position).CompareTo(Vector2.Distance(b.position, transform.position));
            });
            //RollCall();
            SendRP();
            _TaskAssignment.AssignTasks(this, Unreached_Targets, _swarm.GetMembers());
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        UpdateNeighbours();
        Hops = FindHopsToLeader();

        //Target assignment
        _TaskAssignment.AssignTasks(this, Unreached_Targets, _swarm.GetMembers());

        if (Target != null)
            Move(Target.position);
        else if (ConnectedToLeader)
        {
            //Move toward leader's position
            Move(_swarm.Leader.transform.position);
        }
        else
        {
            //Move toward RP
            Move(RP);
        }
    }

    private void UpdateNeighbours()
    {
        //Update neighbours
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node"); //Find all nodes
        NB = new List<GameObject>(); //Clear neighbour list
        foreach (GameObject node in nodes)
        {
            if (node.name != name)
            {
                if (Vector2.Distance(node.transform.position, transform.position) < com_range)
                {
                    NB.Add(node); //Add node to neighbour list
                }
            }
        }
    }

    private int FindHopsToLeader()
    {
        //Find min hop count to leader among neighbors
        if (ID == L_id)
            return Hops = 0;
        else
        {
            int min_hop = int.MaxValue;
            foreach (GameObject node in NB)
            {
                if (node.GetComponent<Node>().Hops < min_hop)
                {
                    min_hop = node.GetComponent<Node>().Hops;
                }
            }

            if (min_hop >= NSN)
            {
                //Debug.Log("No connection to leader");
                ConnectedToLeader = false;
                _swarm.RemoveMember(this);
                min_hop = int.MaxValue;
                return min_hop;
            }
            else
            {
                ConnectedToLeader = true;
                _swarm.AddMember(this);
                return min_hop + 1;
            }
        }
    }

    

    /*
    private void RollCall() //Old
    {
        // Make list of temp members
        List<Node> temp_members = new List<Node>();
        foreach (Node node in temp_members)
        {
            Members.Remove(node);
            if (node.RollCallID(node.ID))
            {
                Members.Add(node);
            }
        }
    }

    public bool RollCallID(int id) //Old
    {
        if (ID == id)
            return true;
        else
            return false;
        /* Change this to propagate through the system
        else
        {
            // Prevent looping 

            foreach (Gameobject node in NB)
                if (node.SendMessage("RollCallID",id))
                    return true;
        }
        
    }
    */
    // actions
    public void Move(Vector2 target)
    {
        //Move to target
        float step = speed * Time.deltaTime;
        Vector2 desired_pos;
        if (FindFobj() < 1)
        {
            //Debug.Log("To far from Leader connection");
            desired_pos = ConnnectingNeighbor.transform.position;
        }
        else
            desired_pos = target;
        // Check if inside safe distance
        bool inside_safe_distance = false;
        foreach (GameObject node in NB)
        {
            if (Vector2.Distance(node.transform.position, transform.position) < Safe)
            {
                //Move away from node
                Vector3 dir = transform.position - node.transform.position;
                dir = dir.normalized;
                desired_pos = transform.position + dir;
                inside_safe_distance = true;
            }
        }
        transform.position = Vector2.MoveTowards(transform.position, desired_pos, step);
        
        if (Target != null)
            TargetReached();
    }

    private float FindFobj() //Basic version
    {
        return (com_range - DistanceToConnectingNeighbor());
    }

    private float DistanceToConnectingNeighbor()
    {
        //Find distance to connecting neighbour
        return Vector2.Distance(FindConnectingNeighbor().transform.position, transform.position);
    }

    private Node FindConnectingNeighbor()
    {
        float min_distance = float.MaxValue;
        int min_hop = int.MaxValue;
        foreach (GameObject node in NB)
        {
            if (node.GetComponent<Node>().Hops <= min_hop)
            {
                min_hop = node.GetComponent<Node>().Hops;
                if (Vector2.Distance(node.transform.position, transform.position) < min_distance)
                {
                    min_distance = Vector2.Distance(node.transform.position, transform.position);
                    ConnnectingNeighbor = node.GetComponent<Node>();
                }  
            }
        }
        return ConnnectingNeighbor;
    }


    public bool TargetReached()
    {
        //Check if target reached
        if (Vector2.Distance(transform.position, Target.position) < 0.001f)
        {
            string value = JsonConvert.SerializeObject(Target.gameObject.name);
            Target = null;
            SendMsg(MsgTypes.TargetReachedMsg, L_id, value);
            return true;
        }
        return false;
    }

    public void StartElection()
    {
        //Start election

        Term = Term + 1;
        //Broadcast result
        Broadcast_Winner(Term);
    }

    public void Vote()
    {
        //Vote for leader
    }

    public void SendRP()
    {
        // Add code to only allow setting when all nodes i connected
        if (_swarm.GetMembers().Count + 1 < NSN)
        {
            return;
        }
        //Send RP to neighbours
        foreach (Node node in _swarm.GetMembers())
        {
            //Change this later to send messages instead of changing variables
            SendMsg(MsgTypes.SetRPMsg, node.ID, JsonConvert.SerializeObject(RP, Formatting.None,
                        new JsonSerializerSettings()
                        { 
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }
    }

    private void Broadcast_Winner(int new_term)
    {
        //Debug.Log("Broadcasting winner");
        if (new_term <= Term)
        {
            return;
        }
        Term = new_term;
        
        //Debug.Log("New leader in " + name + " is: " + L_id);
        //Broadcast winner to neighbours
        foreach (Node node in _swarm.GetMembers())
        {
            //Change this later to send messages instead of changing variables
            node.L_id = L_id;
            //Debug.Log("Broadcasting winner to: " + node.name);
            node.Broadcast_Winner(new_term);
            //Debug.Log("Node " + ID + " broadcasting winner: " + L_id + " with term: " + Term);
        }
    }

    

    public void SetTarget(Transform target, int node_id)
    {
        foreach (Node node in _swarm.GetMembers())
        {
            //Change this later to send messages instead of changing variables
            if (node.ID == node_id)
                SendMsg(MsgTypes.SetTargetMsg, node.ID, JsonUtility.ToJson(target));
        }
    }

    public void SetOwnTarget(Transform target)
    {
        Target = target;
    }

    public void SendMsg(MsgTypes msg_type, int recv_id, string value)
    {
        foreach (Node node in _swarm.GetMembers())
        {
            //Debug.Log("Checking node: " + node.ID);
            if (node.ID == recv_id)
            {
                node.ReceiveMsg(msg_type, ID, value);
                return;
            }
        }
        Debug.Log("No reciever mached id: " + recv_id);
    }

    public void ReceiveMsg(MsgTypes msg_type, int sender_id, string value)
    {
        // Handle unreliable communication
        // Recieve msg
        switch (msg_type)
        {
            case MsgTypes.SetTargetMsg:
                Transform target = GameObject.Find(JsonConvert.DeserializeObject<string>(value)).transform;
                SetOwnTarget(target);
                break;
            case MsgTypes.TargetReachedMsg:
                if (ID == L_id)
                {
                    Transform target_reached = GameObject.Find(JsonConvert.DeserializeObject<string>(value)).transform;
                    Pursuing_Targets.Remove(target_reached);
                    target_reached.gameObject.GetComponent<Target>().TargetReachedByNode();
                    _TaskAssignment.AssignTasks(this, Unreached_Targets, _swarm.GetMembers());
                }
                break;
            case MsgTypes.SetRPMsg:
                //Debug.Log(name + " recieved RP: " + JsonConvert.DeserializeObject<Vector2>(value));
                RP = JsonConvert.DeserializeObject<Vector2>(value);
                break;
            case MsgTypes.RollCallMsg:
                int id_to_check = JsonConvert.DeserializeObject<int>(value);
                RollCallRecvHandler(sender_id, id_to_check);
                break;
            case MsgTypes.RollCallResponseMsg:
                //To do
                break;
            case MsgTypes.AnounceAuctionMsg:
                _TaskAssignment.HandleAnounceAuctionMsg(this,sender_id, value);
                break;
            case MsgTypes.ReturnBitMsg:
                //Debug.Log("Recieved bit from: " + sender_id + " with value: " + value);
                _TaskAssignment.AddBid(sender_id,JsonConvert.DeserializeObject<float>(value));
                break;
        }
    }

    

    private void RollCallRecvHandler(int sender_id, int id_to_check)
    {
        //To do
        // Handle roll call if target
    }
}
