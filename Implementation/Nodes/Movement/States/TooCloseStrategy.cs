using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooCloseStrategy : IMovementStrategy
{
    private float _safeMargin = 2;
    private Movement _movement;
    public void SetMovement(Movement movement)
    {
        _movement = movement;
    }
    protected List<Node> Neighbors = new List<Node>();
    public void SetNeighbors(List<Node> neighbors)
    {
        Neighbors = neighbors;
    }
    private Node GetClosestNeighbor(Node node)
    {
        Node closest = null;
        float closest_dist = Mathf.Infinity;
        foreach (Node neighbor in Neighbors)
        {
            float dist = Vector3.Distance(neighbor.transform.position, node.transform.position);
            if (dist < closest_dist)
            {
                closest_dist = dist;
                closest = neighbor;
            }
        }
        return closest;
    }
    public Vector3 GetDesiredPosition(Node node)
    {
        //Debug.Log("Moving away from neighbor");
        if (Neighbors.Count == 0)
            return node.transform.position;
        Node closest = GetClosestNeighbor(node);

        float dist = Vector3.Distance(node.transform.position, closest.transform.position);
        if (dist > _safeMargin)
        {
            _movement.NormalRangeEvent(node);
        }

        //Get direction away from closest
        Vector3 dir = node.transform.position - closest.transform.position;
        //dir = dir.normalized;
        return node.transform.position + dir;
        
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
}