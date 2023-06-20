using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class FingerMenu : MonoBehaviour
{
    public static FingerMenu instance;

    public Transform index, middle, pinky, ring, thumb,
        detectPinch, detectMiddle, detectRing, detectPinky, indexRight;
    public OVRHand OVRHandPrefabLeft, OVRHandPrefabRight;

    public IList<OVRBone> boneList;

    public SkinnedMeshRenderer left, right, remoteLeft, remoteRight;
    public Material original, selected, transparent;

    public UnityEngine.UI.Image gazePointer;

    // [Header("aaa")]
    //public TouchDetection grip, pinch; 
    public Oculus.Interaction.HandGrab.HandGrabInteractor LInteractor, RInteractor;

    public GameObject
        // main
        mSketch, mRun,      // index
        mCreate, mCopy,     // middle
        mUnpin, mOption,      // ring
        mSetting, mRemove,  // pinky
                            // sub
        mDraw,              // index
        mSlider, mWidth,    // middle
        mPin, mCancel,      // ring
        mApply;             // pinky

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {

        // for debugging
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayerPrefs.SetString("SessionName", UserManagement.loginedUserID_s);
                PlayerPrefs.Save();
                ARtoVRLoading.Instance.startLoadScene("VRScene");
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (ARLobby.sessionList.Count > 0)
                {
                    PlayerPrefs.SetString("SessionName", ARLobby.sessionList[0].Name); // 0번
                    PlayerPrefs.Save();
                    ARtoVRLoading.Instance.startLoadScene("VRScene");
                }
                else
                {
                    Debug.Log("No Room");
                }
            }
        }





        // Hand Tracking
        if (boneList == null && OVRHandPrefabLeft.GetComponent<OVRSkeleton>().Bones.Count > 0)
            boneList = OVRHandPrefabLeft.GetComponent<OVRSkeleton>().Bones;
        
        if (boneList != null && boneList.Count > 0)
        {
            // main menu
            thumb.position = boneList[(int)OVRSkeleton.BoneId.Hand_ThumbTip].Transform.position;
            index.position = boneList[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
            middle.position = boneList[(int)OVRSkeleton.BoneId.Hand_MiddleTip].Transform.position;
            ring.position = boneList[(int)OVRSkeleton.BoneId.Hand_RingTip].Transform.position;
            pinky.position = boneList[(int)OVRSkeleton.BoneId.Hand_PinkyTip].Transform.position;

            thumb.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_ThumbTip].Transform.rotation;
            index.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.rotation;
            middle.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_MiddleTip].Transform.rotation;
            ring.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_RingTip].Transform.rotation;
            pinky.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_PinkyTip].Transform.rotation;

            // sub menu
            detectPinch.position = boneList[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
            detectMiddle.position = boneList[(int)OVRSkeleton.BoneId.Hand_MiddleTip].Transform.position;
            detectRing.position = boneList[(int)OVRSkeleton.BoneId.Hand_RingTip].Transform.position;
            detectPinky.position = boneList[(int)OVRSkeleton.BoneId.Hand_PinkyTip].Transform.position;

            detectPinch.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.rotation;
            detectMiddle.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_MiddleTip].Transform.rotation;
            detectRing.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_RingTip].Transform.rotation;
            detectPinky.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_PinkyTip].Transform.rotation;
        }

        if(OVRHandPrefabLeft.GetComponent<OVRSkeleton>().Bones.Count > 0)
        {
            indexRight.position = OVRHandPrefabRight.GetComponent<OVRSkeleton>()
                .Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
        }

        // 메뉴 아이콘 표시
        foreach (var g in new[] { mSketch, mRun, mCreate, mCopy, mUnpin, mOption, mSetting, mRemove, mDraw, mSlider, mWidth, mPin, mCancel, mApply })
            g.SetActive(false);

        // 설정 메뉴만 표시
        if (CoreInteraction.isEditing)
        {
            mApply.SetActive(true);
            mPin.SetActive(true);
            mSlider.SetActive(true);
        }

        // 스케치 메뉴만 표시
        if (CoreInteraction.isSketching)
        {
            mWidth.SetActive(true);
            mDraw.SetActive(true);
            mCancel.SetActive(true);
            mApply.SetActive(true);
        }

        // 기본 환경
        if (GazeHand.instance.gazedObject != null)
        {
            // Interactable Object
            if (GazeHand.instance.gazedObject.CompareTag("Object") && selectedObject != null)     
            {
                mRun.SetActive(true);
                mCopy.SetActive(true);
                mOption.SetActive(true);
                mRemove.SetActive(true);

            }
            else if(!GazeHand.instance.gazedObject.CompareTag("Object") && selectedObject == null)    // Just Collider
            {
                mSketch.SetActive(true);
                mCreate.SetActive(true);
                if (GazeHand.instance.gazedObject.CompareTag("FixedObject"))
                    mUnpin.SetActive(true);
                mSetting.SetActive(true);
                
            }
            // else Nothing appeared

        }
        else    // No collision
        {
            mSketch.SetActive(true);
            mCreate.SetActive(true);
            //mUnpin.SetActive(true);
            mSetting.SetActive(true);
        }



        // 물체를 추가할 때
        if (CoreInteraction.isSelecting || CoreInteraction.isSketching || CoreInteraction.isEditing)
        {
            index.gameObject.SetActive(false);
            middle.gameObject.SetActive(false);
            ring.gameObject.SetActive(false);
            pinky.gameObject.SetActive(false);

            return;
        }
       

        // 물체 위에 손을 댈 때
        if ((LInteractor.Candidate != null && LInteractor.Candidate.State == Oculus.Interaction.InteractableState.Hover) ||
            (RInteractor.Candidate != null && RInteractor.Candidate.State == Oculus.Interaction.InteractableState.Hover))
        {

            //Debug.Log(interactor.Candidate.name);

            index.gameObject.SetActive(false);
            middle.gameObject.SetActive(false);
            ring.gameObject.SetActive(false);
            pinky.gameObject.SetActive(false);

            //Debug.Log("selected");
            if(LInteractor.Candidate != null && LInteractor.Candidate.State == Oculus.Interaction.InteractableState.Hover)
            {
                //left.material = selected;
                remoteLeft.material = selected;
            }
            if(RInteractor.Candidate != null && RInteractor.Candidate.State == Oculus.Interaction.InteractableState.Hover)
            {
                //right.material = selected;
                remoteRight.material = selected;
            }


        }
        else
        {
            if (!tempHidden)
            {
                index.gameObject.SetActive(true);
                middle.gameObject.SetActive(true);
                ring.gameObject.SetActive(true);
                pinky.gameObject.SetActive(true);
            }

            if(LInteractor.Candidate == null)
            {
                //left.material = original;
                remoteLeft.material = original;
            }
            if (RInteractor.Candidate == null)
            {
                //right.material = original;
                remoteRight.material = original;
            }

        }

        // 선택된 경우 테두리 그리기
        if(selectedObject != null)
        {
            if (isSelected && selectedObject.GetComponent<ParticleSystem>() != null)
            {
                selectedObject.GetComponent<ParticleSystem>().Play();
            }
            
        }

        // Gaze Pointer
        if (GazeHand.instance.gazedObject != null && (
            GazeHand.instance.gazedObject.CompareTag("Object") ||
            GazeHand.instance.gazedObject.CompareTag("3DButton")
            ))
            gazePointer.color = Color.yellow;
        else
            gazePointer.color = Color.white;

    }



    bool isSelected = false;
    GameObject selectedObject;

    bool tempHidden = false;

    public IEnumerator tempHide()
    {
        index.gameObject.SetActive(false);
        middle.gameObject.SetActive(false);
        ring.gameObject.SetActive(false);
        pinky.gameObject.SetActive(false);
        tempHidden = true;


        yield return new WaitForSeconds(0.5f);


        tempHidden = false;



    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HandTrigger"))
        {
            
            Debug.Log("Gazed Object: "+ (GazeHand.instance.gazedObject == null? "None" : GazeHand.instance.gazedObject.name));


            
            if (GazeHand.instance.gazedObject != null)      // 물체를 가리킨 상태
            {
                if(GazeHand.instance.gazedObject.CompareTag("Object")) // Object이면
                { 
                    if (isSelected)         // 이미 한번 회오리가 돌고 있는 상태
                    {
                        if (selectedObject.GetComponent<ParticleSystem>() != null && 
                            selectedObject == GazeHand.instance.gazedObject)    // 다른 걸로 안 바꾼 거면 정확히 선택한 것임
                        {
                            switch (other.name)
                            {
                                case "Ring":
                                    Debug.Log("Edit");
                                    if (CoreInteraction.instance.allowsObjectSaving)
                                        CoreInteraction.instance.edit(selectedObject);
                                    else
                                        Debug.Log("NetworkEdit");
                                    break;
                                case "Middle":
                                    Debug.Log("Copy");
                                    if (CoreInteraction.instance.allowsObjectSaving)
                                        CoreInteraction.instance.copy(selectedObject);
                                    else
                                    {
                                        Debug.Log("NetworkCopy");
                                        Spawner.instance.beginCopyObj(selectedObject);
                                    }
                                        
                                    break;
                                case "Pinky":
                                    Debug.Log("Delete");
                                    if (CoreInteraction.instance.allowsObjectSaving)
                                        CoreInteraction.instance.delete(selectedObject);
                                    else
                                    {
                                        Debug.Log("NetworkDespawn");
                                        Spawner.instance.beginRemoveObj(selectedObject);
                                    }
                                        
                                    break;
                                default:        // index
                                    // 스톱워치를 응시한 경우에 한정하여 사용
                                    if (selectedObject.name[..5] == "GTime" && !CoreInteraction.isEditing)
                                    {
                                        Timer timer = selectedObject.GetComponent<Timer>();
                                        if (timer.isRunning)
                                            timer.shouldStop = true;
                                        else
                                            StartCoroutine(Timer.runTimer(timer));
                                    }

                                    // 메모를 응시한 경우에 한정
                                    else if (selectedObject.name[..5] == "GNote" && !CoreInteraction.isEditing)
                                    {
                                        Oculus.Voice.Voice_Handler handler = selectedObject.GetComponent<Oculus.Voice.Voice_Handler>();
                                        handler.ToggleActivation();
                                    }

                                    else
                                        Debug.Log("Index finger may not be used");

                                    break;

                                    
                            }
                            isSelected = false;
                            selectedObject.GetComponent<ParticleSystem>().Stop();
                            selectedObject = null;
                        }
                        else        // 하나를 선택한 상태에서 갑자기 다른 Object를 응시하여 선택한 케이스
                        {
                            selectedObject.GetComponent<ParticleSystem>().Stop();
                            selectedObject = GazeHand.instance.gazedObject;
                            return;
                        }

                    }
                    else       // 처음 선택한 상태임. (isSelected = false)
                    {
                        selectedObject = GazeHand.instance.gazedObject;
                        isSelected = true;
                    }


                }
                else if (GazeHand.instance.gazedObject.CompareTag("3DButton"))    // 이동 버튼  
                {
                    if(GazeHand.instance.gazedObject.name.StartsWith("New Meeting"))
                    {
                        PlayerPrefs.SetString("SessionName", UserManagement.loginedUserID_s);
                        PlayerPrefs.Save();
                        ARtoVRLoading.Instance.startLoadScene("VRScene");
                        //UnityEngine.SceneManagement.SceneManager.LoadScene("VRScene");
                    }
                    else if(GazeHand.instance.gazedObject.name.StartsWith("MemUp"))
                    {
                        GameObject.Find("MembersContent").transform.position -= new Vector3(0, 0.13f, 0);
                    }
                    else if (GazeHand.instance.gazedObject.name.StartsWith("MemDown"))
                    {
                        GameObject.Find("MembersContent").transform.position += new Vector3(0, 0.13f, 0);
                    }
                    else if (GazeHand.instance.gazedObject.name.StartsWith("RoomUp"))
                    {
                        GameObject.Find("RoomsContent").transform.position -= new Vector3(0, 0.18f, 0);
                    }
                    else if (GazeHand.instance.gazedObject.name.StartsWith("RoomDown"))
                    {
                        GameObject.Find("RoomsContent").transform.position += new Vector3(0, 0.18f, 0);
                    }
                    else if (GazeHand.instance.gazedObject.name.StartsWith("Meeting"))    // 이미 개설된 방
                    {
                        PlayerPrefs.SetString("SessionName", GazeHand.instance.gazedObject.name[8..]); // Meeting_ 이후
                        PlayerPrefs.Save();
                        ARtoVRLoading.Instance.startLoadScene("VRScene");
                    }
                    else
                    {
                        Debug.Log("Other menu");
                    }

                }
                else  // 벽 또는 고정된 오브젝트
                {

                    if (isSelected)         // 하나를 선택한 상태에서 허공을 본 경우
                    {
                        isSelected = false;
                        if (selectedObject != null && selectedObject.GetComponent<ParticleSystem>())
                            selectedObject.GetComponent<ParticleSystem>().Stop();   // 그 전에 선택한 것 해제
                        selectedObject = null;
                        return;
                    }
                    else                  // 선택하지 않은 상태에서 허공을 본 경우
                    {

                        if (GazeHand.instance.gazedObject.CompareTag("FixedObject") && isLongRing) // 고정된 오브젝트인 경우 고정 해제
                        {
                            Debug.Log("Unfix " + GazeHand.instance.gazedObject.name);
                            GazeHand.instance.gazedObject.tag = "Object";
                            GazeHand.instance.gazedObject.GetComponent<Oculus.Interaction.HandGrab.HandGrabInteractable>().enabled = true;
                        }

                        if (GazeHand.instance.gazedObject.CompareTag("FixedObject") && isLongIndex) // 오래 누르면 호출
                        {
                            if (GazeHand.instance.gazedObject.name[..5] == "GTime" && !CoreInteraction.isEditing)
                            {
                                Timer timer = GazeHand.instance.gazedObject.GetComponent<Timer>();
                                if (timer.isRunning)
                                    timer.shouldStop = true;
                                else
                                    StartCoroutine(Timer.runTimer(timer));

                                return; // 스케치 실행 방지
                            }

                            // 메모를 응시한 경우에 한정
                            else if (GazeHand.instance.gazedObject.name[..5] == "GNote" && !CoreInteraction.isEditing)
                            {
                                Oculus.Voice.Voice_Handler handler = GazeHand.instance.gazedObject.GetComponent<Oculus.Voice.Voice_Handler>();
                                handler.ToggleActivation();

                                return; // 스케치 실행 방지
                            }

                            else
                                Debug.Log("Index finger may not be used");

                            
                        }


                        switch (other.name)
                        {
                            case "Middle":
                                Debug.Log("Create an Object");      // 3D 물체, 메모, 
                                if (CoreInteraction.instance.allowsObjectSaving)
                                    CoreInteraction.instance.create();
                                else
                                    StartCoroutine(Spawner.instance.select());
                                break;

                            case "Ring":                            // 고정된 물체의 고정 해제
                                Debug.Log("Not yet used");
                                break;
                            case "Pinky":
                                Debug.Log("Settings");

                                // 임시
                                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "VRScene")
                                {
                                    if(isLongPinky)  finish();
                                }

                                CoreInteraction.instance.settings();
                                break;
                            default:        // index

                                if (CoreInteraction.instance.allowsObjectSaving)
                                {
                                    if (isLongIndex)
                                    {
                                        Debug.Log("Create Sketch");         // 3D 스케치
                                        CoreInteraction.instance.sketch();
                                    }
                                    else
                                        Debug.Log("Sketch not started due to short touch");
                                }
                                else
                                {
                                    Debug.Log("Sketch for networking");
                                    Spawner.instance.spawnSketch();
                                  
                                }
                                break;
                        }
                    }



                }

            }
            else    // 완전 허공
            {
                if (isSelected)
                {
                    isSelected = false;
                    if (selectedObject != null) selectedObject.GetComponent<ParticleSystem>().Stop();
                    selectedObject = null;
                }
                else                 // 선택하지 않은 상태에서 허공을 본 경우
                {
                    switch (other.name)
                    {
                        case "Middle":
                            Debug.Log("Create an Object");      // 3D 물체, 메모, 
                            CoreInteraction.instance.create();
                            break;
                        case "Ring":
                            Debug.Log("Not yet used");
                            break;
                        case "Pinky":
                            Debug.Log("Settings");


                            // 임시
                            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "VRScene")
                            {
                                if (isLongPinky) finish();
                            }
                                

                            CoreInteraction.instance.settings();
                            break;
                        default:


                            if (CoreInteraction.instance.allowsObjectSaving)
                            {
                                if (isLongIndex)
                                {
                                    Debug.Log("Create Sketch");         // 3D 스케치
                                    CoreInteraction.instance.sketch();
                                }
                                else
                                    Debug.Log("Sketch not started due to short touch");
                            }
                            else
                            {
                                Debug.Log("Sketch for networking");
                                Spawner.instance.spawnSketch();
                                
                            }


                            break;
                    }
                }



            }


            StartCoroutine(tempHide());

        }  
    }


    async void finish()
    {
        //if (Spawner.instance._runner.IsServer)
            await Spawner.instance._runner.Shutdown();

       // ARtoVRLoading.Instance.startLoadScene("SampleScene");

    }


    public static bool isPinching = false, isMiddle = false, isRing = false, isPinky = false;


    public static double longRing, longIndex, longPinky = 0; // 고정 해제를 위한 롱클릭 방법
    public static bool isLongRing { get { return longRing > 1; } }
    public static bool isLongIndex { get { return longIndex > 1; } }
    public static bool isLongPinky { get { return longPinky > 1; } }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "PinchDetector")
        {
            isPinching = true;
            longIndex += Time.deltaTime;
        }
        if (other.gameObject.name == "MiddleDetector")
        {
            isMiddle = true;
        }
        if (other.gameObject.name == "RingDetector")
        {
            isRing = true;
            longRing += Time.deltaTime;
            
        }
        if (other.gameObject.name == "PinkyDetector")
        {
            isPinky = true;
            longPinky += Time.deltaTime;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PinchDetector")
        {
            isPinching = false;
            longIndex = 0;
        }
        if (other.gameObject.name == "MiddleDetector")
        {
            isMiddle = false;
        }
        if (other.gameObject.name == "RingDetector")
        {
            isRing = false;
            longRing = 0;
        }
        if (other.gameObject.name == "PinkyDetector")
        {
            isPinky = false;
            longPinky = 0;
        }
    }
}
