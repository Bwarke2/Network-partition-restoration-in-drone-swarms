using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmPRP : ISwarmPRP
{
    private Movement _movement;
    public void SetMovement(Movement movement)
    {
        _movement = movement;
    }
    public void Move(Node node)
    {
        Debug.Log("Swarm PRP");
        
        //Move toward RP
        if (node.RP == null)
            return;

        float step = IMovementStrategy._speed * Time.deltaTime;
        Vector2 desired_pos = node.RP;
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
    }

    new public void HandlePartitionRestored(Node node)
    {
        if (node.Target == null)
        {
            _movement.SetStrategy(new NoTargetStrategy());
            return;
        }
        _movement.SetStrategy(new TargetStrategy());
    }
}