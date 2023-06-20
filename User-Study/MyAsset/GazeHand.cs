using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;


// Eye-GazeHand with Quest Pro
public class GazeHand : MonoBehaviour
{
    public GameObject ovrCameraRig, centerEyeAnchor, leftHandAnchor, rightHandAnchor;           // 본체, 물체를 잡는 손
    public GameObject localCamera, localLeftHandAnchor, localRightHandAnchor, localPivot;       // 로컬, 실제 손
    public GameObject handGrabInteractorL, handGrabInteractorR;                                 // 물체 잡기 도구
    HandGrabInteractable selectedObjectL, selectedObjectR;                                      // 잡힌 물체

    public EyeTest eye;
    public Transform pointer;
    public float dist = 0.1f;

    float length;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
        createPointer();
        setHand();

    }




    Collider lastCollider;

    void createPointer()
    {
        
        GazeRay ray = eye.centerEyeRay;

        RaycastHit[] raycastHits =  Physics.RaycastAll(ray.origin, ray.direction);
        System.Array.Sort(raycastHits, (a, b) => {      // 거리 내림차순으로 정렬
            if (a.distance < b.distance) return 1;      // 오름차순: -1
            else if (a.distance > b.distance) return -1; // 오름차순: 1
            else return 0;
        });
        

        // 지금 잡고 있는 물체
        selectedObjectL = handGrabInteractorL.GetComponent<HandGrabInteractor>().SelectedInteractable;
        selectedObjectR = handGrabInteractorR.GetComponent<HandGrabInteractor>().SelectedInteractable;

        if(selectedObjectL != null || selectedObjectR != null)
            Debug.Log($"Selected: {(selectedObjectL != null ? selectedObjectL.gameObject.name : "None")}, " +
            $"{(selectedObjectR != null ? selectedObjectR.gameObject.name : "None")}");

        // 하나 이상 충돌
        if (raycastHits.Length > 0)
        {
            int index = raycastHits.Length-1;       // 주의! 역순으로 뜸!      카메라 -->  4   3   2   1   0

            string s = "";
            for(int i=0; i<raycastHits.Length; i++)
            {
                s += i + ": " + raycastHits[i].collider.gameObject.name + "\t";
            }
            Debug.Log(s);

            // 선상에 있는 모든 오브젝트 검사
            do
            {
                if (raycastHits[index].collider.gameObject.CompareTag("User")                  // 사용자 손은 인식 X

                    // 만약 바로 뒤로 보내고 싶으면 아래 2개 주석 풀기
                //|| (selectedObjectL != null && raycastHits[index].collider.gameObject == selectedObjectL.gameObject)        // 왼손 물체
                //|| (selectedObjectR != null && raycastHits[index].collider.gameObject == selectedObjectR.gameObject)       // 오른손 물체
                )
                {
                    index--;
                    continue;
                }
                break;

            } while (index >= 0);

            if(index < 0)    // No raycasted except user && grabbed
            {
                pointer.position = ray.origin + ray.direction * length;
                //pointer.LookAt(ray.direction);
            }
            else                                // raycasted
            {
                if(raycastHits[index].collider != lastCollider)
                {
                    length = raycastHits[index].distance;
                    ray.length = length;
                    
                    /* - ray.direction * 0.05f*/
                    //pointer.LookAt(raycastHits[index].normal);
                    // Debug.Log(raycastHits[0].collider.gameObject.name);
                }
                //pointer.position = raycastHits[index].point;
                pointer.position = ray.origin + ray.direction * length;
                lastCollider = raycastHits[index].collider;
            }

           
        }
        else    // No raycasted
        {
            pointer.position = ray.origin + ray.direction * length;
            //pointer.LookAt(ray.direction);
        }

        pointer.LookAt(localPivot.transform);
    }

    void setHand()
    {   
        // Gaze pointer로 본체(=실제 손) 이동
        ovrCameraRig.transform.position = pointer.transform.position - eye.centerEyeRay.direction * dist;

        // 카메라의 위치 = 희망하는 기준점 + (원래 눈 위치 - 원래 카메라 위치)
        // 고개 돌림 보정
        localCamera.transform.SetPositionAndRotation(
            centerEyeAnchor.transform.position - ovrCameraRig.transform.position + localPivot.transform.position,
            centerEyeAnchor.transform.rotation);   // 본체와 같은 회전

        // 실제 손 위치로 이동
        localLeftHandAnchor.transform.SetPositionAndRotation(
            leftHandAnchor.transform.position - ovrCameraRig.transform.position + localPivot.transform.position,
            leftHandAnchor.transform.rotation);
        localRightHandAnchor.transform.SetPositionAndRotation(
            rightHandAnchor.transform.position - ovrCameraRig.transform.position + localPivot.transform.position - new Vector3(0.4f, 0, 0),
            rightHandAnchor.transform.rotation);

        //offset = gazePointer.transform.position - localCamera.transform.position;
        //Debug.Log(offset);
    }


}
