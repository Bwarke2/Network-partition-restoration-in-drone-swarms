using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ILostNodePRP : IMovementStrategy
{
    void HandleLostNode(Node node)
    {
    }
}