using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;


public class TaskAssignmnet
{
    public List<Transform> Unreached_Targets = new List<Transform>();
    public List<Transform> Pursuing_Targets = new List<Transform>();
    public Dictionary<int, float> Bids = new Dictionary<int, float>();

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
        if (RecieverNode.Target != null)
        {
            return;
            /*float distance_to_current_target = Vector2.Distance(transform.position, Target.position);
            if (distance > distance_to_current_target)
            {
                //Debug.Log("Distance to target is greater than current target");
                return;
            }*/
        }
        
        //Send distance to sender
        RecieverNode.SendMsg(MsgTypes.ReturnBitMsg, sender_id, JsonConvert.SerializeObject(distance, Formatting.None,
                new JsonSerializerSettings()
                { 
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
    }

    public void AssignTasks(Node Auctioneer, List<Transform> Targets_to_assign, List<Node> Nodes_to_assign)
    {
        //Debug.Log("Assigning tasks");
        List<Transform> inputTargets = new List<Transform>();
        inputTargets.AddRange(Targets_to_assign);
        if (Unreached_Targets.Count < Nodes_to_assign.Count)
        {
            inputTargets.AddRange(Pursuing_Targets);
        }
        
        foreach (Transform un_target in inputTargets)
        {
            //Debug.Log("Auctioning task");
            // Auction task
            if (un_target != null)
                AuctionTask(Auctioneer, un_target, Nodes_to_assign);
        }
    }

    private void AuctionTask(Node Auctioneer, Transform target, List<Node> Nodes_to_assign)
    {
        // Auction task
        // Reset Bids
        Bids = new Dictionary<int, float>();
        // Send auction message to neighbours
        foreach (Node node in Nodes_to_assign)
        {
            //Change this later to send messages instead of changing variables
            Auctioneer.SendMsg(MsgTypes.AnounceAuctionMsg, node.ID, JsonConvert.SerializeObject(target.name, Formatting.None,
                        new JsonSerializerSettings()
                        { 
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        //Wait for bit

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
        Unreached_Targets.Remove(target);
        Pursuing_Targets.Add(target);

        // Send task to winner
        foreach (Node node in Nodes_to_assign)
        {
            //Change this later to send messages instead of changing variables
            if (node.ID == min_bid_id)
                Auctioneer.SendMsg(MsgTypes.SetTargetMsg, node.ID, JsonConvert.SerializeObject(target.name, Formatting.None,
                        new JsonSerializerSettings()
                        { 
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }
    }
}


