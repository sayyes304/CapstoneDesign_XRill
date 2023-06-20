using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class VirtualObjectNet : NetworkBehaviour
{

    //public static VirtualObjectNet localModifiedObj;

   // NetworkObject obj;
    [Networked] public int playerID { get; set; }
    [Networked] public Vector3 pos { get; set; }
    [Networked] public Quaternion rot { get; set; }
    [Networked] public Vector3 scale { get; set; }


    [Networked] public int objectType { get; set; }
    [Networked(OnChanged = nameof(ChangedColor))] public int color { get; set; }

    //[Networked] public NetworkBool isGrabbing { get; set; }


    // VirtualObjectInfo type specific data --------------


    [Networked(OnChanged = nameof(ChangedTimer))] public int timer_seconds { get; set; }
    [Networked(OnChanged = nameof(ChangedTimerTime))] public int current_seconds { get; set; }
    [Networked(OnChanged = nameof(ChangedClock))] public int clock_region { get; set; }
    [Networked(OnChanged = nameof(ChangedNote))] public NetworkString<_512> note_string { get; set; } 
    [Networked, Capacity(300)] public NetworkArray<Vector3> sketch_dots => default;
    [Networked] public float width { get; set; }

    public Material pureBlack;

    // end of VOInfo


    // 생성 직후, 오브젝트 데이터 불러오기
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_data(string data, RpcInfo info = default)
    {
       // Debug.Log("id: " + id);
        Debug.Log("data: "+data);
        //objectInfo = data;

        if(data.Length > 0)
        {
            VirtualObjectInfo VO = JsonUtility.FromJson<VirtualObjectInfo>(data);

            Debug.Log("source: " + info.Source);
            if (info.Source != Runner.LocalPlayer)
                GetComponent<VirtualObject>().info = VO;

            apply(VO);
        }
        else  // 기본형..
        {
            
            Material m;
            switch (objectType)
            {
                case (int)ObjectType.OBJ_SUBSKETCH:
                    m = pureBlack;
                    break;

                case (int)ObjectType.OBJ_TIMER:
                case (int)ObjectType.OBJ_CLOCK:
                case (int)ObjectType.OBJ_NOTE:
                case (int)ObjectType.OBJ_SPHERE:
                case (int)ObjectType.OBJ_CUBE:
                default:
                    m = CoreInteraction.instance.intToColor(objectType);
                    break;
            }

            Object.GetComponent<MeshRenderer>().material = m;
        }



    }

    // 불러온 데이터를 net과 local에 적용
    public void apply(VirtualObjectInfo info)
    {

        if (info == null)
        {
            //GetComponent<VirtualObject>().info = new VirtualObjectInfo();
            return;
        }

        objectType = info.objectType;
        color = info.color;
        if(Object.GetComponent<MeshRenderer>() != null)
        {
            Object.GetComponent<MeshRenderer>().material = CoreInteraction.instance.intToColor(color);
        }

        switch (objectType) {

            case (int)ObjectType.OBJ_TIMER:
                timer_seconds = info.timer_seconds;
                Object.GetComponent<Timer>().seconds = timer_seconds;
                break;

            case (int)ObjectType.OBJ_CLOCK:
                clock_region = info.clock_region;
                Object.GetComponent<Clock>().region = (Region)clock_region;
                break;

            case (int)ObjectType.OBJ_SUBSKETCH:

                Debug.Log("apply sk");

                width = info.width;
                sketch_dots.CopyFrom(info.sketch_dots, 0, info.sketch_dots.Count);

                TubeRenderer tubeRenderer = Object.GetComponent<TubeRenderer>();

                Object.GetComponent<MeshRenderer>().material = Spawner.instance.pureColors[0];   // pure yellow
                Object.GetComponent<LineRenderer>().SetPositions(sketch_dots.ToArray());
                tubeRenderer.SetRadius(width / 1000);       // 1000:1 즉 0.01 : 10 (반지름 기준)
                tubeRenderer.SetPositions(sketch_dots.ToArray());
                Object.GetComponent<MeshFilter>().mesh = tubeRenderer.GetMesh();
                Object.GetComponent<MeshCollider>().sharedMesh = tubeRenderer.GetMesh();
                break;

            case (int)ObjectType.OBJ_NOTE:
                note_string = info.note_string;
                Object.transform.Find("Canvas").Find("Text").GetComponent<TMPro.TMP_Text>().text = note_string.ToString();
                break;
            default:
                break;
        }

        initialized = true;
    }

    bool initialized = false;


    // 현재 상태를 net에 적용 (기본값)
    public void applyCurState()
    {

        color = Object.GetComponent<VirtualObject>().info.color;

        switch (objectType)
        {
            case (int)ObjectType.OBJ_TIMER:
                timer_seconds = Object.GetComponent<Timer>().seconds;
                current_seconds = Object.GetComponent<Timer>().currentSeconds;
                break;
            case (int)ObjectType.OBJ_CLOCK:
                clock_region = (int)Object.GetComponent<Clock>().region;
                break;

            case (int)ObjectType.OBJ_SUBSKETCH:


                // 스케치는 수정하지 않는 것으로 고려
                /*
                width = obj.GetComponent<VirtualObject>().info.width;
                sketch_dots.CopyFrom(obj.GetComponent<VirtualObject>().info.sketch_dots,
                    0, obj.GetComponent<VirtualObject>().info.sketch_dots.Count);

                TubeRenderer tubeRenderer = obj.GetComponent<TubeRenderer>();

                obj.GetComponent<MeshRenderer>().material = Spawner.instance.pureColors[0];   // pure yellow
                obj.GetComponent<LineRenderer>().SetPositions(sketch_dots.ToArray());
                tubeRenderer.SetRadius(width / 1000);       // 1000:1 즉 0.01 : 10 (반지름 기준)
                tubeRenderer.SetPositions(sketch_dots.ToArray());
                obj.GetComponent<MeshFilter>().mesh = tubeRenderer.GetMesh();
                obj.GetComponent<MeshCollider>().sharedMesh = tubeRenderer.GetMesh();
                */
                break;

            case (int)ObjectType.OBJ_NOTE:
                note_string = Object.transform.Find("Canvas").Find("Text").GetComponent<TMPro.TMP_Text>().text;
                break;
            default:
                break;
        }

    }

    // net 값을 모든 local에 적용 (onChanged)
    public static void ChangedTimer(Changed<VirtualObjectNet> changed) {
        changed.Behaviour.Object.GetComponent<Timer>().seconds = changed.Behaviour.timer_seconds;
    }
    public static void ChangedTimerTime(Changed<VirtualObjectNet> changed) {
        changed.Behaviour.Object.GetComponent<Timer>().currentSeconds = changed.Behaviour.current_seconds;
    }
    public static void ChangedClock(Changed<VirtualObjectNet> changed) {
        changed.Behaviour.Object.GetComponent<Clock>().region = (Region)changed.Behaviour.clock_region;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_note(string note, RpcInfo info = default)
    {
        // if (Runner.IsServer)
        Debug.Log(note);
        note_string = note;

    }

    public static void ChangedNote(Changed<VirtualObjectNet> changed) {
        changed.Behaviour.Object.transform.Find("Canvas").Find("Text").GetComponent<TMPro.TMP_Text>().text
            = changed.Behaviour.note_string.ToString();
    }
    public static void ChangedColor(Changed<VirtualObjectNet> changed) {
        changed.Behaviour.Object.GetComponent<MeshRenderer>().material
            = CoreInteraction.instance.intToColor(changed.Behaviour.color);
    }


    

    IEnumerator printPos()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            Debug.Log("Auth: "+ Object.InputAuthority+ " Pos: " + Object.gameObject.transform.position);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        //Obj = GetComponent<NetworkObject>();
        //StartCoroutine(printPos());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Spawned()
    {
        playerID = Object.InputAuthority;

        restore();
        //GetComponent<VirtualObject>().info = JsonUtility.FromJson<VirtualObjectInfo>(objectInfo);

        

    }

    // 중간에 참가한 사람에게 회의 오브젝트 표시
    public void restore()
    {
        
        

        if (Object.GetComponent<MeshRenderer>() != null)
        {
            Material m;
            switch (objectType)
            {
                case (int)ObjectType.OBJ_SUBSKETCH:
                    m = pureBlack;
                    break;

                case (int)ObjectType.OBJ_TIMER:
                case (int)ObjectType.OBJ_CLOCK:
                case (int)ObjectType.OBJ_NOTE:
                case (int)ObjectType.OBJ_SPHERE:
                case (int)ObjectType.OBJ_CUBE:
                default:
                    m = Spawner.instance.localPrefabs[objectType].GetComponent<MeshRenderer>().sharedMaterial;
                    break;
            }

            Object.GetComponent<MeshRenderer>().material = m;
        }
        

        switch (objectType)
        {

            case (int)ObjectType.OBJ_TIMER:
                Object.GetComponent<Timer>().seconds = timer_seconds;
                break;

            case (int)ObjectType.OBJ_CLOCK:
                Object.GetComponent<Clock>().region = (Region)clock_region;
                break;

            case (int)ObjectType.OBJ_SUBSKETCH:

                Debug.Log("restore sk");
                TubeRenderer tubeRenderer = Object.GetComponent<TubeRenderer>();

                //Object.GetComponent<MeshRenderer>().material = Spawner.instance.pureColors[0];   // pure yellow



                List<Vector3> s = new List<Vector3>();
                for (int i = 1; i < 300; i++)
                    if (!sketch_dots[i].Equals(Vector3.zero))
                        s.Add(sketch_dots[i]);
                    else break;

                tubeRenderer.SetPositions(s.ToArray());
                GetComponent<MeshFilter>().mesh = tubeRenderer.GetMesh();
                GetComponent<MeshCollider>().sharedMesh = tubeRenderer.GetMesh();

                break;

            case (int)ObjectType.OBJ_NOTE:
                Object.transform.Find("Canvas").Find("Text").GetComponent<TMPro.TMP_Text>().text = note_string.ToString();
                break;
            default:
                break;
        }

        initialized = true;
    }

    public override void FixedUpdateNetwork()
    {

        if (Object != null)
        {

            if (initialized)
            {
                if(objectType == (int)ObjectType.OBJ_NOTE)
                {
                    if (GetComponent<Oculus.Voice.Voice_Handler>()._active)
                    {
                        applyCurState();
                    }
                }
                else if(objectType == (int)ObjectType.OBJ_SUBSKETCH)
                {
                    
                }
                else
                {
                    applyCurState();
                }

                
            }

            /*
            if (initialized && objectType == (int)ObjectType.OBJ_SUBSKETCH && GetComponent<TubeRenderer>()._positions.Length == 0)
                Runner.Despawn(Object);
            */
            

            pos = transform.position;
            rot = transform.rotation;
            scale = transform.localScale;

            // 실제로 잡고 있는 사람: local VO
            if (GetComponent<Oculus.Interaction.HandGrab.HandGrabInteractable>().SelectingInteractors.Count > 0)
            {

                Object.GetComponent<NetworkTransform>().enabled = false;

            
                RPC_posTest(Object.transform.position, Object.transform.rotation, scale);    // 선택한 local obj의 정보를 강제로 전송
            }
            else
            {
                Object.GetComponent<NetworkTransform>().enabled = true;
            }
        }

       
        
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_posTest(Vector3 pos, Quaternion rot, Vector3 scale, RpcInfo info = default)
    {

        // Debug.Log("RPC: "+pos);
        Object.GetComponent<NetworkTransform>().TeleportToPositionRotation(pos, rot);
        Object.transform.localScale = scale;

    }



    // 스케치 그리기

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_sketchLive(int playerId, int index, Vector3 currentPoint, RpcInfo info = default)
    {
        GetComponent<VirtualObjectNet>().objectType = (int)ObjectType.OBJ_SUBSKETCH;
        GetComponent<MeshRenderer>().material = pureBlack;
        //Debug.Log("lineSketch");
        if (playerId == 9)
        {
            if (index < 300)  // 현재 한도
            {
                sketch_dots.Set(index++, currentPoint);
            }
        }

        List<Vector3> s = new List<Vector3>();
        for (int i = 0; i < index; i++)
            if (!sketch_dots[i].Equals(Vector3.zero))
                s.Add(sketch_dots[i]);

        TubeRenderer tubeRenderer = GetComponent<TubeRenderer>();
        tubeRenderer.SetPositions(s.ToArray());
        GetComponent<MeshFilter>().mesh = tubeRenderer.GetMesh();
        GetComponent<MeshCollider>().sharedMesh = tubeRenderer.GetMesh();

    }



}
