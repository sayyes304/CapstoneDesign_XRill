using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class CubeController : MonoBehaviour
{
    [SerializeField]
    static public int count = 0;
    static public float time, completiontime;
    [SerializeField] bool isStart = false;

    public GameObject testCubes;

    public void hideCube()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("green_cube") || other.CompareTag("blue_cube") || other.CompareTag("red_cube"))
            if (other.CompareTag(gameObject.tag))
            {
                other.gameObject.SetActive(false);
                count++;
            }
        print("Count : " + count);


        if (count == 6)
        {
            completiontime = time;
            print("Complete : " + completiontime);

            SaveCSV();
            isStart = false;

        }

    }

    void SaveCSV()
    {
        FileStream fs = new FileStream("Assets/result.csv", FileMode.Append, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

        sw.WriteLine(SceneManager.GetActiveScene().name + "," + completiontime.ToString());

        sw.Close();

        count++;
    }

    private void Update()
    {
        if(isStart)
            time += Time.deltaTime;
    }

    public void toggleStart()
    {
        isStart = true;
        testCubes.SetActive(true);
    }
}
