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
    public void Move(Node node)
    {
        //Debug.Log("Lost node PRP 3");
        if (node.RP == null)
        {
            //Debug.Log("No target");
            return;
        }

        float step = IMovementStrategy._speed * Time.deltaTime;
        Vector2 desired_pos = node.RP;
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
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
}