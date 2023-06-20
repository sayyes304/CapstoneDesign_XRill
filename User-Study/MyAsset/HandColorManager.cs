using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HandColorManager : MonoBehaviour
{
    public Oculus.Interaction.HandGrab.HandGrabInteractor LInteractor, RInteractor;

    public SkinnedMeshRenderer left, right, remoteLeft, remoteRight;
    public Material original, selected;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // 물체 위에 손을 댈 때
            if (LInteractor.Candidate != null && LInteractor.Candidate.State == Oculus.Interaction.InteractableState.Hover)
            {
                left.material = selected;
                remoteLeft.material = selected;
            }
            if (RInteractor.Candidate != null && RInteractor.Candidate.State == Oculus.Interaction.InteractableState.Hover)
            {
                right.material = selected;
                remoteRight.material = selected;
            }

            if (LInteractor.Candidate == null)
            {
                left.material = original;
                remoteLeft.material = original;
            }
            if (RInteractor.Candidate == null)
            {
                right.material = original;
                remoteRight.material = original;
            }


        if (LInteractor.IsGrabbing)
        {
            string name = LInteractor.SelectedInteractable.gameObject.name;
            switch (name)
            {
                case "GazeHand":
                    {
                        SceneManager.LoadScene("GazeHand_Prior");
                        break;
                    }
                case "HandRay":
                    {
                        SceneManager.LoadScene("Hand_Prior");
                        break;
                    }
                case "ControllerRay":
                    {
                        SceneManager.LoadScene("Controller_Prior");
                        break;
                    }
                default: break;
            }

        }

        if (RInteractor.IsGrabbing)
        {
            string name = RInteractor.SelectedInteractable.gameObject.name;
            switch (name)
            {
                case "GazeHand":
                    {
                        SceneManager.LoadScene("GazeHand_Prior");
                        break;
                    }
                case "HandRay":
                    {
                        SceneManager.LoadScene("Hand_Prior");
                        break;
                    }
                case "ControllerRay":
                    {
                        SceneManager.LoadScene("Controller_Prior");
                        break;
                    }
                default: break;
            }

        }
    }

}