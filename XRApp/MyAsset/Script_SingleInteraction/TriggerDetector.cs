using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{

    public bool touched;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OptionTrigger"))
        {
            touched = true;
            Debug.Log(name + " pushed");
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("OptionTrigger"))
        {
            touched = false;
        }
    }
}
