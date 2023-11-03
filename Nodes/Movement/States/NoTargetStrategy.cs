
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NoTargetStrategy : IMovementStrategy
{
    private Movement _movement;
    public void SetMovement(Movement movement)
    {
        _movement = movement;
    }
    public void HandleNewTarget(Node node)
    {
        _movement.SetStrategy(new TargetStrategy());
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
        //Debug.Log("No target");
        //Do nothing
    }
}