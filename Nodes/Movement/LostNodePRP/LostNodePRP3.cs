using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LostNodePRP3 : ILostNodePRP
{
    public void Move(Node node)
    {
        Debug.Log("Lost node PRP 3");
        if (node.RP == null)
        {
            //Debug.Log("No target");
            return;
        }

        float step = IMovementStrategy._speed * Time.deltaTime;
        Vector2 desired_pos = node.RP;
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
        
        //Do nothing
    }
}