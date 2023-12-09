using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Communication : MonoBehaviour
{
    public List<GameObject> NB = new List<GameObject>();    //Neighbouring nodes
    public int Hops = int.MaxValue;        //Number of hops to leader
    public bool ConnectedToLeader = false; //Connected to leader
    public Node ConnnectingNeighbor;       //Connecting neighbour
    [SerializeField]
    private int NSN;             // Node start number
    private Swarm _swarm;   //Swarm object

    //Constants
    public const float com_range = 10;         //Communication range
    public const float msg_loss = 0;         //Message loss probability in percent
    
    //Metrics
    private int _num_sent_msgs = 0;

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
        if (_swarm.Leader == null)
            return;
        ConnnectingNeighbor = FindConnectingNeighbor();
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
        Node CN = null;
        foreach (GameObject node in NB)
        {
            if (node.GetComponent<Communication>().Hops < min_hop)
            {
                min_hop = node.GetComponent<Communication>().Hops;
                min_distance = Vector2.Distance(node.transform.position, transform.position);
                CN = node.GetComponent<Node>();
            }
            if (node.GetComponent<Communication>().Hops == min_hop)
            {
                if (Vector2.Distance(node.transform.position, transform.position) < min_distance)
                {
                    min_distance = Vector2.Distance(node.transform.position, transform.position);
                    CN = node.GetComponent<Node>();
                }  
            }
        }
        return CN;
    }

    public void SendMsg<T>(MsgTypes msg_type, int recv_id, T value)
    {
        Node sender_node = GetComponent<Node>();
        string value_to_send = JsonConvert.SerializeObject(value, Formatting.None,
                        new JsonSerializerSettings()
                        { 
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
        foreach (Node node in _swarm.GetMembers())
        {
            //Debug.Log("Checking node: " + node.ID);
            if (node.ID == recv_id)
            {
                _num_sent_msgs++;
                ReceiveMsg(node, msg_type, sender_node.ID, value_to_send);
                return;
            }
        }
        Debug.Log("No reciever mached id: " + recv_id);
    }

    public void BroadcastMsg<T>(MsgTypes msg_type, T value)
    {
        Node sender_node = GetComponent<Node>();
        string value_to_send = JsonConvert.SerializeObject(value, Formatting.None,
                        new JsonSerializerSettings()
                        { 
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
        //Debug.Log("Broadcasting to " + _swarm.GetMembers().Count + " nodes");
        foreach (Node node in _swarm.GetMembers())
        {
            //Debug.Log("Sendt broadcast to node: " + node.ID);
            if (node.ID != sender_node.ID)
            {
                _num_sent_msgs++;
                ReceiveMsg(node, msg_type, sender_node.ID, value_to_send);
            }
        }
    }

    public void ReceiveMsg(Node Recieving_node, MsgTypes msg_type, int sender_id, string value)
    {
        // Simulate unreliable communication
        if (Random.Range(0, 100) < msg_loss)
        {
            Debug.Log("Message lost in " + Recieving_node.name + " of type: " + msg_type);
            return;
        }
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
            case MsgTypes.LostNodeDroppedMsg:
                Recieving_node.LostNodeDroppedMsgHandler(sender_id, value);
                break;  
            case MsgTypes.SwarmStuckMsg:
                Recieving_node.NoSwarmMovementMsgHandler(sender_id, value);
                break;
            case MsgTypes.ElectionMsg:
                Recieving_node.ElectionMsgHandler(sender_id, value);
                break;
            case MsgTypes.VoteMsg:
                Recieving_node.VoteMsgHandler(sender_id, value);
                break;
            case MsgTypes.HeartBeatMsg:
                Recieving_node.HeartBeatMsgHandler(sender_id, value);
                break;
            case MsgTypes.HeartBeatResponseMsg:
                Recieving_node.HeartBeatResponseMsgHandler(sender_id, value);
                break;
            case MsgTypes.PartitionMsg:
                Recieving_node.PartitionMsgHandler(sender_id, value);
                break;
            case MsgTypes.PartitionRestoredMsg:
                Recieving_node.PartitionRestoredMsgHandler(sender_id, value);
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

    public void SetNSN(int nsn)
    {
        NSN = nsn;
    } 

    public int GetNumSentMsgs()
    {
        return _num_sent_msgs;
    }
}
