using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IMovementStrategy
{
    protected const float _speed = 1; //Movement speed

    public void SetConnectingNode(Node node)
    {
    }
    public void SetNeighbors(List<Node> neighbors)
    {
    }
    void Move(Node node);
}