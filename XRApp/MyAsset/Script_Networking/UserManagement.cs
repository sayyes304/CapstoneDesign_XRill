using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class Users
{
    public List<UserDB> users;
}


[System.Serializable]
public class UserDB
{
    public string id;
    public string pw;
    public string name;
    public int connectionID;

    public override string ToString()
    {
        return id;
    }
}

public class Cert : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        char[] d = new char[certificateData.Length];
        certificateData.CopyTo(d, 0);
        Debug.Log("Cert[[["+new string(d)+"]]]");
        return true;
    }
}


public class UserManagement : MonoBehaviour
{
    [Header("유저 이름")]
    public string loginedUserID;
    public UserDB loginedUserInfo;

    public static string loginedUserID_s;
    public static UserDB loginedUserInfo_s;

    public TMP_Text label;  // 우측 위 사용자 이름

    public GameObject roomBox;
    public GameObject memberBox;
    public Transform roomView, memberView;


    //public static readonly string url = "https://192.168.79.9/json";
    public static readonly string url2 = "https://192.168.79.9/test.txt";

    static Users temp;
    public static bool available = false;

    public static Dictionary<string, UserDB> getDB()
    {
        Dictionary<string, UserDB> db = new Dictionary<string, UserDB>();

        if(!available) return db;

        foreach(var u in temp.users)
        {
            db.Add(u.id, u);
        }
        return db;
    }


    // Start is called before the first frame update
    void Start()
    {
        loginedUserID_s = loginedUserID;

        if (loginedUserID.Length == 0)
        {
            Debug.Log("아이디를 입력하세요!!!!!!!!!");
            return;
        }
            

        StartCoroutine(download2(url2));
    }


    IEnumerator download2(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.certificateHandler = new Cert();
        yield return request.SendWebRequest(); // 응답이 올 때까지 다음으로 넘어가지 않음

        //Debug.Log(request.responseCode);

        // 결과가 성공인 경우
        if (request.result == UnityWebRequest.Result.Success
            && request.responseCode == 200)
        {
            Debug.Log(request.downloadHandler.text);

            temp = JsonUtility.FromJson<Users>("{\"users\": "+request.downloadHandler.text+"}");

            available = true;
        }
        else Debug.Log("Download failed");
    }

    // Update is called once per frame
    void Update()
    {
        
        if (available)
        {
            var db = getDB();
            loginedUserInfo = db[loginedUserID];
            loginedUserInfo_s = loginedUserInfo;

            label.text = loginedUserID;

            PlayerPrefs.SetString("Username", loginedUserID);
            PlayerPrefs.SetInt("ConnID", loginedUserInfo.connectionID);
            PlayerPrefs.Save();

            foreach(var d in db.Values)
            {
                if (d.id != loginedUserID)
                {
                    var b = Instantiate(memberBox, memberView);
                    b.transform.Find("Name").GetComponent<TMP_Text>().text = d.id;
                }
            }

            //foreach(var d in db) Debug.Log(JsonUtility.ToJson(d.Value));
            available = false;
        }

        if (ARLobby.shouldBeChanged)
        {
            for(int i=roomView.transform.childCount-1; i>=0; i--)
            {
                Destroy(roomView.transform.GetChild(i).gameObject);
            }

            foreach (var d in ARLobby.sessionList)
            {
                var b = Instantiate(roomBox, roomView);
                b.name = "Meeting_"+d.Name;
                b.transform.Find("RName").GetComponent<TMP_Text>().text = d.Name;
                b.transform.Find("Ppl").GetComponent<TMP_Text>().text = $"{d.PlayerCount} ppl.";
            }

            ARLobby.shouldBeChanged = false;
        }
    }
}
