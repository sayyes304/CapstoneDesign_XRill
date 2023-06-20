using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.HandGrab;



public class CoreInteraction : MonoBehaviour
{
    public GameObject GazeHandVisual;
    public static CoreInteraction instance;

    public GameObject cube, sphere, sketchGroup, sketchPiece, screen, note, clock, timer;
    public GameObject prefabSet, grabberL, grabberR;
    public HandGrabInteractor leftInteractor, rightInteractor;
    public GameObject optionCanvas;

    // 0~7 순서대로 빨 주 노 초 파 보 흰 검
    public Material[] color;
    public Color[] colorTint;

    public float initGazeHandOffset = 0.43f, editGazeHandOffset = 100f;

    [Header("AR 모드에서만 체크하세요")]
    public bool allowsObjectSaving = true;

    public OVRSpatialAnchor SpatialAnchor;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;

    }

    // Update is called once per frame
    void Update()
    {

        grabberL.transform.position = leftInteractor.transform.position;
        grabberR.transform.position = rightInteractor.transform.position;

        GazeHandVisual.SetActive(!isEditing);   // 수정 중에는 자동 숨기기

        SpatialAnchor = VOManager.SpatialAnchor;
    }

    #region create
    public void create()
    {
        Debug.Log("Create");


        // VR 모드에서는 별도 항목 사용
        if (!allowsObjectSaving)
            return;



        // 여러 개의 후보 오브젝트가 눈앞에 1차 생성되며
        // 하나를 잡으면 나머지는 자동 파괴. (선택 완료)
        // 크기나 색상은 생성 이후에 직접 조절
        prefabSet.SetActive(true);
        StartCoroutine(select());
    }

    public static bool isSelecting = false;

    IEnumerator select()
    {
        GameObject obj;

        isSelecting = true;
        grabberL.SetActive(true); // 주먹 쥐었는지 판단
        grabberR.SetActive(true);

        // 둘 중 한 손이 잡을 때까지 기다림 // wait until condition is false
        //yield return new WaitUntil(() => leftInteractor.HasSelectedInteractable || rightInteractor.HasSelectedInteractable);

        // 특정 물체를 응시한 상태로 주먹을 쥘 때
        yield return new WaitUntil(() => leftInteractor.IsGrabbing || rightInteractor.IsGrabbing);


        string name;
        HandGrabInteractor interactor;

        if (leftInteractor.IsGrabbing)
            interactor = leftInteractor;
        else
            interactor = rightInteractor;

        interactor.ForceRelease();
        grabberL.SetActive(false);
        grabberR.SetActive(false);
        prefabSet.SetActive(false);

        isSelecting = false;

        // 다른 것
        if (!GazeHand.instance.gazedObject.CompareTag("Prefab"))
        {
            yield break;
        }


        //name = interactor.SelectedInteractable.gameObject.name;
        name = GazeHand.instance.gazedObject.name;
        Debug.Log("selected: "+name);

        switch(name){
            case "Screen":
                obj = Instantiate(screen, interactor.transform.position, Quaternion.identity,SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "Cube":
                obj = Instantiate(cube, interactor.transform.position, Quaternion.identity, SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "Note":
                obj = Instantiate(note, interactor.transform.position, Quaternion.identity, SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "Sphere":
                obj = Instantiate(sphere, interactor.transform.position, Quaternion.identity, SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "Timer":
                obj = Instantiate(timer, interactor.transform.position, Quaternion.identity, SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "Clock":
                obj = Instantiate(clock, interactor.transform.position, Quaternion.identity, SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;

            // 기존 블럭을 잡은 경우
            default:
                // 창은 사라지고 그대로 마무리
                obj = null;
                break;

        }

        // object 추가
        if(obj != null && allowsObjectSaving)
        {
            obj.GetComponent<VirtualObject>().info.objectID = VOManager.curIndex;
            VOManager.keyList.keyList.Add($"{VOManager.curIndex}");
            VOManager.virtualObjects.Add($"{VOManager.curIndex++}", obj.GetComponent<VirtualObject>().info);
        }
            



        StartCoroutine(FingerMenu.instance.tempHide()); // 숨겨주기


    }

    #endregion

    #region sketch

    public void sketch()
    {
        Debug.Log("Sketch");
        // 집게손가락으로 스케치 가능
        // 양손 모두 사용 가능 (동시 지원 목표)
        // 중지손가락으로 두께
        // 약지손가락으로 색상
        // 새끼손가락으로 적용

        StartCoroutine(whileSketching());
    }

    public static bool isSketching = false; // overall
    public static bool isDrawing = false; // piece

    GameObject sketchObj;
    Vector3 maxV, minV;
    List<string> subSketchID;
    int tempParentID;

    // 그리기 세트 시작
    IEnumerator whileSketching()
    {
        isSketching = true;
        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true;
        //FingerMenu.instance.detectPinch.GetComponent<MeshRenderer>().enabled = true;
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true;
        //FingerMenu.instance.detectRing.GetComponent<MeshRenderer>().enabled = true;
        sketchCanvas.SetActive(true);

        subSketchID = new List<string>();


        // 기준점? 설치 위치
        sketchObj = Instantiate(sketchGroup, FingerMenu.instance.index.position, Quaternion.identity,SpatialAnchor.transform);

        // Object 추가

        if (allowsObjectSaving)
        {
            sketchObj.GetComponent<VirtualObject>().info.objectID = VOManager.curIndex;
            VOManager.keyList.keyList.Add($"{VOManager.curIndex}");
            tempParentID = VOManager.curIndex;
            VOManager.virtualObjects.Add($"{VOManager.curIndex++}", sketchObj.GetComponent<VirtualObject>().info);
        }



        maxV = new Vector3(-9999999, -9999999, -9999999);
        minV = new Vector3(9999999, 9999999, 9999999);

        Debug.Log("sketch is about to start");

        while (true)
        {
            if (FingerMenu.isPinching && !isDrawing)    // draw
            {
                Debug.Log("pinching");
                StartCoroutine(whileDrawing());
            }

            yield return null;

            if (!isDrawing && FingerMenu.isMiddle) // width
            {
                // sketch 두께 조절
                StartCoroutine(slideLineWidth());
            }


            if (!isDrawing && FingerMenu.isPinky) // exit and confirm
            {
                break;
            }


            if (!isDrawing && FingerMenu.isRing) // exit without saving
            {
                Debug.Log("User wants to exit it, so destroy it");
                // 오류 방지?를 위해 끝번부터 삭제
                for (int i = sketchObj.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(sketchObj.transform.GetChild(i).gameObject);
                }
                Destroy(sketchObj);

                sketchCanvas.SetActive(false);
                isSketching = false;
                //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = false;
                //FingerMenu.instance.detectPinch.GetComponent<MeshRenderer>().enabled = false;
                //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = false;
                //FingerMenu.instance.detectRing.GetComponent<MeshRenderer>().enabled = false;
                StartCoroutine(FingerMenu.instance.tempHide()); // 숨겨주기
                yield break;
            }
        }

        Debug.Log("sketch is completed");
        // subsketch가 없으면 파괴하기
        if(sketchObj.GetComponentInChildren<LineRenderer>() == null)
        {
            Debug.Log("There is no subsketch, so destroy it");
            Destroy(sketchObj);

        }
        else
        {
            //박스 크기 구하기
            Debug.Log(minV);
            Debug.Log(maxV);
            Vector3 center = (minV + maxV) / 2;
            sketchObj.GetComponent<BoxCollider>().center = center;
            // 정석은 2배인데, 박스임을 고려하여(회전시 모서리 부분이 큼) 1.4배로 조정
            sketchObj.GetComponent<BoxCollider>().size = (center - minV) * 1.4f; // center - min

            sketchObj.transform.Find("Border").position = center + sketchObj.transform.position;
            sketchObj.transform.Find("Border").localScale = (center - minV) * 1.4f;

            Debug.Log(center);
            Debug.Log(center - minV);

            // 약간 앞으로 옮기기
            sketchObj.transform.position = sketchObj.transform.position + new Vector3(0, 0, 0.8f);


            // 자식 추가
            sketchObj.GetComponent<VirtualObject>().info.sketch_child = subSketchID;

        }
            isSketching = false;
            sketchCanvas.SetActive(false);
            //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = false;
            //FingerMenu.instance.detectPinch.GetComponent<MeshRenderer>().enabled = false;
            //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = false;
            //FingerMenu.instance.detectRing.GetComponent<MeshRenderer>().enabled = false;
            StartCoroutine(FingerMenu.instance.tempHide()); // 숨겨주기


        // finish
    }

    // 두께 조절
    public GameObject sketchCanvas;
    public float currentSketchWidth = 10;       // 1000:1 즉 0.01 : 10 (반지름 기준)
    public TMPro.TMP_Text sketchWidthText;
    public LineRenderer sketchWidthVisual;

    IEnumerator slideLineWidth()
    {

        Vector3 pos = FingerMenu.instance.detectMiddle.position;

        while (FingerMenu.isMiddle)
        {
            float value = (FingerMenu.instance.detectMiddle.position.x - pos.x) * Time.deltaTime * 2; // 50cm = 1 

            currentSketchWidth += value;
            //Debug.Log(currentSketchWidth);

            if (currentSketchWidth < 1) currentSketchWidth = 1;
            if (currentSketchWidth > 100) currentSketchWidth = 100;


            //obj.GetComponent<MeshRenderer>().material = color[(int)Mathf.Round(clockSlider.value)];
            sketchWidthText.text = $"{(int)currentSketchWidth}";

            sketchWidthVisual.startWidth = currentSketchWidth / 500;
            sketchWidthVisual.endWidth = currentSketchWidth / 500;

            yield return null;
        }

        currentSketchWidth = (int)Mathf.Round(currentSketchWidth);
        sketchWidthText.text = $"{(int)currentSketchWidth}";
    }




    // pinch했을 때 각각 그리기
    List<Vector3> currentLine;
    IEnumerator whileDrawing()
    {
        isDrawing = true;
        currentLine = new List<Vector3>();

        Debug.Log("start drawing subsketch");

        GameObject subSketch = Instantiate(sketchPiece, sketchObj.transform.position, Quaternion.identity, sketchObj.transform);

        LineRenderer lineRenderer = subSketch.GetComponent<LineRenderer>();
        TubeRenderer tubeRenderer = subSketch.GetComponent<TubeRenderer>();
        tubeRenderer.SetRadius(currentSketchWidth / 1000);       // 1000:1 즉 0.01 : 10 (반지름 기준)

        int i = 0;
        while (true)
        {
            if (i++ % 3 != 0)   // 3프레임당 1개
            {
                yield return null;
                continue;
            }

            // local space이므로 이렇게 처리
            Vector3 p = FingerMenu.instance.index.position - sketchObj.transform.position;
            lineRenderer.SetPosition(lineRenderer.positionCount++, p);

            currentLine.Add(p);
            tubeRenderer.SetPositions(currentLine.ToArray());
            subSketch.GetComponent<MeshFilter>().mesh = tubeRenderer.GetMesh();
            subSketch.GetComponent<MeshCollider>().sharedMesh = tubeRenderer.GetMesh();

            // Box 영역 구하기
            if (p.x < minV.x) minV.x = p.x;
            if (p.y < minV.y) minV.y = p.y;
            if (p.z < minV.z) minV.z = p.z;
            if (p.x > maxV.x) maxV.x = p.x;
            if (p.y > maxV.y) maxV.y = p.y;
            if (p.z > maxV.z) maxV.z = p.z;

            yield return null;

            if (!FingerMenu.isPinching)
            {
                break;
            }

        }

        Debug.Log("finish drawing subsketch");
        isDrawing = false;

        // object 추가

        if (allowsObjectSaving)
        {
            subSketchID.Add($"{VOManager.curIndex}");
            subSketch.GetComponent<VirtualObject>().info.objectID = VOManager.curIndex;
            subSketch.GetComponent<VirtualObject>().info.width = currentSketchWidth;
            subSketch.GetComponent<VirtualObject>().info.sketch_dots = currentLine;
            subSketch.GetComponent<VirtualObject>().info.parent = tempParentID;
            subSketch.GetComponent<VirtualObject>().info.independent = false;
            VOManager.keyList.keyList.Add($"{VOManager.curIndex}");
            VOManager.virtualObjects.Add($"{VOManager.curIndex++}", subSketch.GetComponent<VirtualObject>().info);
        }





    }

    #endregion

    public void copy(GameObject obj)
    {
        if (obj.GetComponent<HandGrabInteractable>())
        {
            Debug.Log("Copy" + obj);

            obj.GetComponent<VirtualObject>().isCopied = true;  // 임시 플래그
            obj.GetComponent<VirtualObject>().lastID = obj.GetComponent<VirtualObject>().info.objectID;

            Instantiate(obj, obj.transform.position + new Vector3(0.1f, 0.1f, 0.1f), obj.transform.rotation,SpatialAnchor.transform);

            obj.GetComponent<VirtualObject>().isCopied = false;
        }
        else
        {
            Debug.LogWarning("Cannot copy it because it does not have HandGrabInteractable");
        }
    }

    public void delete(GameObject obj)
    {
        if (obj.GetComponent<HandGrabInteractable>())
        {
            Debug.Log("Delete" + obj);

            // subsketch 존재
            if (obj.GetComponentInChildren<LineRenderer>() != null)
            {
                for (int i = obj.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(obj.transform.GetChild(i).gameObject);
                }
            }

            Destroy(obj);
        }
        else
        {
            Debug.LogWarning("Cannot delete it because it does not have HandGrabInteractable");
        }
    }

    #region edit

    // for menu
    GameObject m1, m2, m3, m4;
    public void edit(GameObject obj)
    {
        optionCanvas.SetActive(true);
        

        if (m1 == null)
        {
            Transform panel = optionCanvas.transform.Find("MenuPanel");
            m1 = panel.Find("Menu1").gameObject;
            m2 = panel.Find("Menu2").gameObject;
            m3 = panel.Find("Menu3").gameObject;
            m4 = panel.Find("Menu4").gameObject;
        }

        if (obj.GetComponent<HandGrabInteractable>())
        {
            Debug.Log("Edit" + obj);
            
            // switch
            switch(obj.name.Substring(0, 5))
            {
                // Cube, Sphere, Clock, Timer 완성    || Note, Screen 미완성

                // Basics
                case "GCube":
                case "GSphe":
                    StartCoroutine(editBasics(obj));             // 색상
                    break;

                // Sketch
                case "GSket":                                                   // (위치 조정, 색상)

                    Debug.Log("별도의 추가 옵션이 없으면 자동 고정 적용");
                    obj.GetComponent<HandGrabInteractable>().enabled = false;   // 뒤집기
                    obj.tag = "FixedObject";
                    Debug.Log("Fix " + obj.name);

                    optionCanvas.SetActive(false);
                    break;

                // Widgets
                case "GScre":

                    Debug.Log("별도의 추가 옵션이 없으면 자동 고정 적용");
                    obj.GetComponent<HandGrabInteractable>().enabled = false;   // 뒤집기
                    obj.tag = "FixedObject";
                    Debug.Log("Fix " + obj.name);

                    optionCanvas.SetActive(false);
                    break;

                case "GCloc":                                   // 시계
                    StartCoroutine(editClock(obj));             // 지역 수정, 색상
                    break;
                case "GNote":
                    StartCoroutine(editBasics(obj));            // 음성 텍스트, 키보드 텍스트, 색상
                    break;
                case "GTime":                                   // 타이머(시작, 끝) -> 검지손가락
                    StartCoroutine(editTimer(obj));            // 시간 설정, 색상 
                    break;

                default:
                    break;

            }

        }
        else
        {
            Debug.LogWarning("Cannot edit it because it does not have HandGrabInteractable");
        }
    }


    public static bool
        isEditing = false,
        isChangingColor = false,    // for all objects including cube and sphere except screen
        isChangingTextwithKey = false,     // for note
        isChangingTextwithVoice = false,     // for note
        isChangingTime = false,     // for timer
        isChangingRegion = false,   // for clock
        isChangingSketch = false;   // for sketch


    #region editForType



    // 값 조절은 중지로
    // 설정 종료는 새끼로
    // 옵션 선택은 손바닥면에 UI 추가하기
    IEnumerator editBasics(GameObject obj)
    {
        GazeHand.instance.dist = editGazeHandOffset;
        isEditing = true;

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true;
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true;

        m1.SetActive(true);
        m2.SetActive(false);
        m3.SetActive(false);
        m4.SetActive(false);

        StartCoroutine(editColor(obj)); // 초기 옵션

        // 메뉴 추가하기

        while (true)
        {

            if (FingerMenu.isLongRing) // 물체 고정
            {
                obj.GetComponent<HandGrabInteractable>().enabled = false;   // 뒤집기
                obj.tag = "FixedObject";
                Debug.Log("Fix " + obj.name);
                break;  // 물체 고정시 자동으로 메뉴에서 나가짐
            }

            if (FingerMenu.isPinky) // 종료
                break;

            yield return null;
        }

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = false;
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = false;

        isEditing = false;
        GazeHand.instance.dist = initGazeHandOffset;
        StartCoroutine(FingerMenu.instance.tempHide()); // 숨겨주기
        optionCanvas.SetActive(false);
    }


    IEnumerator editNote(GameObject obj){ yield return null; }
    IEnumerator editClock(GameObject obj) {

        GazeHand.instance.dist = editGazeHandOffset;
        isEditing = true;

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true; // Exit
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true; // Change value

        m1.SetActive(true);
        m1.transform.Find("Text").GetComponent<TMPro.TMP_Text>().text = "시간대\n조절";
        m2.SetActive(false);
        m3.SetActive(false);
        m4.SetActive(false);

        //StartCoroutine(editColor(obj)); // 초기 옵션
        StartCoroutine(editReg(obj));

        // 메뉴 추가하기

        while (true)
        {


            if (FingerMenu.isLongRing) // 물체 고정
            {
                obj.GetComponent<HandGrabInteractable>().enabled = false;   // 뒤집기
                obj.tag = "FixedObject";
                Debug.Log("Fix " + obj.name);
                break;  // 물체 고정시 자동으로 메뉴에서 나가짐
            }


            if (FingerMenu.isPinky)     // 어떤 메뉴를 보고 있든지간에 종료
                break;

            yield return null;
        }

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true; // Exit
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true; // Change value

        isEditing = false;
        GazeHand.instance.dist = initGazeHandOffset;
        StartCoroutine(FingerMenu.instance.tempHide()); // 숨겨주기
        optionCanvas.SetActive(false);

    }
    IEnumerator editTimer(GameObject obj) {


        GazeHand.instance.dist = editGazeHandOffset;
        isEditing = true;

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true; // Exit
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true; // Change value

        m1.SetActive(true);
        m1.transform.Find("Text").GetComponent<TMPro.TMP_Text>().text = "시간\n설정";
        m2.SetActive(false);
        m3.SetActive(false);
        m4.SetActive(false);

        StartCoroutine(editHowLong(obj)); // 초기 옵션

        // 메뉴 추가하기

        while (true)
        {

            if (FingerMenu.isLongRing) // 물체 고정
            {
                obj.GetComponent<HandGrabInteractable>().enabled = false;   // 뒤집기
                obj.tag = "FixedObject";
                Debug.Log("Fix " + obj.name);
                break;  // 물체 고정시 자동으로 메뉴에서 나가짐
            }

            if (FingerMenu.isPinky)     // 어떤 메뉴를 보고 있든지간에 종료
                break;

            yield return null;
        }

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true; // Exit
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true; // Change value

        isEditing = false;
        GazeHand.instance.dist = initGazeHandOffset;
        StartCoroutine(FingerMenu.instance.tempHide()); // 숨겨주기
        optionCanvas.SetActive(false);


    }
    IEnumerator editSketch(GameObject obj) { yield return null; }

    #endregion

    #region editModule

    public int colorToInt(Material material)
    {
        for(int i=0; i<color.Length; i++)
        {
            if (color[i].color == material.color)
                return i;
        }
        return 0;   // not found -> default
    }

    public Material intToColor(int c)
    {
        if (c < color.Length) return color[c];
        else return color[0];
    }

    public GameObject colorCanvas;
    public UnityEngine.UI.Slider colorSlider;


    // 중지 슬라이더
    // 새끼 종료
    IEnumerator editColor(GameObject obj)
    {
        // show color UI
        isChangingColor = true;
        colorCanvas.SetActive(true);

        // colorSlider.value는 VO로부터 미리 설정
        colorSlider.value = colorToInt(obj.GetComponent<MeshRenderer>().material);
        Debug.Log("Begin: " + colorSlider.value);

        
        while (true)
        {
            if (isChangingColor && FingerMenu.isMiddle) // 중지는 값 조절
                StartCoroutine(slideColor(obj));

            if (isChangingColor && FingerMenu.isPinky)  // 새끼는 설정 종료
            {
                break;
            }

            if (FingerMenu.isLongRing) // 물체 고정 메뉴 실행시 강제 종료
            {
                break;  
            }

            /*
            // 오브젝트별 다르게
            if (obj.name.Substring(0, 5)=="GCloc")
            {
                if (m2.GetComponent<TriggerDetector>().touched)
                {
                    Debug.Log("M2");
                    StartCoroutine(editReg(obj));
                    break;
                    
                }
            }

            // 오브젝트별 다르게
            if (obj.name.Substring(0, 5) == "GTime")
            {
                if (m2.GetComponent<TriggerDetector>().touched)
                {
                    StartCoroutine(editHowLong(obj));
                    break;

                }
            }
            */
            yield return null;
        }

        isChangingColor = false;
        colorCanvas.SetActive(false);
    }

    IEnumerator slideColor(GameObject obj) {

        Vector3 pos = FingerMenu.instance.detectMiddle.position;

        while (FingerMenu.isMiddle)
        {
            float value = (FingerMenu.instance.detectMiddle.position.x - pos.x) * Time.deltaTime; // 50cm = 1 
          
            colorSlider.value += value;
            Debug.Log(colorSlider.value);

            //value = Mathf.Min(7, Mathf.Max(value, 0));

            obj.GetComponent<MeshRenderer>().material = color[(int)Mathf.Round(colorSlider.value)];


            if (obj.transform.Find("memo") != null)
            {
                obj.transform.Find("memo").GetComponent<SpriteRenderer>().color = colorTint[(int)Mathf.Round(colorSlider.value)];
            }



            yield return null;
        }

        Debug.Log("Final: "+colorSlider.value);
        colorSlider.value = (int)Mathf.Round(colorSlider.value);
        obj.GetComponent<VirtualObject>().info.color = (int)Mathf.Round(colorSlider.value);
    }

    //------------------------------------------------

    // from editClock
    public GameObject clockCanvas;
    public UnityEngine.UI.Slider clockSlider;
    public TMPro.TMP_Text timeShow;
    IEnumerator editReg(GameObject obj)
    {

        isChangingRegion = true;
        clockCanvas.SetActive(true);

        colorSlider.value = (int)obj.GetComponent<Clock>().region;  // should be changed
        timeShow.text = $"현재 시각: {Clock.timeStr(0)}\n해당 지역: {Clock.timeStr((Region)(int)Mathf.Round(clockSlider.value))}";

        while (true)
        {
            if (isChangingRegion && FingerMenu.isMiddle) // 중지는 값 조절
                StartCoroutine(slideReg(obj));

            if (isChangingRegion && FingerMenu.isPinky)  // 새끼는 설정 종료
            {
                break;
            }

            if (FingerMenu.isLongRing) // 물체 고정 메뉴 실행시 강제 종료
            {
                break;
            }

            /*
            // 오브젝트별 다르게
            if (obj.name.Substring(0, 5) == "GCloc")
            {
                if (m1.GetComponent<TriggerDetector>().touched)
                {
                    StartCoroutine(editColor(obj));
                    break;

                }
            }
            */

            yield return null;

        }

        clockCanvas.SetActive(false);
        isChangingRegion = false;

    }


    IEnumerator slideReg(GameObject obj)
    {

        Vector3 pos = FingerMenu.instance.detectMiddle.position;

        while (FingerMenu.isMiddle)
        {
            float value = (FingerMenu.instance.detectMiddle.position.x - pos.x) * Time.deltaTime; // 50cm = 1 

            clockSlider.value += value;
            Debug.Log(clockSlider.value);

            //obj.GetComponent<MeshRenderer>().material = color[(int)Mathf.Round(clockSlider.value)];
            timeShow.text = $"현재 시각: {Clock.timeStr(0)}\n해당 지역: {Clock.timeStr((Region)(int)Mathf.Round(clockSlider.value))}";
            obj.GetComponent<Clock>().region = (Region)(int)Mathf.Round(clockSlider.value);
            yield return null;
        }

        Debug.Log("Final: " + clockSlider.value);
        clockSlider.value = (int)Mathf.Round(clockSlider.value);
    }

    //------------------------------------------------

    // for Timer
    public GameObject timerCanvas;
    public TMPro.TMP_Text timerText;
    float tempTimerValue;

    // 수정 필요
    IEnumerator editHowLong(GameObject obj)
    {

        isChangingTime = true;
        timerCanvas.SetActive(true);

        //colorSlider.value
        timerText.text = $"{Timer.formatting(obj.GetComponent<Timer>().seconds)}";
        tempTimerValue = obj.GetComponent<Timer>().seconds;


        while (true)
        {
            if (isChangingTime && FingerMenu.isMiddle) // 중지는 값 조절
                StartCoroutine(slideHowLong(obj));

            if (isChangingTime && FingerMenu.isPinky)  // 새끼는 설정 종료
            {
                break;
            }

            if (FingerMenu.isLongRing) // 물체 고정 메뉴 실행시 강제 종료
            {
                break;
            }

            /*
            if (isChangingTime && FingerMenu.isPinching)  // 타이머 한정으로 시간 설정 후 검지손가락 선택시 설정 종료 및 타이머 시작
            {
                StartCoroutine(Timer.runTimer(obj.GetComponent<Timer>()));
                break;
            }
            */

            /*
            // 오브젝트별 다르게
            if (obj.name.Substring(0, 5) == "GTime")
            {
                if (m1.GetComponent<TriggerDetector>().touched)
                {
                    StartCoroutine(editColor(obj));
                    break;

                }
            }
            */
            yield return null;

        }

        timerCanvas.SetActive(false);
        isChangingTime = false;

    }

    
    IEnumerator slideHowLong(GameObject obj)
    {

        Vector3 pos = FingerMenu.instance.detectMiddle.position;

        while (FingerMenu.isMiddle)
        {
            float value = (FingerMenu.instance.detectMiddle.position.x - pos.x) * Time.deltaTime * 2; // 50cm = 1 

            tempTimerValue += value;
            Debug.Log(tempTimerValue);

            //obj.GetComponent<MeshRenderer>().material = color[(int)Mathf.Round(clockSlider.value)];
            timerText.text = $"{Timer.formatting((int)tempTimerValue)}";
            obj.GetComponent<Timer>().seconds = (int)tempTimerValue;

            yield return null;
        }

        Debug.Log("Final: " + clockSlider.value);
        tempTimerValue = (int)Mathf.Round(tempTimerValue) / 10 * 10;    // 10 단위로 정리
        obj.GetComponent<Timer>().seconds = (int)tempTimerValue;
        timerText.text = $"{Timer.formatting((int)tempTimerValue)}";
    }

    #endregion

    #endregion


    public void settings()
    {
        Debug.Log("Settings");
        // 설정 팝업 띄우기
    }


}
