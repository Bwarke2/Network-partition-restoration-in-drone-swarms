using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public enum ConnectedToLeader
{
    Connected,
    NotConnected,
    Leader
}

public class HeartBeat : MonoBehaviour
{
    [SerializeField]
    private int _L_ID;
    [SerializeField]
    private int _this_id;
    private Communication _com;
    private int _term = 0;
    private int _NumMembers = 6;
    public ConnectedToLeader ConnectedToLeader = ConnectedToLeader.NotConnected;
    private const float max_response_time = 3;
    private Dictionary<int, float> _heart_beat_responses = new Dictionary<int, float>();
    private IEnumerator HeartBeatTimeOut_coroutine;
    private IEnumerator HB_frequence_coroutine;

    // Leader attributes
    [SerializeField]
    private List<int> _followers = new List<int>();
    [SerializeField]
    private List<int> _lost = new List<int>();
    [SerializeField]
    private Dictionary<int,int> _numOfPartitions = new Dictionary<int, int>();

    public void Setup(int this_id, int L_id, Communication communication)
    {
        _this_id = this_id;
        _L_ID = L_id;
        _com = communication;
        HB_frequence_coroutine = HB_frequence();
        HeartBeatTimeOut_coroutine = HeartBeatTimeOut();
        StartCoroutine(HB_frequence_coroutine);
    }

    public void SetLeader(int L_id)
    {
        _L_ID = L_id;
    }

    public void SetTerm(int term)
    {
        _term = term;
    }

    public void SetNumMembers(int num_members)
    {
        _NumMembers = num_members;
    }

    public int GetNumberOfLostNodes()
    {
        return _lost.Count;
    }

    private void SendHeartBeat()
    {
        //Send HeartBeat
        //Debug.Log("Sending HeartBeat from: " + _this_id + " with term: " + _term);
        _com.BroadcastMsg<int>(MsgTypes.HeartBeatMsg, _term);
        StartCoroutine(HB_frequence_coroutine);
    }

    public void HandleHeartBeatMsg(int sender_id, string value)
    {
        int term = JsonConvert.DeserializeObject<int>(value);
        //Debug.Log("HeartBeat recieved from: " + sender_id + " with term: " + term);
        /*if (sender_id != _L_ID && ConnectedToLeader == ConnectedToLeader.Connected)
        {
            Debug.Log("HeartBeat recieved from non leader node: " + sender_id);
            //Handle other leader observed
        }*/
        ConnectedToLeader = ConnectedToLeader.Connected;
        StopCoroutine(HeartBeatTimeOut_coroutine);
        HeartBeatTimeOut_coroutine = HeartBeatTimeOut();
        StartCoroutine(HeartBeatTimeOut_coroutine);
        _com.SendMsg<int>(MsgTypes.HeartBeatResponseMsg, sender_id, _this_id);
    }

    public void HandleBroadcastWinnerMsg(string value)
    {
        int L_id = JsonConvert.DeserializeObject<int>(value);
        ConnectedToLeader = ConnectedToLeader.NotConnected;
        //Debug.Log("Recieved broadcast winner msg with leader: " + L_id);
        SetLeader(L_id);
        _term++;
        
        StopCoroutine(HB_frequence_coroutine);
        HB_frequence_coroutine = HB_frequence();
        StartCoroutine(HB_frequence_coroutine);
        if(_L_ID == _this_id)
        {
            //Debug.Log("Stopping HeartBeat timeout in node: " + _this_id);
            ConnectedToLeader = ConnectedToLeader.Leader;
        }
    }

    

    public void HandleLostNodeDroppedMsg(int sender_id, string value)
    {
        int num_members = JsonConvert.DeserializeObject<int>(value);
        _lost.Clear();
    }

    public void HandleHearthBeatResponseMsg(int Sender, string value)
    {
        int recv_id = JsonConvert.DeserializeObject<int>(value);
        if (!_heart_beat_responses.ContainsKey(recv_id))
        {
            _heart_beat_responses.Add(recv_id, Time.time);
        }
        else
        {
            _heart_beat_responses[recv_id] = Time.time;
        }
    }

    private Dictionary<int, float> UpdateResponses()
    {
        Dictionary<int, float> timely_responses = new Dictionary<int, float>();
        foreach (KeyValuePair<int, float> response in _heart_beat_responses)
        {
            if (Time.time - response.Value < max_response_time)
            {
                if (!_followers.Contains(response.Key))
                {
                    _followers.Add(response.Key);
                    if(_lost.Contains(response.Key))
                    {
                        _lost.Remove(response.Key);
                        Debug.Log("Partition restored with node: " + response.Key);
                        _com.BroadcastMsg<int>(MsgTypes.PartitionRestoredMsg, response.Key);
                        _com.SendMsg<int>(MsgTypes.PartitionRestoredMsg, _this_id, response.Key);
                    }
                    
                }
                    
                timely_responses.Add(response.Key, response.Value);
                //Debug.Log("Node: " + response.Key + " responded in time");
            }
            else
            {
                if(_followers.Contains(response.Key))
                {
                    Debug.Log("Node: " + response.Key + " did not respond in time");
                    _followers.Remove(response.Key);
                    _lost.Add(response.Key);
                    if(!_numOfPartitions.ContainsKey(response.Key))
                        _numOfPartitions.Add(response.Key, 1);
                    else
                        _numOfPartitions[response.Key]++;
                    if(_numOfPartitions[response.Key] < 10)
                    {
                        _com.BroadcastMsg<int>(MsgTypes.PartitionMsg, response.Key);
                        //Send a msg to this node that a node is lost
                        _com.SendMsg<int>(MsgTypes.PartitionMsg, _this_id, response.Key);
                    }
                    else
                    {
                        //Stop caring about node
                        _lost.Remove(response.Key);
                    }
                }
            }
        }
        timely_responses.Add(_this_id, Time.time);
        return timely_responses;
    }

    IEnumerator HeartBeatTimeOut()
    {
        yield return new WaitForSeconds(5);
        if(_L_ID != _this_id)
        {
            Debug.Log("HeartBeat timeout in node: " + _this_id);
            Node thisNode = GetComponent<Node>();
            ConnectedToLeader = ConnectedToLeader.NotConnected;
            GetComponent<Movement>().LostNodeEvent(thisNode);
        }
            
    }

    IEnumerator HB_frequence()
    {
        while (true)
        {
            yield return null;
            //Debug.Log("New HeartBeat timer in node: " + _this_id);
            if (_L_ID == _this_id)
            {
                SendHeartBeat();
                UpdateResponses();
            }
                
            yield return new WaitForSeconds(1);
        }
    }

    
}



