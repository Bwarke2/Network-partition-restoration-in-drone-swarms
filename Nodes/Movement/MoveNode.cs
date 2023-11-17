using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MoveNode
{
    private const float _speed = 1; //Movement speed

    public static void MoveToward(Node nodeToMove, Vector2 desiredPos)
    {
        float step = _speed * Time.deltaTime;
        nodeToMove.transform.position = Vector2.MoveTowards(nodeToMove.transform.position, desiredPos, step);
    }
}