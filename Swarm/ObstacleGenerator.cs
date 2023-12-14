using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ObstacleGenerator : MonoBehaviour
{
    public GameObject ObstaclePrefab;
    public Transform ObstacleParent;
    public int min_x = -10;
    public int max_x = 10;
    public int min_y = -10;
    public int max_y = 10;

    public List<GameObject> GenerateRandomObstacle(int numObstacle)
    {
        List<GameObject> targets = new List<GameObject>();
        for (int i = 0; i < numObstacle; i++)
        {
            GameObject target = GenerateRandomObstacle();
            target.name = "Obstacle " + i;
            targets.Add(target);
        }
        return targets;
    }

    GameObject GenerateRandomObstacle()
    {
        float x = Random.Range(min_x, max_x);
        float y = Random.Range(min_y, max_y);
        Vector3 newposition = new Vector3(x, y,0);
        Quaternion rotation = Quaternion.Euler(0,0,Random.Range(0,360));
        GameObject target = CreateObstacle(newposition,rotation);
        if (target == null)
            return GenerateRandomObstacle();
        return target;
    }

    GameObject CreateObstacle(Vector3 newposition,Quaternion rotation)
    {
        if(Physics.CheckSphere(newposition, 2f))
        {
            Debug.Log("Obstacle is too close to another target");
            return null;
        }
        GameObject newObstacle = Instantiate(ObstaclePrefab, newposition,rotation,ObstacleParent);
        newObstacle.tag = "Obstacle";
        return newObstacle;
    }
}