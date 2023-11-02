using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;


public class LeaderElection
{
    private int Term = 0;            //Term
    private int L_id = 0;            //Leader ID
    private Communication _com = null;

    public void Startup(Communication com)
    {
        _com = com;
    }

    public void LeaderStart(int leader_id)
    {
        L_id = leader_id;
    }

    public int GetLeaderID()
    {
        return L_id;
    }

    public int GetTerm()
    {
        return Term;
    }

    public void StartElection(List<Node> nodes_participating)
    {
        //Start election
        //Debug.Log("Starting election");
        int New_Term = Term + 1;
        //Broadcast result
        int winner = L_id;
        Broadcast_Winner(New_Term, nodes_participating, winner);
    }

    public void Vote()
    {
        //Vote for leader
    }

    private void Broadcast_Winner(int new_term, List<Node> nodes_participating, int winner_id)
    {
        //Debug.Log("Broadcasting winner");
        if (new_term <= Term)
        {
            return;
        }
        Term = new_term;
        L_id = winner_id;
        //Debug.Log("New leader is: " + L_id);
        //Broadcast winner to neighbours
        foreach (Node node in nodes_participating)
        {
            //Change this later to send messages instead of changing variables
            //Debug.Log("Broadcasting winner to: " + node.name);
            _com.SendMsg(node, MsgTypes.Broadcast_WinnerMsg, node.ID, JsonConvert.SerializeObject(L_id, Formatting.None,
                        new JsonSerializerSettings()
                        { 
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
            //Debug.Log("Node " + ID + " broadcasting winner: " + L_id + " with term: " + Term);
        }
    }

    public void HandleBroadcastWinnerMsg(int sender_id, string value)
    {
        L_id = JsonConvert.DeserializeObject<int>(value);
        //Debug.Log("New leader is: " + L_id);
    }
}