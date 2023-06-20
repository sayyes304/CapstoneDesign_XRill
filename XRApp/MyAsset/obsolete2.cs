using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;

public class WebRTCTest : MonoBehaviour
{
    RTCPeerConnection localConnection, remoteConnection;
    RTCDataChannel sendChannel, receiveChannel;



    // Start is called before the first frame update
    void Start()
    {
        // Create local peer
        localConnection = new RTCPeerConnection();
        sendChannel = localConnection.CreateDataChannel("sendChannel");

        sendChannel.OnOpen = () => {
            Debug.Log("OnOpen");
            //sendChannel.Send(System.Text.Encoding.UTF8.GetBytes("Hello RTC!"));
        };

        sendChannel.OnMessage = (m) => { Debug.Log("local user received: "+System.Text.Encoding.UTF8.GetString(m)); };
        sendChannel.OnClose = () => { Debug.Log("OnClose"); };

        // Create remote peer
        remoteConnection = new RTCPeerConnection();
        remoteConnection.OnDataChannel = (channel) => { Debug.Log("Channel received"); };


        // 후보 연결 등록
        localConnection.OnIceCandidate = e => { if (!string.IsNullOrEmpty(e.Candidate)) remoteConnection.AddIceCandidate(e); };
        remoteConnection.OnIceCandidate = e =>{ if (!string.IsNullOrEmpty(e.Candidate)) localConnection.AddIceCandidate(e); };


        //StartCoroutine(signalling());
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    IEnumerator signalling()
    {
        // Signalling Process
        var op1 = localConnection.CreateOffer();
        yield return op1;
        var op2 = localConnection.SetLocalDescription(ref op1.);
        yield return op2;
        var op3 = remoteConnection.SetRemoteDescription(ref op1.Desc);
        yield return op3;
        var op4 = remoteConnection.CreateAnswer();
        yield return op4;
        var op5 = remoteConnection.SetLocalDescription(op4.Desc);
        yield return op5;
        var op6 = localConnection.SetRemoteDescription(op4.Desc);
        yield return op6;

    }
    */


}
