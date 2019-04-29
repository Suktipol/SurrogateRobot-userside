using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.Extras;
using Valve.VR;
using UnityEngine.UI;

// Script Location(Hierarchy) : [CameraRig] > Camera > LeftCamera > FIBO_FLOOR2_UI > All_Station

public class ExtendUI : MonoBehaviour {
    public Image stationPic;
    public List<Sprite> stationTexture;   

    float sizeRatio = 0;
    bool laserInState = false;

    void OnEnable()
    {
        SteamVR_LaserPointer.PointerIn += LaserInStation;
        SteamVR_LaserPointer.PointerOut += LaserOutStation;
        SteamVR_LaserPointer.PointerClick += LaserClickStation;
    }

    void OnDisable()
    {
        SteamVR_LaserPointer.PointerIn -= LaserInStation;
        SteamVR_LaserPointer.PointerOut -= LaserOutStation;
        SteamVR_LaserPointer.PointerClick -= LaserClickStation;
    }

    void LaserInStation(object sender, PointerEventArgs e)
    {
        if (e.target.transform.CompareTag("Station"))
        {
            float startTime = Time.deltaTime;
            string stationName = e.target.name;
            int n = int.Parse(stationName.Replace("Station", ""));

            stationPic.sprite = stationTexture[n - 1];
            laserInState = true;

            e.target.GetComponent<Renderer>().material.color = Color.red;
        }     
    }

    void Update()
    {
        if (laserInState == true){
            if (sizeRatio < 1){
                sizeRatio += Time.deltaTime * 3.5f;
                stationPic.rectTransform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(1f, 1f, 0), sizeRatio);
            }
        }
        else { 
            if (sizeRatio > 0){
                sizeRatio -= Time.deltaTime * 3.5f;
                stationPic.rectTransform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(1f, 1f, 0), sizeRatio);
            }
        }
    }

    /// <summary>
    /// When click at the station 
    ///     1. FIBO_FLOOR2_UI will disappear.
    ///     2. TCP will send the index of each station as data.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void LaserClickStation(object sender, PointerEventArgs e)
    {
        if (e.target.CompareTag("Station"))
        {
            transform.parent.gameObject.SetActive(false);
            SceneManager.isFloor2UiShow = false;

            string stationNum = e.target.name;
            stationNum = stationNum.Replace("Station", "");

            Debug.Log("Choose Station : " + stationNum);

            //Code to send number of station in TCP is here
            SceneManager.tcpServer.TCPSendMessage(stationNum);

            e.target.GetComponent<Renderer>().material.color = Color.white;
        }
    } 

    void LaserOutStation(object sender, PointerEventArgs e)
    {
        if (e.target.transform.CompareTag("Station"))
        {
            e.target.GetComponent<Renderer>().material.color = Color.white;
            laserInState = false;          
        }
    }

}
