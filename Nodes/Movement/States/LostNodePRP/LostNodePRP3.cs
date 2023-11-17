using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LostNodePRP3 : ILostNodePRP
{
    private Movement _movement;
    public void SetMovement(Movement movement)
    {
        _movement = movement;
    }
    public Vector3 GetDesiredPosition(Node node)
    {
        //Debug.Log("Lost node PRP 3");
        if (node.RP == null)
        {
            //Debug.Log("No target");
            return node.transform.position;
        }

        return node.RP;   
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