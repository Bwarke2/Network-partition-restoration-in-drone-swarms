using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Movement : MonoBehaviour
{
    private IMovementStrategy _moveStrat = new NoTargetStrategy();
    private Swarm _swarm;
    private Communication _com;
    private Transform _target;

    public List<Vector2> _Path = new List<Vector2>();
    public Vector2 Last_Target_Pos;
    private Node _attached_node;
    public const float Safe = 1;   //Safe distance from other nodes
    public bool _waitingForLostNode = false;

    public void Setup(Swarm swarm, Communication com, Node node, IMovementStrategy moveStrat)
    {
        _swarm = swarm;
        _com = com;
        _attached_node = node;
        _Path.Add(node.transform.position);
        Last_Target_Pos = node.transform.position;
        SetStrategy(moveStrat);
    }

    public void Move(Node node)
    {
        _moveStrat.Move(node);
    }

    public void SetStrategy(IMovementStrategy moveStrat)
    {
        //Debug.Log("Node " + _attached_node.name +" Setting strategy to: " + moveStrat.GetType().Name);
        if (_moveStrat != moveStrat)
        {
            _moveStrat = moveStrat;
            _moveStrat.SetMovement(this);
        }
    }

    public void SetTarget(Transform new_target)
    {
        _Path.Add(_attached_node.transform.position);
        if (new_target == null)
            //Debug.Log("Setting target to null in node: " + _attached_node.name);
        if (_target != null)
            Last_Target_Pos = _target.position;
        _target = new_target;
    }

    public Transform GetTarget()
    {
        return _target;
    }

    public bool TargetReached()
    {
        //Check if target reached
        if (Vector2.Distance(_attached_node.transform.position, _target.position) < 0.001f)
        {
            string value = JsonConvert.SerializeObject(_target.gameObject.name);
            SetTarget(null);
            TargetReachedEvent(_attached_node);
            _com.SendMsg(_attached_node, MsgTypes.TargetReachedMsg, _attached_node.GetLeaderID(), value);
            return true;
        }
        return false;
    }

    public IMovementStrategy GetStrategy()
    {
        return _moveStrat;
    }

    public void DecideMoveStrat(Node in_node)
    {
        //Check if partition has happened
        if (_swarm.GetMembers().Count < _com.GetNSN())
        {
            LostNodeEvent(in_node);
            return;
        }
        
        //Check if partition is restored
        CheckIfPartitionIsRestored(in_node);

        //Check if to far from connecting node
        float F_obj = in_node.FindFobj();
        if (F_obj < 1)
        {
            TooFarEvent(in_node, _com.GetConnectingNeighbor());
            return;
        }
        //Check if too close to neighbour
        CheckIfToClose(in_node, _com.GetNeighbours());

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

    private void CheckIfPartitionIsRestored(Node in_node)
    {
        if (_moveStrat is ILostNodePRP || _moveStrat is ISwarmPRP)
        {
            if (_swarm.GetMembers().Count >= _com.GetNSN())
            {
                PartitionRestoredEvent(in_node);
            }
        }
    }

    public void LostNodeEvent(Node node)
    {
        _moveStrat.HandleLostNode(node, _com, this, _swarm);
    }

    public void PartitionRestoredEvent(Node node)
    {
        if (_waitingForLostNode)
        {
            StopCoroutine(WaitForLostNode());
            _waitingForLostNode = false;
        }
        
        _moveStrat.HandlePartitionRestored(node);
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
        //_moveStrat.HandleNoMovement(node);
    }

    public void LostNodeDroppedEvent(Node node)
    {
        _moveStrat.HandleLostNodeDropped(node);
    }

    public void LostNodeDroppedMsgHandler(int sender_id, string value)
    {
        int num_of_members = JsonConvert.DeserializeObject<int>(value);
        Debug.Log("Lost node dropped in node: " + _attached_node.name + ", new NSN: " + num_of_members);
        _com.SetNSN(num_of_members);
        _moveStrat.HandleLostNodeDropped(_attached_node);   
    }

    public void RPReachedByLeaderEvent()
    {
        StartCoroutine(WaitForLostNode());
    }

    IEnumerator WaitForLostNode()
    {
        _waitingForLostNode = true;
        yield return new WaitForSeconds(10);
        Debug.Log("Done waiting for lost node");
        int numOfMembers = _swarm.GetMembers().Count;
        int numOfDroppedNodes = _com.GetNSN() - numOfMembers;
        _swarm.AddDroppedNode(numOfDroppedNodes);
        _com.SetNSN(numOfMembers);
        _com.BroadcastMsg(this._attached_node, MsgTypes.LostNodeDroppedMsg, JsonConvert.SerializeObject(numOfMembers, Formatting.None,
                        new JsonSerializerSettings()
                        { 
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        _moveStrat.HandleLostNodeDropped(_attached_node);  
    }
}