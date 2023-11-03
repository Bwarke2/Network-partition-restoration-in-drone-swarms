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
    public void Move(Node node)
    {
        if (connecting_node == null)
            return;
        if (Vector2.Distance(node.transform.position, connecting_node.transform.position) < _safe_connecting_distance)
        {
            _movement.NormalRangeEvent(node);
            return;
        }
        Vector3 desired_pos = connecting_node.transform.position;
        float step = IMovementStrategy._speed * Time.deltaTime;
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
        //Debug.Log("Moving towards neighbor");
    }

    public void HandleNormalRange(Node node)
    {
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