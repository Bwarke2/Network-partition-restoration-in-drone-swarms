using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmPRP : IMovementStrategy
{
    private Movement _movement;
    private bool _waitingForLostNode = false;
    public void SetMovement(Movement movement)
    {
        _movement = movement;
    }

    public void Move(Node node)
    {
        //Debug.Log("Swarm PRP");
        
        
        if (node.RP == null)
            return;

        if (Vector2.Distance(node.transform.position, node.RP) < 3.5f)
        {
            if ((node.ID == node.GetLeaderID()) && (_waitingForLostNode == false))
            {
                Debug.Log("Leader Waiting for lost node");
                _movement.RPReachedByLeaderEvent();
            }
            _movement.SetWaitingForLostNode(true);
            _waitingForLostNode = true;
            //Dont move if within range of RP
            return;
        }

        //Move toward RP
        float step = IMovementStrategy._speed * Time.deltaTime;
        Vector2 desired_pos = node.RP;
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
    }

    public void HandleTooClose(Node node, List<Node> neighbors)
    {
        _movement.SetStrategy(new TooCloseStrategy());
        
        _movement.GetStrategy().SetNeighbors(neighbors);
    }

    public void HandleTooFar(Node node, Node connectingNode)
    {
        _movement.SetStrategy(new TooFarStrategy());
        _movement.GetStrategy().SetConnectingNode(connectingNode);
    }

    
    public void HandlePartitionRestored(Node node)
    {
        float distance = Vector2.Distance(node.transform.position, node.RP);
        if (distance > 3.1f)
        {
            //Debug.Log("Distance to RP: " + distance);
            return;
        }
        if (_movement.GetTarget() == null)
        {
            _movement.SetStrategy(new NoTargetStrategy());
            return;
        }
        _movement.SetStrategy(new TargetStrategy());
    }

    public void HandleLostNodeDropped(Node node)
    {
        if (_movement.GetTarget() == null)
        {
            _movement.SetStrategy(new NoTargetStrategy());
            return;
        }
        _movement.SetStrategy(new TargetStrategy());
    }

    public void HandleNoMovement(Node node)
    {
        //if(_movement.GetWaitingForLostNode() == false)
            //Debug.Log("Failed to reach RP");
    }
}