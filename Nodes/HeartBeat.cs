using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class HeartBeat : MonoBehaviour
{
    [SerializeField]
    private int _L_ID;
    [SerializeField]
    private int _this_id;
    private Communication _com;
    private int _term = 0;
    private const float max_response_time = 3;
    private Dictionary<int, float> _heart_beat_responses = new Dictionary<int, float>();
    private IEnumerator HeartBeatTimeOut_coroutine;
    private IEnumerator NewHeartBeatTimer_coroutine;

    // Leader attributes
    [SerializeField]
    private List<int> _followers = new List<int>();

    public void Setup(int this_id, int L_id, Communication communication)
    {
        _this_id = this_id;
        _L_ID = L_id;
        _com = communication;
        NewHeartBeatTimer_coroutine = NewHeartBeatTimer();
        HeartBeatTimeOut_coroutine = HeartBeatTimeOut();
        StartCoroutine(NewHeartBeatTimer_coroutine);
    }

    public void SetLeader(int L_id)
    {
        _L_ID = L_id;
    }

    public void SetTerm(int term)
    {
        _term = term;
    }

    private void SendHeartBeat()
    {
        //Send HeartBeat
        //Debug.Log("Sending HeartBeat from: " + _this_id + " with term: " + _term);
        _com.BroadcastMsg<int>(MsgTypes.HeartBeatMsg, _term);
        StartCoroutine(NewHeartBeatTimer_coroutine);
    }

    public void HandleHeartBeatMsg(int sender_id, string value)
    {
        int term = JsonConvert.DeserializeObject<int>(value);
        //Debug.Log("HeartBeat recieved from: " + sender_id + " with term: " + term);
        if (sender_id != _L_ID)
        {
            Debug.Log("HeartBeat recieved from non leader node: " + sender_id);
            //Handle other leader observed
            return;
        }
        //Debug.Log("Stopping HeartBeat timeout in node: " + _this_id);
        StopCoroutine(HeartBeatTimeOut_coroutine);
        HeartBeatTimeOut_coroutine = HeartBeatTimeOut();
        StartCoroutine(HeartBeatTimeOut_coroutine);
        _com.SendMsg<int>(MsgTypes.HeartBeatResponseMsg, _L_ID, _this_id);
    }

    public void HandleBroadcastWinnerMsg(string value)
    {
        int L_id = JsonConvert.DeserializeObject<int>(value);
        //Debug.Log("Recieved broadcast winner msg with leader: " + L_id);
        SetLeader(L_id);
        _term++;
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
                    _com.BroadcastMsg<int>(MsgTypes.PartitionRestoredMsg, response.Key);
                }
                    
                timely_responses.Add(response.Key, response.Value);
                //Debug.Log("Node: " + response.Key + " responded in time");
            }
            else
            {
                Debug.Log("Node: " + response.Key + " did not respond in time");
                if(_followers.Contains(response.Key))
                {
                    _followers.Remove(response.Key);
                    _com.BroadcastMsg<int>(MsgTypes.PartitionMsg, response.Key);
                }
            }
        }
        timely_responses.Add(_this_id, Time.time);
        return timely_responses;
    }

    private void RemoveLateResponses()
    {
        Dictionary<int, float> responses = UpdateResponses();
        if (responses.Count > 0)
        {
            Debug.Log("Responses: " + responses.Count);
        }
    }

    IEnumerator HeartBeatTimeOut()
    {
        yield return new WaitForSeconds(5);
        if(_L_ID != _this_id)
        {
            Debug.Log("HeartBeat timeout in node: " + _this_id);
            Node thisNode = GetComponent<Node>();
            GetComponent<Movement>().LostNodeEvent(thisNode);
        }
            
    }

    IEnumerator NewHeartBeatTimer()
    {
        while (true)
        {
            yield return null;
            //Debug.Log("New HeartBeat timer in node: " + _this_id);
            if (_L_ID == _this_id)
            {
                SendHeartBeat();
                RemoveLateResponses();
            }
                
            yield return new WaitForSeconds(1);
        }
    }

    
}



