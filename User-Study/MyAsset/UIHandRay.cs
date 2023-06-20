using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHandRay : MonoBehaviour
{
    public OVRHand hand;
    public OVRInputModule inputmodule;
    // Start is called before the first frame update
    void Start()
    {
        inputmodule.rayTransform = hand.PointerPose;   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
