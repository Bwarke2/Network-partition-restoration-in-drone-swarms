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

    public void HandleLostNode(Node node, Communication com, Movement movement, Swarm swarm)
    {
        IMovementStrategy newStrat;
        if (com.ConnectedToLeader == false)
        {
            switch (swarm.CurrentPartitionPolicy)
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
        _movement.SetStrategy(new NoTargetStrategy());
    }

    public void Move(Node node)
    {
        //Debug.Log("Moving towards target");
        if (_movement.GetTarget() == null)
        {
            //Debug.Log("No target");
            return;
        }

        float step = IMovementStrategy._speed * Time.deltaTime;
        Vector2 desired_pos = _movement.GetTarget().position;
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
    
        _movement.TargetReached();
    }
}