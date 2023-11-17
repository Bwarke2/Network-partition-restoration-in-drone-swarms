using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class LostNodePRP2_returnpath : LostNodePRP2
{
    public override Vector3 GetDesiredPosition(Node node)
    {
        //Debug.Log("Lost node PRP 2 _returnpath");
        
        if (node.RP == null)
            return node.transform.position;
        
        Vector3 desired_pos = _movement.Path.Last();
        //If node is at desired pos
        if (Vector2.Distance(node.transform.position, desired_pos) < 0.1f)
        {
            Debug.Log("Node " + node.ID + " reached last position");
            _movement.Path.RemoveAt(_movement.Path.Count - 1);
            _movement.SetStrategy(new LostNodePRP2());
        }
        return desired_pos;
    }

    public override void HandleNoMovement(Node node)
    {
        //Do nothing
    }
}