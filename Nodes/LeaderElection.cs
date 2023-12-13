using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;


public class LeaderElection
{
    private int Term = 0;            //Term
    private int L_id = 0;            //Leader ID
    private Communication _com = null;
    private Dictionary<int, float> Votes = new Dictionary<int, float>();
    private int _id = 0;
    private Swarm _swarm;
    public bool Do_elctions = true;

    public void Startup(Communication com, int id, bool do_elections)
    {
        _com = com;
        _swarm = GameObject.FindGameObjectWithTag("Swarm").GetComponent<Swarm>();
        _id = id;
        Do_elctions = do_elections;
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
        Term = New_Term;
        Votes.Clear();
        Votes.Add(_id, GetFitness());
        //Broadcast election message
        _com.BroadcastMsg<int>(MsgTypes.ElectionMsg, New_Term);
        //Start timer for election
        _com.StartCoroutine(EndElection(nodes_participating));
    }

    public IEnumerator EndElection(List<Node> nodes_participating)
    {
        //Wait for election to end
        yield return null;
        //Check if leader is elected
        float Best_Fitness = 0;
        foreach( KeyValuePair<int, float> vote in Votes)
        {
            //Debug.Log("Vote from: " + vote.Key + " with fitness: " + vote.Value);
            if (vote.Value > Best_Fitness)
            {
                Best_Fitness = vote.Value;
                L_id = vote.Key;
            }
        }
        Debug.Log("Best fitness: " + Best_Fitness + " from: " + L_id);
        Broadcast_Winner(Term, nodes_participating, L_id);
    }

    public void HandleVoteMsg(int sender_id, string value)
    {
        //Debug.Log("Handling vote message");
        float F_l = JsonConvert.DeserializeObject<float>(value);
        //Debug.Log("Vote recived from: " + sender_id + " with fitness: " + F_l);
        Votes.Add(sender_id, F_l);
    }

    public void HandleElectionMsg(int sender_id, string value)
    {
        //Debug.Log("Handling election message");
        int new_term = JsonConvert.DeserializeObject<int>(value);
        if (new_term > Term)
        {
            Term = new_term;
            //Debug.Log("New term is: " + Term);
            //Broadcast election message
            Vote(sender_id);
        }
    }

    public void Vote(int sender_id)
    {
        //Vote for leader
        float F_l = GetFitness();
        //Debug.Log("Sending vote to: " + sender_id + " with fitness: " + F_l);
        // Send vote to sender
        _com.SendMsg<float>(MsgTypes.VoteMsg, sender_id, GetFitness());
    }

    private float GetFitness()
    {
        //Find who to vote for
        float alpha = 0.25f;
        float beta = 0.25f;
        float gamma = 0;    //Bat not implemented
        float delta = 0;    //Type_value not implemented
        int NB_length = _com.GetNeighbours().Count;
        float E_bat = 100;  //Bat not implemented
        float Type_value = 1;//Type_value not implemented
        
        float F_l = alpha * NB_length + beta * Connectivity(_id,NB_length) + gamma * E_bat + delta * Type_value;
        return F_l;
    }

    private float Connectivity(int this_id, int num_neighbors)
    {
        if (num_neighbors == 0)
        {
            Debug.Log("No neighbours");
            return 0;
        }

        //Find connectivity
        float connectivity = 0;

        foreach (GameObject node in _com.GetNeighbours())
        {
            connectivity += Vector2.Distance(_com.transform.position, node.transform.position);
        }
        
        return connectivity/num_neighbors;
    }

    private void Broadcast_Winner(int new_term, List<Node> nodes_participating, int winner_id)
    {
        //Debug.Log("Broadcasting winner");
        Term = new_term;
        L_id = winner_id;
        _swarm.SetLeader(L_id, Term);
        //Debug.Log("New leader is: " + L_id);
        //Broadcast winner to neighbours
        
        _com.GetComponent<HeartBeat>().SetLeader(L_id);
        _com.BroadcastMsg<int>(MsgTypes.Broadcast_WinnerMsg, L_id);
    }

    public void HandleBroadcastWinnerMsg(int sender_id, string value)
    {
        L_id = JsonConvert.DeserializeObject<int>(value);
    }
}