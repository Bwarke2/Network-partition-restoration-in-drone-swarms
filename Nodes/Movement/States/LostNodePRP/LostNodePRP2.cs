using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LostNodePRP2 : ILostNodePRP
{
    protected Movement _movement;
    public void SetMovement(Movement movement)
    {
        _movement = movement;
    }
    public virtual void Move(Node node)
    {
        //Debug.Log("Lost node PRP 2");
        if (node.RP == null)
            return;

        float step = IMovementStrategy._speed * Time.deltaTime;
        Vector2 desired_pos = node.RP;
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
        
        //Do nothing
    }

    public void HandleNormalRange(Node node)
    {
        //Debug.Log("Lost node PRP 1");
        if (_movement.GetTarget() == null)
        {
            _movement.SetStrategy(new NoTargetStrategy());
            return;
        }
        _movement.SetStrategy(new TargetStrategy());
    }

    public void HandlePartitionRestored(Node node)
    {
        if (_movement.GetTarget() == null)
        {
            _movement.SetStrategy(new NoTargetStrategy());
            return;
        }
        _movement.SetStrategy(new TargetStrategy());
    }

    public virtual void HandleNoMovement(Node node)
    {
        //Go to last position instead of RP
        //Debug.Log("No movement in node " + node.ID + " going to last position");
        if (_movement.Path.Count == 0)
        {
            //Debug.Log("Path is empty");
            return;
        }
        _movement.SetStrategy(new LostNodePRP2_returnpath());
    }
}