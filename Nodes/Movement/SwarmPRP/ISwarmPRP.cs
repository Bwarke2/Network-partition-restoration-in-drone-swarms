using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ISwarmPRP : IMovementStrategy
{
    void HandleLostNode(Node node)
    {
    }

}