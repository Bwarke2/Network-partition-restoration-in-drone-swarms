
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

    public void HandleLostNode(Node node, Communication com, Movement movement, Swarm swarm)
    {
        IMovementStrategy newStrat;
        if (com.ConnectedToLeader == false)
        {
            switch (swarm.GetPartitionPolicy())
            {
                case PartitionPolicy.PRP1:
                    newStrat = new LostNodePRP1();
                    break;
                case PartitionPolicy.PRP2:
                    newStrat = new LostNodePRP2();
                    break;
                case PartitionPolicy.PRP3:
                    newStrat = new LostNodePRP3();
                    break;
                default:
                    newStrat = new LostNodePRP1();
                    break;
            }
        }
        else
        {
            newStrat = new SwarmPRP();
        }
        movement.SetStrategy(newStrat);
    }

    public void HandleNoSwarmMovement(Node node, Transform newTarget)
    {
        Debug.Log("No swarm movement in node " + node.name + " moving towards " + newTarget.name);
        _movement.SetTarget(newTarget);
        _movement.SetStrategy(new TargetStrategy());
    }

    public void Move(Node node)
    {
        //Debug.Log("No target");
        //Do nothing
    }
}