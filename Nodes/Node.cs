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
    
    public bool Do_elctions = true;
    //Metrics
    public float totalDistance = 0;
    private Vector3 previousLoc = Vector3.zero; //Used to calculate total distance

    public int NumSentMsgs = 0;

    //Heartbeat component
    private HeartBeat _heartbeat;

    //Movement component
    private Movement _movement;
    
    //Communication attributes
    private Communication _com;
    private Swarm _swarm;    //Swarm object

    //Leader attributes
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
        _leaderElection.Startup(_com, ID, Do_elctions);
        _movement = GetComponent<Movement>();
        _movement.Setup(_swarm);
        _heartbeat = GetComponent<HeartBeat>();
        
    }

    void Start()
    {
        _TaskAssignment.Setup();
        _com.UpdateNeighbours();
        if (name == "Node_leader")
        {
            _leaderElection.LeaderStart(ID);
        }
        _heartbeat.Setup(ID, 0, _com);
        StartCoroutine(LeaderElection());
        StartCoroutine(UpdateRP());
        StartCoroutine(TaskAssignment());
    }

    // Update is called once per frame
    void Update()
    {
        _com.UpdateNeighbours();
    }

    void LateUpdate()
    {
        RecordDistance();
        _movement.CheckForMovementEvents(this);
        _movement.Move(this);
        NumSentMsgs = GetNumSentMsgs();
    }

    float GetBattery()
    {
        return Bat;
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
            
            yield return null;
            yield return null;
            if (ID == _leaderElection.GetLeaderID())
                SendNewRP();
            //Debug.Log("RP updated");
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator TaskAssignment()
    {
        while (true)
        {
            yield return null;
            yield return null;
            if (ID == _leaderElection.GetLeaderID())
                _TaskAssignment.AssignTasks(this, _swarm.GetMembers());
            yield return new WaitForSeconds(10);
        }
    }

    private IEnumerator LeaderElection()
    {
        while (true)
        {
            yield return null;
            if (ID == _leaderElection.GetLeaderID())
                _leaderElection.StartElection(_swarm.GetMembers());
            yield return new WaitForSeconds(20);
                if (_leaderElection.Do_elctions == false)
                    break; //Only do one election
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
                _com.SendMsg<int>(MsgTypes.RollCallMsg, node.ID, ID);
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
            _com.SendMsg<Vector2>(MsgTypes.SetRPMsg, node.ID, RP);
        }
    }

    private void ChooseRP()
    {
        //Choose RP
        switch (_swarm.GetPartitionPolicy())
        {
            case PartitionPolicy.PRP1:
                if (_movement.Path.Last() != null)
                    RP = _movement.Path.Last();
                else    
                    RP = _movement.GetTarget().position;
                break;
            case PartitionPolicy.PRP2:
                if (_movement.Path.Last() != null)
                    RP = _movement.Path.Last();
                else
                    RP = _movement.GetTarget().position;
                break;
            case PartitionPolicy.PRP3:
                if (_movement.GetTarget() != null)
                    RP = _movement.GetTarget().position;
                else
                    RP = _movement.Path.Last();
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
                _com.SendMsg<Transform>(MsgTypes.SetTargetMsg, node.ID, target);
        }
    }

    public void SetOwnTarget(Transform target)
    {
        ShowTarget = target;
        _movement.SetTarget(target);
        _movement.NewTargetEvent(this);
    }

    public void RaiseNoSwarmMovementEvent()
    {
        if (_movement.GetTarget() == null)
        {
            Debug.Log("No target for leader");
            return;
        }
        _com.BroadcastMsg<string>(MsgTypes.SwarmStuckMsg, _movement.GetTarget().name);
    }

    public void NoSwarmMovementMsgHandler(int sender_id, string value)
    {
        Transform newTarget = GameObject.Find(JsonConvert.DeserializeObject<string>(value)).transform;
        _movement.NoSwarmMovementEvent(this,newTarget);
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
        else
        {
            Debug.Log("Target reached by node: " + name);
            Debug.Log("Wrong node recieved targetReachedMsg: " + name);
        }
    }

    IEnumerator LeaderTargetReachedHandler(Transform target_reached)
    {
        yield return new WaitForSeconds(1);
        _TaskAssignment.AssignTasks(this, _swarm.GetMembers());
        if (target_reached != null)
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
        _TaskAssignment.HandleBroadcastWinnerMsg(sender_id, ID, value);
        _heartbeat.HandleBroadcastWinnerMsg(value);
    }

    public void HeartBeatMsgHandler(int sender_id, string value)
    {
        _heartbeat.HandleHeartBeatMsg(sender_id, value);
        _movement.HeartBeatEvent(this);
    }
    
    public void LostNodeDroppedMsgHandler(int sender_id, string value)
    {
        //Debug.Log("Recieved lost node dropped msg");
        _heartbeat.HandleLostNodeDroppedMsg(sender_id, value);
        _movement.LostNodeDroppedMsgHandler(sender_id, value);
    }

    public void VoteMsgHandler(int sender_id, string value)
    {
        _leaderElection.HandleVoteMsg(sender_id, value);
    }

    public void ElectionMsgHandler(int sender_id, string value)
    {
        //Debug.Log("Recieved election message");
        _leaderElection.HandleElectionMsg(sender_id, value);
    }

    public void HeartBeatResponseMsgHandler(int sender_id, string value)
    {
        _heartbeat.HandleHearthBeatResponseMsg(sender_id, value);
    }

    public void PartitionMsgHandler(int sender_id, string value)
    {
        _movement.HandlePartitionMsg(sender_id, value);
    }

    public void PartitionRestoredMsgHandler(int sender_id, string value)
    {
        _movement.HandlePartitionRestoredMsg(sender_id, value);
    }
}
