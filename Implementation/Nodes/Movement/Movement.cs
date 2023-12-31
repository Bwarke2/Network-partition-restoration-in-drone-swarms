using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Movement : MonoBehaviour
{
    private IMovementStrategy _moveStrat = new NoTargetStrategy();
    [SerializeField]
    private string _strategyName;
    private Swarm _swarm;
    [SerializeField]
    private Transform _target;
    private int _max_nodes = 0;
    private int _currentNrNodes = 0;
    public List<Vector2> Path = new List<Vector2>();
    private Queue<Vector2> _lastPositions = new Queue<Vector2>();
    public Vector2 Last_Target_Pos;
    public const float Safe = 1;   //Safe distance from other nodes
    private bool _waitingForLostNode = false;
    private int _waitTime = 60;

    public void Setup(Swarm swarm)
    {
        _swarm = swarm;
        _max_nodes = _swarm.GetMembers().Count;
        _currentNrNodes = _max_nodes;
        Path.Add(transform.position);
        Last_Target_Pos = transform.position;
        SetStrategy(new NoTargetStrategy());
        switch (_swarm.GetPartitionPolicy())
        {
            case PartitionPolicy.PRP1:
                _waitTime = 1;
                break;
            default:
                _waitTime = 60;
                break;
        }
    }

    public void Move(Node node)
    {
        Vector3 desiredPos = _moveStrat.GetDesiredPosition(node);
        MoveNode.MoveToward(node, desiredPos);
        CheckIfTargetReached();
        if (_lastPositions.Count >= 10)
        {
            if (!CheckIfMoved(FindAveragePosition(), node.transform.position))
            {
                //if (node.debug_node == true)
                    //Debug.Log("Node " + node.name + " did not move");
                NoMovementEvent(node);
            }
            _lastPositions.Clear();
        }
        _lastPositions.Enqueue(node.transform.position);
    }

    private Vector2 FindAveragePosition()
    {
        Vector2 average = Vector2.zero;
        foreach (Vector2 pos in _lastPositions)
        {
            average += pos;
        }
        average /= _lastPositions.Count;
        return average;
    }

    private bool CheckIfMoved(Vector2 averagePos, Vector2 currentPos)
    {
        float dist = Vector2.Distance(averagePos, currentPos);
        //if (GetComponent<Node>().debug_node == true)
            //Debug.Log("Distance moved: " + dist);
        if (dist < 0.05)
        {
            //Debug.Log("Node did not move");
            return false;
        }
            
        return true;
    }

    public void SetStrategy(IMovementStrategy moveStrat)
    {
        if (GetComponent<Node>().debug_node == true)
            Debug.Log("Node " + GetComponent<Node>().name +" Setting strategy to: " + moveStrat.GetType().Name);
        if (_moveStrat != moveStrat)
        {
            _moveStrat = moveStrat;
            _moveStrat.SetMovement(this);
            _strategyName = _moveStrat.GetType().Name;
        }
    }

    public void SetTarget(Transform new_target)
    {
        Path.Add(GetComponent<Node>().transform.position);
        /*if (new_target == null)
            Debug.Log("Setting target to null in node: " + GetComponent<Node>().name);*/
        if (_target != null)
        {
            Last_Target_Pos = _target.position;
        }
            
        _target = new_target;
    }

    public Transform GetTarget()
    {
        return _target;
    }

    public bool CheckIfTargetReached()
    {
        if ((_moveStrat is TargetStrategy) == false || _target == null)
            return false;
        float dist = Vector2.Distance(GetComponent<Node>().transform.position, _target.position);
        if (dist < 0.001f)
        {
            //Debug.Log("Target reached in node: " + GetComponent<Node>().name);
            string value = _target.gameObject.name;
            SetTarget(null);
            TargetReachedEvent(GetComponent<Node>());
            int L_id = GetComponent<Node>().GetLeaderID();
            //Debug.Log("Sending target reached message to leader: " + L_id);
            GetComponent<Communication>().SendMsg<string>(MsgTypes.TargetReachedMsg, L_id, value);
            return true;
        }
        return false;
    }

    public IMovementStrategy GetStrategy()
    {
        return _moveStrat;
    }

    public void CheckForMovementEvents(Node in_node)
    {
        //Check if partition has happened
        /*
        if (_swarm.GetMembers().Count < GetComponent<Communication>().GetNSN())
        {
            LostNodeEvent(in_node);
            return;
        }*/
        
        //Check if partition is restored
        //CheckIfPartitionIsRestored(in_node);

        //Check if to far from connecting node
        float F_obj = in_node.FindFobj();
        if (F_obj < 1 && GetComponent<Communication>().GetConnectingNeighbor() != null)
        {
            TooFarEvent(in_node, GetComponent<Communication>().GetConnectingNeighbor());
            return;
        }
        //Check if too close to neighbour
        CheckIfToClose(in_node, GetComponent<Communication>().GetNeighbours());

        //Check if target is null
        if (_target == null)
        {
            return;
        }
    }

    private void CheckIfToClose(Node in_node, List<GameObject> NB)
    {
        float dist_to_closest_nb = Mathf.Infinity;
        foreach (GameObject node_obj in NB)
        {
            float dist = Vector2.Distance(node_obj.transform.position, in_node.transform.position);
            if (dist < dist_to_closest_nb)
                dist_to_closest_nb = dist;
            if (dist < Safe)
            {
                //Move away from node
                List<Node> neighbours = new List<Node>();
                foreach (GameObject go in NB)
                {
                    neighbours.Add(go.GetComponent<Node>());
                }
                TooCloseEvent(in_node, neighbours);
                return;
            }
        }
    }

    /*private void CheckIfPartitionIsRestored(Node in_node)
    {
        if (_moveStrat is ILostNodePRP || _moveStrat is SwarmPRP)
        {
            if (_swarm.GetMembers().Count >= GetComponent<Communication>().GetNSN())
            {
                PartitionRestoredEvent(in_node);
            }
        }
    }*/

    public void LostNodeEvent(Node node)
    {
        _moveStrat.HandleLostNode(node, GetComponent<Communication>(), this, _swarm);
    }

    public void HeartBeatEvent(Node node)
    {
        _moveStrat.HandleHBEvent(node);
    }

    public void PartitionRestoredEvent(Node node)
    {
        if (_waitingForLostNode)
        {
            StopCoroutine(WaitForLostNode());
            _waitingForLostNode = false;
        }
        GetComponent<Communication>().SetNSN(_swarm.GetMembers().Count);
        _moveStrat.HandlePartitionRestored(node);
    }

    public void HandlePartitionMsg(int sender_id, string value)
    {
        //Debug.Log("Lost node message recieved in node: " + GetComponent<Node>().name);
        int lostNode_id = JsonConvert.DeserializeObject<int>(value);
        Debug.Log("Lost connection to node: " + lostNode_id + " in node: " + GetComponent<Node>().name);
        _currentNrNodes--;
        LostNodeEvent(GetComponent<Node>());
    }

    public void HandlePartitionRestoredMsg(int sender_id, string value)
    {
        //Check if all partitions are restored
        int lostNode_id = JsonConvert.DeserializeObject<int>(value);
        //Debug.Log("Node rejoined network: " + GetComponent<Node>().name);
        _currentNrNodes++;
        if (_currentNrNodes < _max_nodes)
            return;
        else if (_currentNrNodes > _max_nodes)
        {
            _max_nodes = _currentNrNodes;
            //Debug.Log("Max nodes increased to: " + _max_nodes);
        }
        PartitionRestoredEvent(GetComponent<Node>());
    }

    public void TargetReachedEvent(Node node)
    {
        _moveStrat.HandleTargetReached(node);
    }

    public void NewTargetEvent(Node node)
    {
        _moveStrat.HandleNewTarget(node);
    }

    public void TooCloseEvent(Node node, List<Node> neighbors)
    {
        _moveStrat.HandleTooClose(node, neighbors);
    }

    public void TooFarEvent(Node node, Node connectingNode)
    {
        _moveStrat.HandleTooFar(node, connectingNode);
    }

    public void NormalRangeEvent(Node node)
    {
        _moveStrat.HandleNormalRange(node);
    }

    public void NoMovementEvent(Node node)
    {
        //Implement later
        _moveStrat.HandleNoMovement(node);
    }

    public void NoSwarmMovementEvent(Node node, Transform newTarget)
    {
        //Debug.Log("No swarm movement in node " + node.name + " moving towards " + newTarget.name);
        _moveStrat.HandleNoSwarmMovement(node,newTarget);
    }

    public void LostNodeDroppedMsgHandler(int sender_id, string value)
    {
        int num_of_members = JsonConvert.DeserializeObject<int>(value);
        Debug.Log("Lost node dropped in node: " + GetComponent<Node>().name + ", new NSN: " + num_of_members);
        GetComponent<Communication>().SetNSN(num_of_members);
        _max_nodes--;
        SetWaitingForLostNode(false);
        _moveStrat.HandleLostNodeDropped(GetComponent<Node>());
    }

    public void RPReachedByLeaderEvent()
    {
        StartCoroutine(WaitForLostNode());
    }

    IEnumerator WaitForLostNode()
    {
        _waitingForLostNode = true;
        Debug.Log("Waiting for lost node");
        if (_swarm.GetPartitionPolicy() != PartitionPolicy.PRP1)
            yield return new WaitForSeconds(_waitTime);
        else
            yield return null;
        Debug.Log("Done waiting for lost node");
        _waitingForLostNode = false;
        int numOfMembers = _swarm.GetMembers().Count;
        _swarm.AddDroppedNode();
        GetComponent<Communication>().SetNSN(numOfMembers);
        GetComponent<Communication>().BroadcastMsg<int>(MsgTypes.LostNodeDroppedMsg, numOfMembers);
        GetComponent<Communication>().SendMsg<int>(MsgTypes.LostNodeDroppedMsg, 
            GetComponent<Node>().ID, numOfMembers);
    }

    public void SetWaitingForLostNode(bool value)
    {
        _waitingForLostNode = value;
    }

    public bool GetWaitingForLostNode()
    {
        return _waitingForLostNode;
    }
}