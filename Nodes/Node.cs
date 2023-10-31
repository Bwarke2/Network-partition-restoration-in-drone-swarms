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
    
    //Communication attributes
    private Communication _com;
    private Swarm _swarm;    //Swarm object

    //Leader attributes
    //public List<Node> Members = new List<Node>(); //Number of connected nodes
    public List<Transform> Unreached_Targets = new List<Transform>();         //Number of unreached targets
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
        _swarm = GameObject.FindGameObjectWithTag("Swarm").GetComponent<Swarm>();
        _com = GetComponent<Communication>();
        _TaskAssignment.Setup(_com);
        //foreach (GameObject go in GameObject.FindGameObjectsWithTag("Node"))
            //Members.Add(go.GetComponent<Node>());
    }

    void Start()
    {
        _com.UpdateNeighbours();
        if (name == "Node_leader")
        {
            Debug.Log("Running leader code");
            L_id = ID;
            _swarm.Leader = this;
            _com.FindHopsToLeader();
            Broadcast_Winner(Term + 1);
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
        _com.UpdateNeighbours();
        //Target assignment
        _TaskAssignment.AssignTasks(this, Unreached_Targets, _swarm.GetMembers());

        if (Target != null)
            Move(Target.position);
        else if (_com.ConnectedToLeader)
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
            desired_pos = _com.GetConnectingNeighbor().transform.position;
        }
        else
            desired_pos = target;
        // Check if inside safe distance
        bool inside_safe_distance = false;
        foreach (GameObject node in _com.GetNeighbours())
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
        return Vector2.Distance(_com.GetConnectingNeighbor().transform.position, transform.position);
    }

    public bool TargetReached()
    {
        //Check if target reached
        if (Vector2.Distance(transform.position, Target.position) < 0.001f)
        {
            string value = JsonConvert.SerializeObject(Target.gameObject.name);
            Target = null;
            _com.SendMsg(this, MsgTypes.TargetReachedMsg, L_id, value);
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
        if (_swarm.GetMembers().Count + 1 < _com.GetNSN())
        {
            return;
        }
        //Send RP to neighbours
        foreach (Node node in _swarm.GetMembers())
        {
            //Change this later to send messages instead of changing variables
            _com.SendMsg(this, MsgTypes.SetRPMsg, node.ID, JsonConvert.SerializeObject(RP, Formatting.None,
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
                _com.SendMsg(this, MsgTypes.SetTargetMsg, node.ID, JsonUtility.ToJson(target));
        }
    }

    public void SetOwnTarget(Transform target)
    {
        Target = target;
    }

    public void SetTargetMsgHandler(int sender_id, string value)
    {
        Transform target = GameObject.Find(JsonConvert.DeserializeObject<string>(value)).transform;
        SetOwnTarget(target);
    }

    public void TargetReachedMsgHandler(int sender_id, string value)
    {
        if (ID == L_id)
        {
            Transform target_reached = GameObject.Find(JsonConvert.DeserializeObject<string>(value)).transform;
            _TaskAssignment.Pursuing_Targets.Remove(target_reached);
            target_reached.gameObject.GetComponent<Target>().TargetReachedByNode();
            _TaskAssignment.AssignTasks(this, Unreached_Targets, _swarm.GetMembers());
        }
    }

    public void SetRPMsgHandler(int sender_id, string value) //Something is different here
    {
        RP = JsonConvert.DeserializeObject<Vector2>(value);
    }

    
    public void RollCallRecvHandler(int sender_id, string value)
    {
        int id_to_check = JsonConvert.DeserializeObject<int>(value);
    }

    public void AnounceAuctionMsgHandler(int sender_id, string value)
    {
        _TaskAssignment.HandleAnounceAuctionMsg(this,sender_id, value);
    }

    public void ReturnBitMsgHandler(int sender_id, string value)
    {
        _TaskAssignment.AddBid(sender_id, JsonConvert.DeserializeObject<float>(value));
    }




}
