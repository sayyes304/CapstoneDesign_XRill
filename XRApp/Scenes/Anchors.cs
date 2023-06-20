using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchors : MonoBehaviour
{
    public const string NumUuidsPlayerPref = "numUuids";

    public OVRSpatialAnchor _spatialAnchor;

    private void Awake()
    {
        _spatialAnchor = GetComponent<OVRSpatialAnchor>();
    }

    private IEnumerator Start()
    {
        while (_spatialAnchor && !_spatialAnchor.Created)
        {
            yield return null;
        }

        if (_spatialAnchor)
        {

        }
        else
        {
            // Creation must have failed
            Destroy(gameObject);
        }

        
        if (!PlayerPrefs.HasKey("uuid"))
        {
            print("µÚÁú·¡ ? : " + PlayerPrefs.GetInt(NumUuidsPlayerPref));
            OnSaveLocal();
        }
            

    }

    public void OnSaveLocal()
    {
        if (!_spatialAnchor) return;

        _spatialAnchor.Save((anchor, success) =>
        {
            if (!success) return;

            // Write uuid of saved anchor to file
            if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
            {
                PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
            }

            int playerNumUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            PlayerPrefs.SetString("uuid", anchor.Uuid.ToString());
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 1);
        });
    }
}
