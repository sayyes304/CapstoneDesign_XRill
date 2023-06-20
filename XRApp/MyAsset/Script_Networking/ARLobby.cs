using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ARLobby : MonoBehaviour, INetworkRunnerCallbacks
{

    NetworkRunner networkRunner;
    public static List<SessionInfo> sessionList;
    public static bool shouldBeChanged = false;

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("OnSceneLoadDone");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("OnSceneLoadStart");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("OnConnectedToServer");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("OnPlayerJoined");

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("OnPlayerLeft");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) // 로비 모드에서만 실행됨!
    {
        Debug.Log("OnSessionListUpdated");
        Debug.Log(sessionList.Count);
        ARLobby.sessionList = sessionList;
        foreach (var session in sessionList)
            Debug.Log(session.Name);

        shouldBeChanged = true;
    }

    async void StartGame(GameMode mode, string roomname)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        networkRunner = gameObject.AddComponent<NetworkRunner>();
        networkRunner.ProvideInput = true;

        // 로비는 방을 그룹지어줌!!!
        await networkRunner.JoinSessionLobby(SessionLobby.Custom, "LobbyTest");


    }


    // Start is called before the first frame update
    void Start()
    {

        StartGame(GameMode.AutoHostOrClient, UserManagement.loginedUserID_s);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnDestroy()
    {
        
    }
    // --------------------


    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        Debug.Log("OnReliableDataReceived");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("OnConnectFailed");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("OnConnectRequest");
    }


    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        Debug.Log("OnCustomAuthenticationResponse");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("OnDisconnectedFromServer");
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("OnHostMigration");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
       // Debug.Log("OnInput");
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        Debug.Log("OnInputMissing");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("OnShutdown");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        Debug.Log("OnUserSimulationMessage");
    }


}
