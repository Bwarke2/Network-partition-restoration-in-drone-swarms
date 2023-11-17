using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovementStrategy
{
    
    public void SetConnectingNode(Node node)
    {
    }
    public void SetNeighbors(List<Node> neighbors)
    {
    }

    public void SetMovement(Movement movement);

    public void HandleLostNode(Node node, Communication com, Movement movement, Swarm swarm){}
    public void HandlePartitionRestored(Node node){}
    public void HandleTargetReached(Node node){}
    public void HandleNewTarget(Node node){}
    public void HandleTooClose(Node node, List<Node> Neighbors){}
    public void HandleTooFar(Node node, Node ConnectingNode){}
    public void HandleNormalRange(Node node){}
    public void HandleNoSwarmMovement(Node node, Transform newTarget){}
    virtual public void HandleLostNodeDropped(Node node){}
    virtual public void HandleNoMovement(Node node){}
    abstract public Vector3 GetDesiredPosition(Node node);
}