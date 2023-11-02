using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ISwarmPRP : IMovementStrategy
{
    new void HandleLostNode(Node node)
    {
    }

}