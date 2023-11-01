using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetStrategy : IMovementStrategy
{
    public void Move(Node node)
    {
        //Debug.Log("Moving towards target");
        if (node.Target == null)
        {
            //Debug.Log("No target");
            return;
        }

        float step = IMovementStrategy._speed * Time.deltaTime;
        Vector2 desired_pos = node.Target.position;
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
    
        node.TargetReached();
    }
}