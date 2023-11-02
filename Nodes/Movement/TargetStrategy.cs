using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetStrategy : IMovementStrategy
{
    private Movement _movement;
    public void SetMovement(Movement movement)
    {
        _movement = movement;
    }
    public void HandleTargetReached(Node node)
    {
        _movement.SetStrategy(new NoTargetStrategy());
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

    public void Move(Node node)
    {
        //Debug.Log("Moving towards target");
        if (node.Target == null)
        {
            //Debug.Log("No target");
            return;
        }

        float step = IMovementStrategy._speed * Time.deltaTime;
        Vector2 desired_pos = node.Target.position;
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
    
        node.TargetReached();
    }
}