using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Region { Seoul, Sydney, Washington, London}
public class Clock : MonoBehaviour
{
    public Region region = Region.Seoul;
    public TMPro.TMP_Text clock;
    public SpriteRenderer sprite;
    public Sprite am, pm;

    public int curHour;

    // UTC +-
    public static readonly int seoulTime = 9,
        sydneyTime = 10,
        washingtonTime = -4,
        londonTime = 1;

    public static int regionTime(Region region)
    {
        switch (region)
        {
            case Region.Sydney:
                return sydneyTime;
            case Region.Washington:
                return washingtonTime;
            case Region.London:
                return londonTime;
            case Region.Seoul:
            default:
                return seoulTime;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {


        clock.text = timeStr(region);

        curHour = hour(region);

        if (isAM(region))
            sprite.sprite = am;
        else
            sprite.sprite = pm;

    }

    public static string timeStr(Region t)
    {
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.Now).AddHours(regionTime((Region)t)).ToString("hh mm"); // tt (AM/PM) Á¦°Å
    }

    public int hour(Region t)
    {
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.Now).AddHours(regionTime((Region)t)).Hour;
    }

    public bool isAM(Region t)
    {
        return hour(t) < 12;
    }
}
