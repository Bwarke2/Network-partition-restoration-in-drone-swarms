using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class LostNodePRP3_returnpath : LostNodePRP2
{
    public override void Move(Node node)
    {
        //Debug.Log("Lost node PRP 2 _returnpath");
        
        if (node.RP == null)
            return;
        
        float step = IMovementStrategy._speed * Time.deltaTime;
        Vector2 desired_pos = _movement.Path.Last();
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
        
        //If node is at desired pos
        if (Vector2.Distance(node.transform.position, desired_pos) < 0.1f)
        {
            Debug.Log("Node " + node.ID + " reached last position");
            _movement.Path.RemoveAt(_movement.Path.Count - 1);
            _movement.SetStrategy(new LostNodePRP3());
        }
    }

    public override void HandleNoMovement(Node node)
    {
        //Do nothing
    }
}