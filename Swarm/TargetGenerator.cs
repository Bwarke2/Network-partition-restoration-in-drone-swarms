using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TargetGenerator : MonoBehaviour
{
    public GameObject TargetPrefab;
    public Transform TargetParent;
    public int min_x = -10;
    public int max_x = 10;
    public int min_y = -10;
    public int max_y = 10;

    public List<GameObject> GenerateRandomTargets(int numTargets)
    {
        List<GameObject> targets = new List<GameObject>();
        for (int i = 0; i < numTargets; i++)
        {
            GameObject target = GenerateRandomTarget();
            target.name = "Target " + i;
            targets.Add(target);
        }
        return targets;
    }

    GameObject GenerateRandomTarget()
    {
        float x = Random.Range(min_x, max_x);
        float y = Random.Range(min_y, max_y);
        Vector3 newposition = new Vector3(x, y,0);
        GameObject target = CreateTarget(newposition);
        if (target == null)
            return GenerateRandomTarget();
        return target;
    }

    GameObject CreateTarget(Vector3 newposition)
    {
        if(Physics.CheckSphere(newposition, 2f))
        {
            Debug.Log("Target is too close to another target");
            return null;
        }
        GameObject newTarget = Instantiate(TargetPrefab, newposition,Quaternion.identity,TargetParent);
        newTarget.tag = "Target";
        return newTarget;
    }
}