using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovementStrategy
{
    protected const float _speed = 1; //Movement speed
    public void SetConnectingNode(Node node)
    {
    }
    public void SetNeighbors(List<Node> neighbors)
    {
    }

    public void SetMovement(Movement movement);

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

    public void HandlePartitionRestored(Node node){}
    public void HandleTargetReached(Node node){}
    public void HandleNewTarget(Node node){}
    public void HandleTooClose(Node node, List<Node> Neighbors){}
    public void HandleTooFar(Node node, Node ConnectingNode){}
    public void HandleNormalRange(Node node){}
    abstract void Move(Node node);
}