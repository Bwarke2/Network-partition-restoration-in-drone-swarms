using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooCloseStrategy : IMovementStrategy
{
    protected List<Node> Neighbors = new List<Node>();
    public void SetNeighbors(List<Node> neighbors)
    {
        Neighbors = neighbors;
    }
    private Node GetClosestNeighbor(Node node)
    {
        Node closest = null;
        float closest_dist = Mathf.Infinity;
        foreach (Node neighbor in Neighbors)
        {
            float dist = Vector3.Distance(neighbor.transform.position, node.transform.position);
            if (dist < closest_dist)
            {
                closest_dist = dist;
                closest = neighbor;
            }
        }
        return closest;
    }
    public void Move(Node node)
    {
        //Debug.Log("Moving away from neighbor");
        if (Neighbors.Count == 0)
            return;
        Node closest = GetClosestNeighbor(node);

        //Get direction away from closest
        Vector3 dir = node.transform.position - closest.transform.position;
        //dir = dir.normalized;
        Vector3 desired_pos = node.transform.position + dir;
        float step = IMovementStrategy._speed * Time.deltaTime;
        node.transform.position = Vector2.MoveTowards(node.transform.position, desired_pos, step);
    }
}