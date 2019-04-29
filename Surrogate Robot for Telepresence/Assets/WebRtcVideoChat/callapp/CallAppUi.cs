/* 
 * Copyright (C) 2015 Christoph Kutza
 * 
 * Please refer to the LICENSE file for license information
 */
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Byn.Media;
using Byn.Media.Native;

/// <summary>
/// This class + prefab is a complete app allowing to call another app using a shared text or password
/// to meet online.
/// 
/// It supports Audio, Video and Text chat. Audio / Video can optionally turned on/off via toggles.
/// 
/// After the join button is pressed the (first) app will initialize a native webrtc plugin 
/// and contact a server to wait for incoming connections under the given string.
/// 
/// Another instance of the app can connect using the same string. (It will first try to
/// wait for incoming connections which will fail as another app is already waiting and after
/// that it will connect to the other side)
/// 
/// The important methods are "Setup" to initialize the call class (after join button is pressed) and
/// "Call_CallEvent" which reacts to events triggered by the class.
/// 
/// Also make sure to use your own servers for production (uSignalingUrl and uStunServer).
/// 
/// NOTE: Currently, only 1 to 1 connections are supported. This will change in the future.
/// </summary>
public class CallAppUi : MonoBehaviour
{

    /// <summary>
    /// Texture of the local video
    /// </summary>
    protected Texture2D mLocalVideoTexture = null;

    /// <summary>
    /// Texture of the remote video
    /// </summary>
    protected Texture2D mRemoteVideoTexture = null;

    public GameObject leftQuad;
    public GameObject rightQuad;

    /*[Header("Video panel elements")]
    public RawImage uLocalVideoImage;*/

    [Header("Resources")]
    public Texture2D uNoCameraTexture;

    protected bool mFullscreen = false;


    protected CallApp mApp;


    private float mVideoOverlayTimeout = 0;

    private bool mHasLocalVideo = false;
    private int mLocalVideoWidth = -1;
    private int mLocalVideoHeight = -1;
    private int mLocalFps = 0;
    private int mLocalFrameCounter = 0;
    private FramePixelFormat mLocalVideoFormat = FramePixelFormat.Invalid;

    private bool mHasRemoteVideo = false;
    private int mRemoteVideoWidth = -1;
    private int mRemoteVideoHeight = -1;
    private int mRemoteFps = 0;
    private int mRemoteFrameCounter = 0;
    private FramePixelFormat mRemoteVideoFormat = FramePixelFormat.Invalid;

    private float mFpsTimer = 0;

    private string mPrefix = "CallAppUI_";
    private bool isRejoin = false;

    protected virtual void Awake()
    {
        mApp = GetComponent<CallApp>();

        mPrefix += this.gameObject.name + "_";
    }

    protected virtual void Start()
    {
        SetupCallApp();
    }

    private void SetupCallApp()
    {
        //mApp.SetVideoDevice(mApp.GetVideoDevices()[1]);
        mApp.SetAudio(true);
        mApp.SetVideo(true);

        mApp.SetIdealResolution(960, 720);
        mApp.SetIdealFps(30);
        mApp.SetAutoRejoin(false);
        mApp.SetShowLocalVideo(true);
        mApp.SetupCall();

        //mApp.Join("SurrogateRobotFIBO");
    }


    /// <summary>
    /// Updates the local video. If the frame is null it will hide the video image
    /// </summary>
    /// <param name="frame"></param>
    /*public virtual void UpdateLocalTexture(IFrame frame, FramePixelFormat format)
    {
        if (uLocalVideoImage != null)
        {
            if (frame != null)
            {
                UnityMediaHelper.UpdateTexture(frame, ref mLocalVideoTexture);
                uLocalVideoImage.texture = mLocalVideoTexture;
                if (uLocalVideoImage.gameObject.activeSelf == false)
                {
                    uLocalVideoImage.gameObject.SetActive(true);
                }

                //apply rotation
                //watch out uLocalVideoImage should be scaled -1 X to make the local camera appear mirrored
                //it should also be scaled -1 Y because Unity reads the image from bottom to top
                uLocalVideoImage.transform.rotation = Quaternion.Euler(0, 0, frame.Rotation);

                mHasLocalVideo = true;
                mLocalFrameCounter++;
                mLocalVideoWidth = frame.Width;
                mLocalVideoHeight = frame.Height;
                mLocalVideoFormat = format;
            }
            else
            {
                //app shutdown. reset values
                mHasLocalVideo = false;
                uLocalVideoImage.texture = null;
                uLocalVideoImage.transform.rotation = Quaternion.Euler(0, 0, 0);
                uLocalVideoImage.gameObject.SetActive(false);
            }
        }
    }*/

    /// <summary>
    /// Updates the remote video. If the frame is null it will hide the video image.
    /// </summary>
    /// <param name="frame"></param>
    public virtual void UpdateRemoteTexture(IFrame frame, FramePixelFormat format)
    {
        if (leftQuad != null && rightQuad != null)
        {
            if (frame != null)
            {
                UnityMediaHelper.UpdateTexture(frame, ref mRemoteVideoTexture);
                //watch out: due to conversion from WebRTC to Unity format the image is flipped (top to bottom)
                //this also inverts the rotation

                
                leftQuad.GetComponent<Renderer>().material.mainTexture = mRemoteVideoTexture;
                rightQuad.GetComponent<Renderer>().material.mainTexture = mRemoteVideoTexture;


                leftQuad.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, 1);
                rightQuad.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, 1);
                rightQuad.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.5f, 0);


                //uRemoteVideoImage.transform.rotation = Quaternion.Euler(0, 0, frame.Rotation * -1);

                mHasRemoteVideo = true;
                mRemoteVideoWidth = frame.Width;
                mRemoteVideoHeight = frame.Height;
                mRemoteVideoFormat = format;
                mRemoteFrameCounter++;
            }
            else
            {
                mHasRemoteVideo = false;
                leftQuad.GetComponent<Renderer>().material.mainTexture = uNoCameraTexture;
                rightQuad.GetComponent<Renderer>().material.mainTexture = uNoCameraTexture;
            }
        }
    }

    /// <summary>
    /// Adds a new message to the message view
    /// </summary>
    /// <param name="text"></param>
    public void Append(string text)
    {
        Debug.Log("Chat output: " + text);
    }


    /// <summary>
    /// Shows the setup screen or the chat + video
    /// </summary>
    /// <param name="showSetup">true Shows the setup. False hides it.</param>
    public void SetGuiState(bool showSetup)
    {
        //this is going to hide the textures until it is updated with a new frame update
        //UpdateLocalTexture(null, FramePixelFormat.Invalid);
        UpdateRemoteTexture(null, FramePixelFormat.Invalid);
    }


    /// <summary>
    /// Sends a message to the other end
    /// </summary>
    /// <param name="msg"></param>
    private void SendMsg(string msg)
    {
        if (String.IsNullOrEmpty(msg))
        {
            //never send null or empty messages. webrtc can't deal with that
            return;
        }

        Append(msg);
        mApp.Send(msg);

    }


    /// <summary>
    /// Shutdown button pressed. Shuts the network down.
    /// </summary>
    public void ShutdownButtonPressed()
    {
        mApp.ResetCall();
    }


    protected virtual void Update()
    {
        if (SceneManager.joinRoomState == "Join")
        {
            if (isRejoin)
            {
                mApp.SetupCall();
            }

            isRejoin = false;
            mApp.Join("SurrogateRobotFIBO");
            SceneManager.joinRoomState = "wait";
        }
        else if (SceneManager.joinRoomState == "Exit")
        {
            mApp.ResetCall();
            isRejoin = true;
            SceneManager.joinRoomState = "wait";
        }


        if(mVideoOverlayTimeout > 0)
        {
            string local = "Local:";
            if (mHasLocalVideo == false)
            {
                local += "no video";
            }
            else
            {
                local += mLocalVideoWidth + "x" + mLocalVideoHeight + Enum.GetName(typeof(FramePixelFormat), mLocalVideoFormat) + " FPS:" + mLocalFps;
            }
            string remote = "Remote:";
            if (mHasRemoteVideo == false)
            {
                remote += "no video";
            }
            else
            {
                remote += mRemoteVideoWidth + "x" + mRemoteVideoHeight + Enum.GetName(typeof(FramePixelFormat), mRemoteVideoFormat) + " FPS:" + mRemoteFps;
            }

            mVideoOverlayTimeout -= Time.deltaTime;
            if(mVideoOverlayTimeout <= 0)
            {
                mVideoOverlayTimeout = 0;
            }
        }

        float fpsTimeDif = Time.realtimeSinceStartup - mFpsTimer;
        if(fpsTimeDif > 1)
        {
            mLocalFps = Mathf.RoundToInt( mLocalFrameCounter / fpsTimeDif);
            mRemoteFps = Mathf.RoundToInt(mRemoteFrameCounter / fpsTimeDif);
            //Debug.Log("RemoteFPS" + mRemoteFps);
            mFpsTimer = Time.realtimeSinceStartup;
            mLocalFrameCounter = 0;
            mRemoteFrameCounter = 0;
        }
    }
}
