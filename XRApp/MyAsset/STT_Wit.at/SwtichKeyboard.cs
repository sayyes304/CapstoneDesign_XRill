using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwtichKeyboard : MonoBehaviour
{
    public GameObject VoiceTxt;
    public GameObject KeyboardTxt;

    void Start()
    {
        KeyboardTxt.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickKeyboard()
    {
        KeyboardTxt.SetActive(true);
        VoiceTxt.SetActive(false);


    }
}
