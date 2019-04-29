using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Video;

public class AR_Manager : MonoBehaviour
{
    private IPEndPoint remoteEndPoint;
    private UdpClient udpClient, udpClientReceiver;

    private Thread udpListenerThread;
    private int PORT;

    private Vector3 pivotPos;
    private Vector3 oldPos;
    private Vector3 oldScale;
    private Vector3 initialScale = new Vector3(1, 1, 1);
    private Quaternion oldRot;
    private bool isShowAtPivot = false;

    private string[] arInfo;
    private string rawData;
    private int isDetected, markerId;

    public List<int> avaliableMarkerIds;
    public List<GameObject> arObjects;
    public List<VideoClip> arVideo;
    public VideoClip noVideo;
    public static VideoClip choosedVideo;

    public GameObject arPivot;

    int index;
    //-------------------------------------------------------
    private void Update()
    {
        if (SceneManager.tcpServer.receivedData != null)
        {
            rawData = SceneManager.tcpServer.receivedData;
            Debug.Log(rawData);
            arInfo = rawData.Split(',');
            isDetected = int.Parse(arInfo[0]);  // status of detection
            if (arInfo[1] != "None")  
                markerId = int.Parse(arInfo[1]);  // id of ar_marker

            if (isDetected == 1 && SceneManager.isArEnable)
            {
                ShowArObject(markerId);
            }
            else if (isDetected == 0)
            {
                HideArObject(markerId);
            }
            SceneManager.tcpServer.receivedData = null;
        }
        
    }

    public void ShowArObject(int id)
    {
        if (IsInvoking("SetOriginalTransform"))
            CancelInvoke("SetOriginalTransform");

        foreach (int i in avaliableMarkerIds)
        {
            if (i == id)
            {
                pivotPos = arPivot.transform.position;
                index = avaliableMarkerIds.IndexOf(i);

                // If don't have video clip, It will show "noVideo" instead.
                if (arVideo[index] != null)
                    choosedVideo = arVideo[index];
                else
                    choosedVideo = noVideo;

                // If robot don't detect any AR marker in 3 seconds, it will reset position & scale.
                if (isShowAtPivot)
                {
                    arObjects[index].transform.position = pivotPos;
                    arObjects[index].transform.localScale = initialScale;
                    isShowAtPivot = false;
                }
                else
                {
                    arObjects[index].transform.localScale = oldScale;
                    arObjects[index].transform.position = oldPos;
                }
                   
                arObjects[index].SetActive(true);
            }
        }
    }

    private void HideArObject(int id)
    {
        oldPos = arObjects[index].transform.position;
        oldScale = arObjects[index].transform.localScale;
        arObjects[index].SetActive(false);

        // if not detect any markers in 3 seconds, program will set new position of AR object
        Invoke("SetOriginalTransform", 3f);
    }

    private void SetOriginalTransform()
    {
        isShowAtPivot = true;
    }
}
