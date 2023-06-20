using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeRay
{
    public Vector3 origin;
    public Vector3 direction;
    public float length { get; set; } = 1;

    public GazeRay(Vector3 o, Vector3 d, float l = 1)
    {
        origin = o;
        direction = d;
        length = l;
    }


}


public class EyeTest : MonoBehaviour
{
    public GameObject localCamera;
    public OVREyeGaze leftEye, rightEye;
    public LineRenderer eyeGaze;
    public float length;

    public GazeRay leftEyeRay
    {
        get
        {
            return new GazeRay(
                leftEye.gameObject.transform.position,
                leftEye.gameObject.transform.forward);
        }

    }

    public GazeRay rightEyeRay
    {
        get
        {
            return new GazeRay(
                rightEye.gameObject.transform.position,
                rightEye.gameObject.transform.forward);
        }

    }


    GazeRay lastRay;
    public GazeRay centerEyeRay
    {
        get
        {
            GazeRay ray = new GazeRay(localCamera.transform.position,       // GH 적용을 위해 로컬 카메라를 눈의 시점으로 지정
                (leftEyeRay.direction + rightEyeRay.direction) / 2);

            if(lastRay == null)
                lastRay = new GazeRay(localCamera.transform.position,
                (leftEyeRay.direction + rightEyeRay.direction) / 2);

            // 보정
            ray.direction = Vector3.Lerp(lastRay.direction, ray.direction, 0.1f);

            // 보정 후 저장
            lastRay.origin = ray.origin;
            lastRay.direction = ray.direction;
            return ray;
        }
    }


    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"Left: {leftEye.enabled}, {leftEye.ConfidenceThreshold}");
        //Debug.Log($"Right: {rightEye.enabled}, {rightEye.ConfidenceThreshold}");
        //Debug.Log($"Left: {leftEyeRay.origin}, {leftEyeRay.direction} / Right: {rightEyeRay.origin}, {rightEyeRay.direction} ");
        //Debug.Log($"Center: {centerEyeRay.origin}, {centerEyeRay.direction}");

        GazeRay ray = centerEyeRay;

        ray.length = length;

        eyeGaze.SetPosition(0, ray.origin);
        eyeGaze.SetPosition(1, ray.origin + ray.direction * ray.length);
    }
}
