using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooFarStrategy : IMovementStrategy
{
    protected Node connecting_node = null;
    private float _safe_connecting_distance = 9;
    private Movement _movement;
    public void SetMovement(Movement movement)
    {
        _movement = movement;
    }

    public void SetConnectingNode(Node node)
    {
        connecting_node = node;
    }
    public Vector3 GetDesiredPosition(Node node)
    {
        if (connecting_node == null)
            return node.transform.position;
        if (Vector2.Distance(node.transform.position, connecting_node.transform.position) < _safe_connecting_distance)
        {
            _movement.NormalRangeEvent(node);
            return node.transform.position;
        }
        return connecting_node.transform.position;
    }

    public void HandleNormalRange(Node node)
    {
        Swarm swarm = GameObject.FindGameObjectWithTag("Swarm").GetComponent<Swarm>();
        IMovementStrategy newStrat;
        if (node.GetComponent<Communication>().ConnectedToLeader == false)
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
            _movement.SetStrategy(newStrat);
            return;

        }
        if (_movement.GetTarget() == null)
        {
            _movement.SetStrategy(new NoTargetStrategy());
            return;
        }
        else
        {
            _movement.SetStrategy(new TargetStrategy());
            return;
        }
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
}