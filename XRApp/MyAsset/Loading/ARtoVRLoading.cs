using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ARtoVRLoading : MonoBehaviour
{
    public static ARtoVRLoading Instance;
    public CanvasGroup loadingUI;
    public string loadingSceneName;
    public string message;
    public TMPro.TMP_Text msgText;

    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
            Instance = this;

        loadingUI.alpha = 1;
        switch (SceneManager.GetActiveScene().name)
        {
            case "VRScene":
                message = "VR 회의장으로 들어가는 중...";
                break;
            case "SampleScene":
            default:
                message = "AR 환경으로 돌아오는 중...";
                break;
        }
        msgText.text = message;

       


        StartCoroutine(fade(1));

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startLoadScene(string scene)
    {

        // 씬 전환하기 전에 이전의 PC 공유 화면의 연결을 끊어야 안 멈춤(시그널링 강제 종료)
        

        while (GameObject.Find("GScreenPrefab(Clone)"))
        {
            var g = GameObject.Find("GScreenPrefab(Clone)");
            Debug.Log("try to destroy renderStreamingSignaling");
            g.GetComponent<Unity.RenderStreaming.Samples.ReceiverSampleForUs>().renderStreaming.Stop();
            g.SetActive(false);
        }
            
        while (GameObject.Find("GScreenPrefab"))
        {
            var g = GameObject.Find("GScreenPrefab");
            Debug.Log("try to destroy renderStreamingSignaling");
            g.GetComponent<Unity.RenderStreaming.Samples.ReceiverSampleForUs>().renderStreaming.Stop();
            GameObject.Find("GScreenPrefab").SetActive(false);
        }
        

        

        switch (scene)
        {
            case "VRScene":
                message = "VR 회의장으로 들어가는 중...";
                break;
            case "SampleScene":
            default:
                message = "AR 환경으로 돌아오는 중...";
                break;
        }
        msgText.text = message;

        loadingSceneName = scene;
        SceneManager.sceneLoaded += afterLoad;

        StartCoroutine(load(scene));
    }



    IEnumerator load(string scene)
    {
        Debug.Log("Start loading " + scene);

        StartCoroutine(fade(0));

        yield return new WaitForSeconds(1);

        AsyncOperation op = SceneManager.LoadSceneAsync(scene);
        //op.allowSceneActivation = false;

        while (!op.isDone)
        {
            yield return null;
        }

        //op.allowSceneActivation = true;
    }

    IEnumerator fade(int dir)
    {
        float t = 0;

        while(t < 1)
        {
            yield return null;
            t += Time.deltaTime;

            if (dir == 0)   // 요청 시작
                loadingUI.alpha = Mathf.Lerp(0, 1, t);
            else if (dir == 1)   // 요청 끝
                loadingUI.alpha = Mathf.Lerp(1, 0, t);
        }


    }
    private void afterLoad(Scene scene, LoadSceneMode arg1)
    {
        if (scene.name == loadingSceneName)
        {
            //StartCoroutine(fade(1));
            
            //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            SceneManager.sceneLoaded -= afterLoad;
        }
    }



    IEnumerator loadingVR()
    {
        yield return null;
    }

}
