using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using UnityEngine.SceneManagement;
using System.IO;

public class InteractorManager : MonoBehaviour
{
    public Material original, selected;
    [Optional] public SkinnedMeshRenderer LControllerMesh, RControllerMesh, LHandMesh, RHandMesh;

    public void GazeHandScene()
    {
        SceneManager.LoadScene("GazeHand_Prior");
    }

    public void HandScene()
    {
        SceneManager.LoadScene("Hand_Prior");
    }

    public void ControllerScene()
    {
        SceneManager.LoadScene("Controller_Prior");
    }

    [SerializeField]
    protected int count = 0;
    public float time, completiontime;
    bool isStart = false;

    public GameObject cubes;
    public void CubeManager()
    {
        count++;
        if (count == 4)
        {
            completiontime = time;
            print("Complete : " + completiontime);
            SaveCSV();
            isStart = false;
        }

    }
    private void Start()
    {
        cubes.SetActive(false);
    }


    void SaveCSV()
    {
        FileStream fs = new FileStream("Assets/result.csv", FileMode.Append, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

        sw.WriteLine(SceneManager.GetActiveScene().name + "," + completiontime.ToString());

        sw.Close();

        count++;
    }

    public void toggleStart()
    {
        isStart = true;
        cubes.SetActive(true);
    }

    private void Update()
    {
        if(isStart)
            time += Time.deltaTime;
    }

    public void HandHover()
    {
        LHandMesh.material = selected;
        RHandMesh.material = selected;
    }

    public void HandUnHover()
    {
        LHandMesh.material = original;
        RHandMesh.material = original;
    }


    public void ControllerHover()
    {
        LControllerMesh.material = selected;
        RControllerMesh.material = selected;
    }

    public void ControllerUnHover()
    {
        LControllerMesh.material = original;
        RControllerMesh.material = original;
    }
}
