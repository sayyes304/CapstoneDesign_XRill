using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[System.Serializable]
public class Keys
{
    public List<string> keyList;
    public int next;

    public Keys()
    {
        keyList = new List<string>();
        next = 1;
    }

}

public class VOManager : MonoBehaviour
{
    public List<GameObject> prefabs;
    public List<Material> colors;
    public List<Material> pureColors;
    public Color[] colorTints;

    public static Keys keyList;    // 모든 키 리스트
    public static int curIndex;    // 이번에 추가될 오브젝트의 번호
    public static Dictionary<string, VirtualObjectInfo> virtualObjects;


    /*
    // Start is called before the first frame update
    void Start()
    {

        keyList = JsonUtility.FromJson<Keys>(PlayerPrefs.GetString("Keys"));
        if (keyList == null)
            keyList = new Keys();
        virtualObjects = new Dictionary<string, VirtualObjectInfo>();
        curIndex = keyList.next;

        foreach(string key in keyList.keyList)
        {
            virtualObjects.Add(key, JsonUtility.FromJson<VirtualObjectInfo>(PlayerPrefs.GetString(key)));
        }

        // 불러와진 VO를 생성
        createVOInit();
    }*/

    public static OVRSpatialAnchor SpatialAnchor;
    [SerializeField]
    OVRSpatialAnchor _anchorPrefab;

    OVRSpatialAnchor spatialAnchor;

    Action<OVRSpatialAnchor.UnboundAnchor, bool> _onLoadAnchor;

    private void Start()
    {
        LoadAnchorsByUuid();
        StartCoroutine(FindGameObjectCoroutine());
    }

    public void LoadAnchorsByUuid()
    {
        // Get number of saved anchor uuids
        if (!PlayerPrefs.HasKey(Anchor.NumUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(Anchor.NumUuidsPlayerPref, 0);
        }

        var playerUuidCount = PlayerPrefs.GetInt("numUuids");
        Log($"Attempting to load {playerUuidCount} saved anchors.");
        if (playerUuidCount == 0)
            return;

        var uuids = new Guid[playerUuidCount];
        for (int i = 0; i < playerUuidCount; ++i)
        {
            var uuidKey = "uuid";
            var currentUuid = PlayerPrefs.GetString(uuidKey);
            Log("QueryAnchorByUuid: " + currentUuid);

            uuids[i] = new Guid(currentUuid);
            print("uuod : " + uuids[i] + "  / Count : " + playerUuidCount);
        }

        Load(new OVRSpatialAnchor.LoadOptions
        {
            Timeout = 0,
            StorageLocation = OVRSpace.StorageLocation.Local,
            Uuids = uuids
        });
    }

    private void Awake()
    {
        _onLoadAnchor = OnLocalized;
    }

    private void Load(OVRSpatialAnchor.LoadOptions options) => OVRSpatialAnchor.LoadUnboundAnchors(options, anchors =>
    {
        if (anchors == null)
        {
            Log("Query failed.");
            return;
        }

        foreach (var anchor in anchors)
        {
            if (anchor.Localized)
            {
                _onLoadAnchor(anchor, true);
            }
            else if (!anchor.Localizing)
            {
                anchor.Localize(_onLoadAnchor);
            }
        }
    });

    private void OnLocalized(OVRSpatialAnchor.UnboundAnchor unboundAnchor, bool success)
    {
        if (!success)
        {
            Log($"{unboundAnchor} Localization failed!");
            return;
        }

        var pose = unboundAnchor.Pose;
        // var spatialAnchor = Instantiate(_anchorPrefab, pose.position, pose.rotation);
        // spatialAnchor = Instantiate(_anchorPrefab, Vector3.zero, Quaternion.Euler(0,0,0));
        spatialAnchor = Instantiate(_anchorPrefab, pose.position, pose.rotation);

        SpatialAnchor = spatialAnchor;
        //_anchorPrefab.transform.position = pose.position;
        // _anchorPrefab.transform.rotation = pose.rotation;
        //_anchorPrefab.transform.Rotate(pose.rotation.eulerAngles);

        // unboundAnchor.BindTo(spatialAnchor);
        // unboundAnchor.BindTo(_anchorPrefab);

        if (spatialAnchor.TryGetComponent<Anchor>(out var anchor))
        //if (_anchorPrefab.TryGetComponent<Anchor>(out var anchor))
        {
            // We just loaded it, so we know it exists in persistent storage.
            anchor.ShowSaveIcon = true;
        }
    }

    private static void Log(string message) => Debug.Log($"[SpatialAnchorsUnity]: {message}");
    // Start is called before the first frame update


    private IEnumerator FindGameObjectCoroutine()
    {
        // FindGameObjectWithTag 실행

        if (SpatialAnchor != null)
        {
            keyList = JsonUtility.FromJson<Keys>(PlayerPrefs.GetString("Keys"));
            if (keyList == null)
                keyList = new Keys();
            virtualObjects = new Dictionary<string, VirtualObjectInfo>();
            curIndex = keyList.next;

            foreach (string key in keyList.keyList)
            {
                virtualObjects.Add(key, JsonUtility.FromJson<VirtualObjectInfo>(PlayerPrefs.GetString(key)));
            }
            // 불러와진 VO를 생성
            createVOInit();
        }
        else
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(FindGameObjectCoroutine());
        }

    }


    void createVOInit()
    {

        foreach(VirtualObjectInfo info in virtualObjects.Values)
        {
            GameObject obj;

            if (info == null)
            {
                continue;
            }

            if (info.objectType == (int)ObjectType.OBJ_SUBSKETCH)
            {
                continue;
            }

            obj = Instantiate(prefabs[info.objectType], info.position, info.rotation ,SpatialAnchor.transform);
            obj.GetComponent<VirtualObject>().info = info;

            if (info.isFix)
            {
                obj.GetComponent<Oculus.Interaction.HandGrab.HandGrabInteractable>().enabled = false;
                obj.tag = "FixedObject";
            }
            else
            {
                obj.GetComponent<Oculus.Interaction.HandGrab.HandGrabInteractable>().enabled = true;
                obj.tag = "Object";
            }


            if (info.objectType != (int)ObjectType.OBJ_SCREEN && info.objectType != (int)ObjectType.OBJ_SKETCH)
                obj.GetComponent<MeshRenderer>().material = colors[info.color];

            //Debug.Log(info.objectType);

            switch (info.objectType)
            {
                case (int)ObjectType.OBJ_TIMER:
                    obj.GetComponent<Timer>().seconds = info.timer_seconds;
                    break;
                case (int)ObjectType.OBJ_CLOCK:
                    obj.GetComponent<Clock>().region = (Region)info.clock_region;
                    break;
                    
                case (int)ObjectType.OBJ_SKETCH:

                    //Debug.Log("Found sketch");

                    // group and border
                    obj.transform.Find("Border").GetComponent<MeshRenderer>().material = colors[8];  // transparent
                    Vector3 center = (info.minV + info.maxV) / 2;
                    obj.GetComponent<BoxCollider>().center = center;

                    obj.GetComponent<BoxCollider>().size = (center - info.minV) * 1.4f; // center - min

                    obj.transform.Find("Border").position = center + obj.transform.position;
                    obj.transform.Find("Border").localScale = (center - info.minV) * 1.4f;

                    // 좌표 문제
                    // subsketch
                    foreach (string child in info.sketch_child)
                    {
                        foreach(VirtualObjectInfo subsketch in virtualObjects.Values)
                        {
                            if(child == $"{subsketch.objectID}")
                            {
                                GameObject ss = Instantiate(prefabs[(int)ObjectType.OBJ_SUBSKETCH],
                                    obj.transform.position, subsketch.rotation, obj.transform); // obj의 position!

                                TubeRenderer tubeRenderer = ss.GetComponent<TubeRenderer>();

                                ss.GetComponent<VirtualObject>().info = subsketch;
                                ss.GetComponent<MeshRenderer>().material = pureColors[0];   // pure yellow
                                ss.GetComponent<LineRenderer>().SetPositions(subsketch.sketch_dots.ToArray());
                                tubeRenderer.SetRadius(subsketch.width / 1000);       // 1000:1 즉 0.01 : 10 (반지름 기준)
                                tubeRenderer.SetPositions(subsketch.sketch_dots.ToArray());
                                ss.GetComponent<MeshFilter>().mesh = tubeRenderer.GetMesh();
                                ss.GetComponent<MeshCollider>().sharedMesh = tubeRenderer.GetMesh();
                            }
                        }
                    }

                    break;
                
                case (int)ObjectType.OBJ_NOTE:
                    obj.transform.Find("Canvas").Find("Text")
                        .GetComponent<TMPro.TMP_Text>().text = info.note_string;

                    obj.transform.Find("memo").GetComponent<SpriteRenderer>().color = colorTints[info.color];

                    break;
                case (int)ObjectType.OBJ_SCREEN:
                    break;
                case (int)ObjectType.OBJ_CUBE:
                case (int)ObjectType.OBJ_SPHERE:
                default:
                    break;
            
            }

            obj.transform.localScale = info.initialScale * info.scale;
        }


        // 매 1초마다 현 상태 저장
        StartCoroutine(update1Sec());
    }

    IEnumerator update1Sec() // 부담 경감?
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            // 오브젝트 삭제 후, 키만 삭제된 상태

            // 삭제 대상인 키 확인
            List<string> shouldRemove = new List<string>();

            foreach (string key in virtualObjects.Keys)
            {
                if(!keyList.keyList.Contains(key))   // 키 리스트에 없으면 삭제해도 됨
                    shouldRemove.Add(key);
            }

            foreach (string key in shouldRemove)    // 그러한 키와 값 삭제
            {
                PlayerPrefs.DeleteKey(key);
                virtualObjects.Remove(key);
            }

            yield return new WaitForSeconds(0.5f);

            keyList.next = curIndex;
            PlayerPrefs.SetString("Keys", JsonUtility.ToJson(keyList));

            foreach(string key in virtualObjects.Keys)
            {

                PlayerPrefs.SetString(key, JsonUtility.ToJson(virtualObjects[key]));
            }

            PlayerPrefs.Save();
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

}
