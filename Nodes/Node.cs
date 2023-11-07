using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public class Node : MonoBehaviour
{
    public int ID = 0;        //Node ID
    public const int Type = 0;      //Node Type
    public Vector2 Position;        //Current node position
    public Vector2 P_start;   //Starting node position
    public Transform ShowTarget = null;   //Target node position
    public Vector2 RP;       //Rendezvous Point
    public float Bat = 100;         //Battery
    
    //Metrics
    public float totalDistance = 0;
    private Vector3 previousLoc = Vector3.zero; //Used to calculate total distance

    public int NumSentMsgs = 0;

    //Movement strategy
    private Movement _movement = null;
    
    //Communication attributes
    private Communication _com;
    private Swarm _swarm;    //Swarm object

    //Leader attributes
    //public List<Transform> Unreached_Targets = new List<Transform>();         //Number of unreached targets
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
            
            SendNewRP();
            StartCoroutine(UpdateRP());
            StartCoroutine(TaskAssignment());
            _TaskAssignment.AssignTasks(this, _swarm.GetMembers());
        }
    }

    // Update is called once per frame
    void Update()
    {
        _com.UpdateNeighbours();
        //Target assignment
        //_TaskAssignment.AssignTasks(this, Unreached_Targets, _swarm.GetMembers());
    }

    void LateUpdate()
    {
        RecordDistance();
        _movement.DecideMoveStrat(this);
        _movement.Move(this);
        NumSentMsgs = GetNumSentMsgs();
    }

    private void RecordDistance()
    {
        totalDistance += Vector3.Distance(transform.position, previousLoc);
	    previousLoc = transform.position;
    }

    private IEnumerator UpdateRP()
    {
        while (true)
        {
            SendNewRP();
            //Debug.Log("RP updated");
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator TaskAssignment()
    {
        while (true)
        {
            _TaskAssignment.AssignTasks(this, _swarm.GetMembers());
            yield return new WaitForSeconds(10);
        }
    }

    private IEnumerator RollCall()
    {
        //Debug.Log("Roll call started");
        yield return new WaitForSeconds(1);
        foreach (Node node in _swarm.GetMembers())
        {
            if (node.ID != ID)
            {
                //Debug.Log("Sending roll call to: " + node.ID);
                _com.SendMsg(this, MsgTypes.RollCallMsg, node.ID, JsonConvert.SerializeObject(ID, Formatting.None,
                        new JsonSerializerSettings()
                        { 
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
            }
        }
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

    public void SendNewRP()
    {
        // Add code to only allow setting when all nodes i connected
        if (_swarm.GetMembers().Count + 1 < _com.GetNSN())
        {
            return;
        }
        ChooseRP();
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

    private void ChooseRP()
    {
        //Choose RP
        switch (_swarm.CurrentPartitionPolicy)
        {
            case PartitionPolicy.PRP1:
                RP = _movement._Path.Last();
                break;
            case PartitionPolicy.PRP2:
                RP = _movement._Path.Last();
                break;
            case PartitionPolicy.PRP3:
                if (_movement.GetTarget() != null)
                    RP = _movement.GetTarget().position;
                else
                    RP = _movement._Path.Last();
                break;
        }
    }

    public int GetNumSentMsgs()
    {
        return _com.GetNumSentMsgs();
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
        ShowTarget = target;
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
        //Add message to all nodes to remove target from list
        Transform target = GameObject.Find(JsonConvert.DeserializeObject<string>(value)).transform;
        
        if (_movement.GetTarget() == target)
        {
            _movement.SetTarget(null);
            ShowTarget = null;
        }

        if (ID == _leaderElection.GetLeaderID())
        {
            if (!_TaskAssignment.RemovePursuingTarget(target))
                Debug.Log("Target not found in pursuing list");
            StartCoroutine(LeaderTargetReachedHandler(target));
        }
    }

    IEnumerator LeaderTargetReachedHandler(Transform target_reached)
    {
        yield return new WaitForSeconds(1);
        _TaskAssignment.AssignTasks(this, _swarm.GetMembers());
        target_reached.gameObject.GetComponent<Target>().TargetReachedByNode();
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
