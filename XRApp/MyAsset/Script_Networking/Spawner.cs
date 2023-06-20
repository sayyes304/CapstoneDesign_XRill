using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using Oculus.Interaction.HandGrab;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public static Spawner instance;
    public NetworkRunner _runner;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();


    [SerializeField] private NetworkPrefabRef _playerPrefab;

    public GameObject screen;
    public string connectionID;
    public Transform[] spawnPos;
    public GameObject localUser;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        prepareARObjects();

        if (connectionID.Length == 0)
            connectionID = PlayerPrefs.GetString("SessionName");
        //connectionID = screen.GetComponent<Unity.RenderStreaming.Samples.ReceiverSampleForUs>().connectionId;


        StartGame(GameMode.AutoHostOrClient, connectionID);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (_runner.IsShutdown)    // Server
        {
            ARtoVRLoading.Instance.startLoadScene("SampleScene");
        }
        */
    }

    void OnDestroy()
    {
        // Fusion 삭제: ShutDown() 호출하면 모든 호스트에서 알아서 이 스크립트가 사라짐
        Debug.Log("회의 종료");
        ARtoVRLoading.Instance.startLoadScene("SampleScene");
    }

    async void StartGame(GameMode mode, string roomname)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        //_runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomname,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            CustomLobbyName = "LobbyTest"       // 방들을 묶어주는 로비 지정
        });

       
    }


    List<SessionInfo> sessionList;

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
       this.sessionList = sessionList;
        foreach(var s in sessionList)
        {
            Debug.Log(s.Name);
            
        }
        
    }

    


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // 테스트해보면 최초 접속자 = 9, 그 다음 0, ... 순서임.
        // 9 = 0
        // 0 = 1
        // 1 = 2
        // 2 = 3
        // (id+1%10)

        //Vector3 spawnP = spawnPos[runner.SessionInfo.PlayerCount-1].position;
        Vector3 spawnP = spawnPos[(player.PlayerId+1)%10].position;
        Quaternion spawnR = (player.PlayerId + 1) % 10 % 2 == 0 ?
            Quaternion.Euler(0, 0, 0) :  // 0, 2
            Quaternion.Euler(0, 180, 0);  // 1, 3

        spawnP = new Vector3(spawnP.x, 0, spawnP.z);

        Debug.Log(spawnP);
        Debug.Log(spawnR);

        
        //localUser.transform.SetPositionAndRotation(spawnP, spawnR);


        // 끝에 player 안 넣으면 inputAuthority가 없는 공용???
        if (!_spawnedCharacters.ContainsKey(player) && runner.IsServer)
        {
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnP + new Vector3(0,0,0), spawnR, player);
            //networkPlayerObject.gameObject.GetComponent<User>().userName = $"User {player.PlayerId}";
            networkPlayerObject.gameObject.GetComponent<User>().spawnPos = spawnP;
            networkPlayerObject.gameObject.GetComponent<User>().spawnRot = spawnR;
            networkPlayerObject.gameObject.GetComponent<User>().playerId = player.PlayerId;

            _spawnedCharacters.Add(player, networkPlayerObject);
        }





        foreach (var u in runner.ActivePlayers)
        {
            Debug.Log("Player "+u.PlayerId);
        }

        /*

        if (PlayerPrefs.GetString("326") != null && PlayerPrefs.GetString("326").Length>0)
        {
            NetworkObject o = _runner.Spawn(netPrefabs[5], new Vector3(1, 1, 1), Quaternion.identity);
            o.GetComponent<VirtualObjectNet>().RPC_data("326", PlayerPrefs.GetString("326"));
        }
        */

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

        // Find and remove the players avatar
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }




    }

    public GameObject localCenterEyeAnchor;
    public GameObject localGazePointer;


    // 이걸 host만 넣어줄 수 있다는 뜻
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        /*
        var data = new UserTransform();
        var ray = EyeTest.instance.centerEyeRay;



        data.position = localCenterEyeAnchor.transform.localPosition;
        data.gazeDirection = ray.direction;
        data.rotation = localCenterEyeAnchor.transform.localRotation;

        input.Set(data);
        */
    }


    //------------------------------------------------------------------------------------------



    #region createNetObject

    [Header("ARPrefab")]
    public GameObject padding;
    public GameObject[] panels; // 0 1 2 3 5 6
    public GameObject[] localPrefabs; // 0 1 2 3 5 6
    public NetworkPrefabRef[] netPrefabs; // 0 1 2 3 5 6
    public List<Material> colors;
    public List<Material> pureColors;

    public GameObject ARPrefabWindow;

    public static Keys keyList;    // 모든 키 리스트
    public static Dictionary<string, VirtualObjectInfo> virtualObjects;
    public Dictionary<ObjectType, List<GameObject>> VOLists;
    public void prepareARObjects()
    {
        // keys
        keyList = JsonUtility.FromJson<Keys>(PlayerPrefs.GetString("Keys"));
        if (keyList == null)
            keyList = new Keys();
        virtualObjects = new Dictionary<string, VirtualObjectInfo>();   // objID, info
        foreach (string key in keyList.keyList)
            virtualObjects.Add(key, JsonUtility.FromJson<VirtualObjectInfo>(PlayerPrefs.GetString(key)));

        //objects
        VOLists = new Dictionary<ObjectType, List<GameObject>>();
        for(int i=0; i<7; i++)
        {
            if(i!=4)
            VOLists.Add((ObjectType)i, new List<GameObject>());
        }
        
        foreach (var type in VOLists.Keys)
        {

            // 기본형
            GameObject panelD = Instantiate(padding, Vector3.zero, Quaternion.identity, panels[(int)type].transform);
            panelD.GetComponent<RectTransform>().localPosition = Vector3.zero;
            panelD.tag = "Prefab";
            GameObject objD = Instantiate(localPrefabs[(int)type], Vector3.zero, Quaternion.identity, panelD.transform);
            objD.transform.localPosition = new Vector3(0, 0, -0.02f);
            objD.transform.localPosition = Vector3.zero;
            objD.transform.localScale *= 0.3f;
            objD.GetComponent<Collider>().enabled = false;


            foreach (VirtualObjectInfo info in virtualObjects.Values)
            {
                if ((ObjectType)info.objectType == type)
                {
                    GameObject panel = Instantiate(padding, Vector3.zero, Quaternion.identity, panels[(int)type].transform);
                    panel.GetComponent<RectTransform>().localPosition = Vector3.zero;
                    panel.tag = "Prefab";
                    GameObject obj = Instantiate(localPrefabs[(int)type], Vector3.zero, Quaternion.identity, panel.transform);
                    obj.transform.localPosition = new Vector3(0,0,-0.02f);
                    obj.transform.localScale *= 0.2f;
                    obj.GetComponent<Collider>().enabled = false;



                    obj.GetComponent<VirtualObject>().info = info;
                    obj.GetComponent<MeshRenderer>().material = colors[info.color];

                    switch (info.objectType)
                    {
                        case (int)ObjectType.OBJ_TIMER:
                            obj.GetComponent<Timer>().seconds = info.timer_seconds;
                            break;
                        case (int)ObjectType.OBJ_CLOCK:
                            obj.GetComponent<Clock>().region = (Region)info.clock_region;
                            break;

                        case (int)ObjectType.OBJ_SUBSKETCH:

                            TubeRenderer tubeRenderer = obj.GetComponent<TubeRenderer>();

                            obj.GetComponent<MeshRenderer>().material = pureColors[0];   // pure yellow
                            obj.GetComponent<LineRenderer>().SetPositions(info.sketch_dots.ToArray());
                            tubeRenderer.SetRadius(info.width / 1000);       // 1000:1 즉 0.01 : 10 (반지름 기준)
                            tubeRenderer.SetPositions(info.sketch_dots.ToArray());
                            obj.GetComponent<MeshFilter>().mesh = tubeRenderer.GetMesh();
                            obj.GetComponent<MeshCollider>().sharedMesh = tubeRenderer.GetMesh();

                            break;

                        case (int)ObjectType.OBJ_NOTE:
                            obj.transform.Find("Canvas").Find("Text")
                                .GetComponent<TMPro.TMP_Text>().text = info.note_string;
                            break;
                        case (int)ObjectType.OBJ_SCREEN:
                            break;
                        case (int)ObjectType.OBJ_CUBE:
                        case (int)ObjectType.OBJ_SPHERE:
                        default:
                            break;
                    }

                    if ((ObjectType)info.objectType == type)
                    {
                        VOLists[type].Add(obj);
                    }

                }


                

                
            }
        }


    }

    // AR 물체 선택
    // run in local only
    public IEnumerator select()
    {
        CoreInteraction core = CoreInteraction.instance;



        ARPrefabWindow.SetActive(true);

        CoreInteraction.isSelecting = true;
        core.grabberL.SetActive(true); // 주먹 쥐었는지 판단
        core.grabberR.SetActive(true);

        // 둘 중 한 손이 잡을 때까지 기다림 // wait until condition is false
        //yield return new WaitUntil(() => leftInteractor.HasSelectedInteractable || rightInteractor.HasSelectedInteractable);

        // 특정 물체를 응시한 상태로 주먹을 쥘 때
        yield return new WaitUntil(() => core.leftInteractor.IsGrabbing || core.rightInteractor.IsGrabbing);


        string name;
        HandGrabInteractor interactor;
        int hand;

        if (core.leftInteractor.IsGrabbing)
        {
            interactor = core.leftInteractor;
            hand = 1;
        }

        else
        {
            interactor = core.rightInteractor;
            hand = 2;
        }
            

        interactor.ForceRelease();
        core.grabberL.SetActive(false);
        core.grabberR.SetActive(false);
        ARPrefabWindow.SetActive(false);

        CoreInteraction.isSelecting = false;

        // 다른 것
        if (!GazeHand.instance.gazedObject.CompareTag("Prefab"))
        {
            yield break;
        }


        //name = interactor.SelectedInteractable.gameObject.name;
        name = GazeHand.instance.gazedObject.transform.GetChild(0).name;
        Debug.Log("selected: " + name);

        string objID = GazeHand.instance.gazedObject.transform.GetChild(0).GetComponent<VirtualObject>().info.objectID.ToString();


        User[] users = FindObjectsByType<User>(FindObjectsSortMode.InstanceID);

        foreach (var user in users)
        {
            if(user.playerId == 9 ) // server
                user.RPC_spawn(PlayerPrefs.GetString(objID),
                    interactor.transform.position, name[..5], hand, _runner.LocalPlayer);
        }
        
       

    }

    // 선택한 AR 물체 생성
    public void spawn(string objInfo, Vector3 position, string prefix, int hand, int id)
    {
        NetworkObject obj;

        CoreInteraction core = CoreInteraction.instance;
        HandGrabInteractor interactor;
        if (hand == 1)
        {
            interactor = core.leftInteractor;
        }
        else
        {
            interactor = core.rightInteractor;
        }

        switch (prefix)
        {
            case "GCube":
                obj = _runner.Spawn(netPrefabs[0], position, Quaternion.identity, _runner.LocalPlayer);
                //interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "GSphe":
                obj = _runner.Spawn(netPrefabs[1], position, Quaternion.identity, _runner.LocalPlayer);
                //interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "GTime":
                obj = _runner.Spawn(netPrefabs[2], position, Quaternion.identity, _runner.LocalPlayer);
                //interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "GCloc":
                obj = _runner.Spawn(netPrefabs[3], position, Quaternion.identity, _runner.LocalPlayer);
                //interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            /* //일단 생략
        case "SubSk":
            obj = _runner.Spawn(netPrefabs[5], position, Quaternion.identity, _runner.LocalPlayer);
            interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
            break;
            */
            case "GNote":
                obj = _runner.Spawn(netPrefabs[6], position, Quaternion.identity, _runner.LocalPlayer);
                //interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;

            // 기존 블럭을 잡은 경우
            default:
                // 창은 사라지고 그대로 마무리
                obj = null;
                break;

        }

        if (obj != null)
            obj.GetComponent<VirtualObjectNet>().RPC_data(objInfo);

        StartCoroutine(FingerMenu.instance.tempHide()); // 숨겨주기
    }



    // 스케치 생성
    public void spawnSketch()
    {
        Debug.Log("spawnSketch");
        User[] users = FindObjectsByType<User>(FindObjectsSortMode.InstanceID);

        foreach (var user in users)
        {
            if (user.playerId == 9) // 서버만 스폰 가능
            {
                user.RPC_spawnSketch(user.playerId);
            }
        }
    }

    public void spawnSketch2(int playerId)
    {

        if (playerId == 9)
        {
            Debug.Log("spawnSketch2");
            NetworkObject subSketch;
            subSketch = _runner.Spawn(netPrefabs[5], Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
            if (subSketch != null)
            {
                subSketch.GetComponent<VirtualObjectNet>().objectType = (int)ObjectType.OBJ_SUBSKETCH;
            }
                

            User[] users = FindObjectsByType<User>(FindObjectsSortMode.InstanceID);

            foreach (var user in users)
                if(subSketch!=null)
                    user.RPC_beginSketch(subSketch.Id); // 스케치 시작
        }


    }





    #endregion


    public void beginCopyObj(GameObject selectedObject)
    {
        Debug.Log("beginCopyObj");

        if (selectedObject.GetComponent<VirtualObjectNet>() != null)
        {
            NetworkObject net = selectedObject.GetComponent<NetworkObject>();

            User[] users = FindObjectsByType<User>(FindObjectsSortMode.InstanceID);

            foreach (var user in users)
            {
                if (user.playerId == 9) // 서버만 가능
                {
                    user.RPC_copy(net);
                }
            }
        }
    }


    public void beginRemoveObj(GameObject selectedObject)
    {
        Debug.Log("beginRemoveObj");

        if (selectedObject.GetComponent<NetworkObject>() != null)
        {
            NetworkObject net = selectedObject.GetComponent<NetworkObject>();

            User[] users = FindObjectsByType<User>(FindObjectsSortMode.InstanceID);

            foreach (var user in users)
            {
                if (user.playerId == 9) // 서버만 가능
                {
                    user.RPC_remove(net);
                }
            }
        }
        else
        {
            Debug.Log("Invalid operation");
        }


    }




    //-----------------------------------------------------------------------------------------------


    public void OnConnectedToServer(NetworkRunner runner)
    {
   
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
  
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }


    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }
    
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }


    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }


}
