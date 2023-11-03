using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Node : MonoBehaviour
{
    public int ID = 0;        //Node ID
    public const int Type = 0;      //Node Type
    public Vector2 Position;        //Current node position
    public Vector2 P_start;   //Starting node position
    //public Transform Target = null;   //Target node position
    public Vector2 RP;       //Rendezvous Point
    public float Bat = 100;         //Battery
    
    //Movement strategy
    private Movement _movement = null;
    
    //Communication attributes
    private Communication _com;
    private Swarm _swarm;    //Swarm object

    //Leader attributes
    public List<Transform> Unreached_Targets = new List<Transform>();         //Number of unreached targets
    private TaskAssignmnet _TaskAssignment = new TaskAssignmnet();

    private LeaderElection _leaderElection = new LeaderElection();

    //Constants
    public const float com_range = 10;         //Communication range
    
    public bool debug_node = false;
    // Start is called before the first frame update
    void Awake()
    {
        ID = GetInstanceID();
        P_start = transform.position;   
        Position = P_start;   
        RP = P_start;
        _swarm = GameObject.FindGameObjectWithTag("Swarm").GetComponent<Swarm>();
        _com = GetComponent<Communication>();
        _leaderElection.Startup(_com);
        _movement = new Movement(_swarm,_com, this, new NoTargetStrategy());
        _TaskAssignment.Setup(_com,_movement);
    }

    void Start()
    {
        _com.UpdateNeighbours();
        if (name == "Node_leader")
        {
            Debug.Log("Running leader setup code");
            _leaderElection.LeaderStart(ID);
            _leaderElection.StartElection(_swarm.GetMembers());
            _swarm.Leader = this;
            _com.FindHopsToLeader();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Target"))
                Unreached_Targets.Add(go.transform);

            //Sort targets based on distance to node start position
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
    }

    void LateUpdate()
    {
        _movement.DecideMoveStrat(this);
        _movement.Move(this);
    }

    public int GetLeaderID()
    {
        return _leaderElection.GetLeaderID();
    }

    public float FindFobj() //Basic version
    {
        if (ID == _leaderElection.GetLeaderID())
            return 1;
        return (com_range - DistanceToConnectedNeighbor());
    }

    private float DistanceToConnectedNeighbor()
    {
        float min_dist = Mathf.Infinity;
        foreach (Node node in _com.GetPossibleConnections())
        {
            float dist = Vector2.Distance(node.transform.position, transform.position);
            if (dist < min_dist)
            {
                min_dist = dist;
            }
        }
        return min_dist;
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
        _movement.SetTarget(target);
        _movement.NewTargetEvent(this);
    }

    public void SetTargetMsgHandler(int sender_id, string value)
    {
        Transform target = GameObject.Find(JsonConvert.DeserializeObject<string>(value)).transform;
        SetOwnTarget(target);
    }

    public void TargetReachedMsgHandler(int sender_id, string value)
    {
        if (ID == _leaderElection.GetLeaderID())
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

    public void BroadcastWinnerMsgHandler(int sender_id, string value)
    {
        //Debug.Log("Recieved broadcast winner msg");
        _leaderElection.HandleBroadcastWinnerMsg(sender_id, value);
    }
}
