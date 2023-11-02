using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Communication : MonoBehaviour
{
    public List<GameObject> NB = new List<GameObject>();    //Neighbouring nodes
    public int Hops = int.MaxValue;        //Number of hops to leader
    public bool ConnectedToLeader = false; //Connected to leader
    public Node ConnnectingNeighbor;       //Connecting neighbour
    private int NSN;             // Node start number

    private Swarm _swarm;
    //Constants
    public const float com_range = 10;         //Communication range
    // Start is called before the first frame update

    void Awake()
    {
        _swarm = GameObject.FindGameObjectWithTag("Swarm").GetComponent<Swarm>();
        NSN = GameObject.FindGameObjectsWithTag("Node").Length;
        if (name == "Node_leader")
        {
            Hops = 0;
            ConnectedToLeader = true;
        }
    }

    void Update()
    {
        FindConnectingNeighbor();
        Hops = FindHopsToLeader();
    }

    public void UpdateNeighbours()
    {
        //Update neighbours
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node"); //Find all nodes
        NB = new List<GameObject>(); //Clear neighbour list
        foreach (GameObject node in nodes)
        {
            if (node.name != name && (Vector2.Distance(node.transform.position, transform.position) < com_range))
            {
                NB.Add(node); //Add node to neighbour list
            }
        }
    }

    public int FindHopsToLeader()
    {
        //Find min hop count to leader among neighbors
        if (GetComponent<Node>().GetInstanceID() == _swarm.Leader.GetInstanceID())
            return Hops = 0;
        
        int min_hop = int.MaxValue;
        foreach (GameObject node in NB)
        {
            if (node.GetComponent<Communication>().Hops < min_hop)
            {
                min_hop = node.GetComponent<Communication>().Hops;
            }
        }

        if (min_hop >= NSN)
        {
            //Debug.Log("No connection to leader");
            ConnectedToLeader = false;
            _swarm.RemoveMember(GetComponent<Node>());
            min_hop = int.MaxValue;
            return Hops = min_hop;
        }
        
        ConnectedToLeader = true;
        _swarm.AddMember(GetComponent<Node>());
        return Hops = min_hop + 1;
    }

    private Node FindConnectingNeighbor()
    {
        float min_distance = float.MaxValue;
        int min_hop = int.MaxValue;
        foreach (GameObject node in NB)
        {
            if (node.GetComponent<Communication>().Hops < min_hop)
            {
                min_hop = node.GetComponent<Communication>().Hops;
                min_distance = Vector2.Distance(node.transform.position, transform.position);
                ConnnectingNeighbor = node.GetComponent<Node>();
            }
            if (node.GetComponent<Communication>().Hops == min_hop)
            {
                if (Vector2.Distance(node.transform.position, transform.position) < min_distance)
                {
                    min_distance = Vector2.Distance(node.transform.position, transform.position);
                    ConnnectingNeighbor = node.GetComponent<Node>();
                }  
            }
        }
        return ConnnectingNeighbor;
    }

    public void SendMsg(Node sender_node, MsgTypes msg_type, int recv_id, string value)
    {
        foreach (Node node in _swarm.GetMembers())
        {
            //Debug.Log("Checking node: " + node.ID);
            if (node.ID == recv_id)
            {
                ReceiveMsg(node, msg_type, sender_node.ID, value);
                return;
            }
        }
        Debug.Log("No reciever mached id: " + recv_id);
    }

    public void ReceiveMsg(Node Recieving_node, MsgTypes msg_type, int sender_id, string value)
    {
        // Handle unreliable communication
        // Recieve msg
        switch (msg_type)
        {
            case MsgTypes.SetTargetMsg:
                Recieving_node.SetTargetMsgHandler(sender_id, value);
                break;
            case MsgTypes.TargetReachedMsg:
                Recieving_node.TargetReachedMsgHandler(sender_id, value);
                break;
            case MsgTypes.SetRPMsg:
                //Debug.Log(name + " recieved RP: " + JsonConvert.DeserializeObject<Vector2>(value));
                Recieving_node.SetRPMsgHandler(sender_id, value);
                break;
            case MsgTypes.RollCallMsg:
                Recieving_node.RollCallRecvHandler(sender_id, value);
                break;
            case MsgTypes.RollCallResponseMsg:
                //To do
                break;
            case MsgTypes.AnounceAuctionMsg:
                Recieving_node.AnounceAuctionMsgHandler(sender_id, value);
                break;
            case MsgTypes.ReturnBitMsg:
                //Debug.Log("Recieved bit from: " + sender_id + " with value: " + value);
                Recieving_node.ReturnBitMsgHandler(sender_id, value);
                break;
            case MsgTypes.Broadcast_WinnerMsg:
                Recieving_node.BroadcastWinnerMsgHandler(sender_id, value);
                break;
        }
    }

    public List<Node> GetPossibleConnections()
    {
        List<Node> possible_connections = new List<Node>();
        foreach (GameObject node in NB)
        {
            if (node.GetComponent<Communication>().Hops <= Hops)
            {
                possible_connections.Add(node.GetComponent<Node>());
            }
        }
        return possible_connections;
    }

    public Node GetConnectingNeighbor()
    {
        return ConnnectingNeighbor;
    }

    public List<GameObject> GetNeighbours()
    {
        return NB;
    }

    public int GetNSN()
    {
        return NSN;
    }
}
