using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // Start is called before the first frame update
    public int seconds;         // 설정된 시간
    public int currentSeconds;  // 현재 흘러가는 시간
    public TMPro.TMP_Text time;

    // controller
    public bool isRunning = false;
    //public bool paused = false;
    public bool shouldStop = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time.text = formatting(currentSeconds);
        /*
        if (paused)
            time.color = Color.gray;
        else
            time.color = Color.white;
        */

       
    }

    private void OnEnable()
    {
        if (isRunning && !shouldStop) StartCoroutine(runTimer(this));   // 복사된 경우...
    }

    public static string formatting(int sec)
    {
        int min = sec / 60; // over 60min
        int seconds = sec % 60;
        return $"{min:D2}:{seconds:D2}";
    }

    public static IEnumerator runTimer(Timer timer)
    {
        timer.currentSeconds = timer.seconds;
        timer.isRunning = true;

        while(timer.currentSeconds > 0)
        {

            // 타이머 일시정지 (보류)
           // yield return new WaitWhile(() => timer.paused);

            // 타이머 종료
            if (timer.shouldStop)
            {
                timer.currentSeconds = 0;
                timer.shouldStop = false;
                break;
            }

            timer.currentSeconds -= 1;
            yield return new WaitForSeconds(1);
        }

        timer.isRunning = false;
    }
}
