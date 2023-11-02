using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement
{
    private IMovementStrategy _moveStrat = new NoTargetStrategy();
    private Swarm _swarm;
    private Communication _com;

    public Movement(Swarm swarm, Communication com, IMovementStrategy moveStrat)
    {
        _swarm = swarm;
        _com = com;
        SetStrategy(moveStrat);
    }

    public void Move(Node node)
    {
        _moveStrat.Move(node);
    }

    public void SetStrategy(IMovementStrategy moveStrat)
    {
        _moveStrat = moveStrat;
        _moveStrat.SetMovement(this);
    }

    public IMovementStrategy GetStrategy()
    {
        return _moveStrat;
    }

    public void LostNodeEvent(Node node)
    {
        _moveStrat.HandleLostNode(node, _com, this, _swarm);
    }

    public void PartitionRestoredEvent(Node node)
    {
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
}