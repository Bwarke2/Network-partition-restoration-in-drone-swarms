using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LostNodePRP1 : ILostNodePRP
{
    private Movement _movement;
    public void SetMovement(Movement movement)
    {
        _movement = movement;
    }
    public void Move(Node node)
    {
        //Debug.Log("Lost node PRP 1");
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
        
    }
}