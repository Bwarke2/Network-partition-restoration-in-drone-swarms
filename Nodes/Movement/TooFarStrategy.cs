using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooFarStrategy : IMovementStrategy
{
    protected Node connecting_node = null;

    public void SetConnectingNode(Node node)
    {
        connecting_node = node;
    }
    public void Move(Node node)
    {
        if (connecting_node == null)
            return;
        Vector3 desired_pos = connecting_node.transform.position;
        float step = IMovementStrategy._speed * Time.deltaTime;
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
        //Debug.Log("Moving towards neighbor");
    }
}