using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ILostNodePRP : IMovementStrategy
{
    new void HandleLostNode(Node node)
    {
    }
}