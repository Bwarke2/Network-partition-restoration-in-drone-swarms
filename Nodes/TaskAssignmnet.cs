using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

public struct Pursuing_Targets_struct
{
    public Transform target;
    public float assign_time;
}
public class TaskAssignmnet
{
    private List<Transform> Unreached_Targets = new List<Transform>();
    private List<Pursuing_Targets_struct> Pursuing_Targets = new List<Pursuing_Targets_struct>();
    private Dictionary<int, float> Bids = new Dictionary<int, float>();
    

    public void Setup()
    {
        Swarm swarm = GameObject.FindGameObjectWithTag("Swarm").GetComponent<Swarm>();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Target"))
            Unreached_Targets.Add(go.transform);

            //Sort targets based on distance to node start position
        Unreached_Targets.Sort(delegate (Transform a, Transform b)
        {
            return Vector2.Distance(a.position, swarm.FindCenterPositionOfSwarm()).CompareTo(Vector2.Distance(b.position, swarm.FindCenterPositionOfSwarm()));
        });
    }

    public bool RemovePursuingTarget(Transform target)
    {
        bool res = false;
        List<Pursuing_Targets_struct> targets_to_remove = new List<Pursuing_Targets_struct>();
        foreach (Pursuing_Targets_struct PT in Pursuing_Targets)
            if (PT.target == target)
                targets_to_remove.Add(PT);
        foreach (Pursuing_Targets_struct PT in targets_to_remove)
            res = Pursuing_Targets.Remove(PT);
        return res;
    }

    public void AddBid(int bidder_id, float bid)
    {
        Bids.Add(bidder_id, bid);
    }

    public void HandleAnounceAuctionMsg(Node RecieverNode, int sender_id, string value)
    {
        //Debug.Log("Recieved auction message from: " + sender_id + " with value: " + value);
        Transform target_to_auction = GameObject.Find(JsonConvert.DeserializeObject<string>(value)).transform;
        //Find distance to target
        float distance = Vector2.Distance(RecieverNode.transform.position, target_to_auction.position);
        if (RecieverNode.GetComponent<Movement>().GetTarget() != null)
        {
            //Debug.Log("Node: " + RecieverNode.name + " is already pursuing a target");
            return;
        }
        
        //Send distance to sender
        RecieverNode.GetComponent<Communication>().SendMsg<float>(RecieverNode, MsgTypes.ReturnBitMsg, sender_id, distance);
    }

    public void AssignTasks(Node Auctioneer, List<Node> Nodes_to_assign)
    {
        //Debug.Log("Node: " + Auctioneer.name + " is auctioning tasks");
        List<Transform> inputTargets = new List<Transform>();
        inputTargets = DecideTasks(Pursuing_Targets, Unreached_Targets);

        foreach (Transform un_target in inputTargets)
        {
            // Auction task
            if (un_target != null)
                AuctionTask(Auctioneer, un_target, Nodes_to_assign);
            else
                Debug.Log("Target is null, input length = " + inputTargets.Count);
        }

        foreach (Transform target in Unreached_Targets)
        {
            if (target == null)
                Debug.Log("Failed to assign all nodes");
        }
    }

    private List<Transform> DecideTasks(List<Pursuing_Targets_struct> currentTasks, List<Transform> unsheduledTasks, float reshedule_time = 10f)
    {
        List<Transform> tasks = new List<Transform>();
        List<Pursuing_Targets_struct> resheduledTasks = new List<Pursuing_Targets_struct>();
        //Debug.Log("Current tasks: " + currentTasks.Count + " Unsheduled tasks: " + unsheduledTasks.Count);
        foreach (Pursuing_Targets_struct PT in currentTasks)
        {
            if (Time.time - PT.assign_time > reshedule_time)
            {
                tasks.Add(PT.target);
                resheduledTasks.Add(PT);
            }
        }
        //Debug.Log("Resheduled tasks: " + resheduledTasks.Count);
        tasks.AddRange(unsheduledTasks);
        //Debug.Log("Total tasks: " + tasks.Count);
        /*int count = 0;
        foreach (Transform task in tasks)
        {
            count++;
            if (task == null)
                Debug.Log("Task: " + count + " is null");
        }

        if(tasks.Count > _swarm.RemainingTargets.Count)
            Debug.Log("More tasks (" + tasks.Count+ ") than targets: " + _swarm.RemainingTargets.Count);
        */
        return tasks;
        
    }

    private void AuctionTask(Node Auctioneer, Transform target, List<Node> Nodes_to_assign)
    {
        // Auction task
        Bids = new Dictionary<int, float>(); //Reset bids
        // Send auction message to neighbours
        foreach (Node node in Nodes_to_assign)
        {
            Auctioneer.GetComponent<Communication>().SendMsg<string>(Auctioneer, MsgTypes.AnounceAuctionMsg, node.ID, target.name);
        }
        
        ConcludeAuction(Auctioneer,target,Nodes_to_assign);
    }

    private void ConcludeAuction(Node Auctioneer, Transform target, List<Node> Nodes_to_assign)
    {
        if (Bids.Count == 0)
        {
            //Debug.Log("No bids recieved");
            return;
        }

        float min_bid = float.MaxValue;
        int min_bid_id = 0;
        foreach (KeyValuePair<int, float> bit in Bids)
        {
            //Debug.Log("Bid: " + bit.Value + " from: " + bit.Key);
            if (bit.Value < min_bid)
            {
                min_bid = bit.Value;
                min_bid_id = bit.Key;
            }
        }

        //Debug.Log("Min bid: " + min_bid + " from: " + min_bid_id);
        bool target_already_assigned = false;
        foreach (Pursuing_Targets_struct PT in Pursuing_Targets)
        {
            if (PT.target == target)
            {
                target_already_assigned = true;
            }
        }
        if (!target_already_assigned)
        {
            Pursuing_Targets_struct target_struct = new Pursuing_Targets_struct();
            target_struct.target = target;
            target_struct.assign_time = Time.time;
            Pursuing_Targets.Add(target_struct);
            if (!Unreached_Targets.Remove(target))
                Debug.Log("Failed to remove target from unreached list: " + target.name);
        }
        
        Auctioneer.GetComponent<Communication>().SendMsg<string>(Auctioneer, MsgTypes.SetTargetMsg, min_bid_id, target.name);
    }
}


