using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VirtualObjectInfo
{

    //public string objectName;   // 물체 이름(선택?)
    public int objectID;        // 고유 번호
    public int objectType;      // 물체 종류

    public Vector3 initialScale;    // 최초 scale
    public float scale = 1;             // 거기에 얼마를 곱했는가
    public Vector3 position;
    public Quaternion rotation;

    public bool isFix = false;  // 물체 고정

    // type-specific

    // all except sketch
    public int color;   // 0~7 순서대로 빨 주 노 초 파 보 흰 검

    // all except subsketch
    public bool independent = true;    // 독립 객체인가(예: 스케치 조각은 독립 객체가 아님)

    // timer
    public int timer_seconds;

    // clock
    public int clock_region;

    // sketch
    public Vector3 minV, maxV;
    public List<string> sketch_child = new List<string>();   // 딸린 자식이 있는 경우 그 id를 기록

    // subsketch
    public float width;             // 스케치 두께
    public int parent;              // 부모 스케치
    public List<Vector3> sketch_dots = new List<Vector3>();     // 점의 좌표

    // note
    public string note_string;      // 메모 텍스트

    // screen

    // calender


    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }


    //public VirtualObjectInfo this[int i]
    public string this[int i]
    {
        get
        {
            return sketch_child[i];
            //return JsonUtility.FromJson<VirtualObject>(child[i]);
        }
        set
        {
            sketch_child[i] = value;
            //child[i] = value.ToString();
        }
    }



    /*
    public void setValue(VirtualObjectInfo other)
    {
        objectID = other.objectID;        // 고유 번호
        objectType = other.objectType;      // 물체 종류

        initialScale = other.initialScale;    // 최초 scale
        scale = other.scale;             // 거기에 얼마를 곱했는가
        position = other.position;
        rotation = other.rotation;

        color = other.color;   // 0~7 순서대로 빨 주 노 초 파 보 흰 검
        isFix = other.isFix;  // 물체 고정

        // type-specific
        // all except subsketch
        independent = other.independent;    // 독립 객체인가(예: 스케치 조각은 독립 객체가 아님)

        // timer
        timer_seconds = other.timer_seconds;

        // clock
        clock_region = other.clock_region;

        // sketch
        sketch_child = other.sketch_child;   // 딸린 자식이 있는 경우 그 id를 기록
        sketch_dots = other.sketch_dots;

        // note
        note_string = other.note_string;      // 메모 텍스트

    }
    */

    
    
}
