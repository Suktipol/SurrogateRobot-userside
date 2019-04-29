using System;
using System.Collections; 
using System.Collections.Generic; 
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
using System.Threading; 
using UnityEngine;
using Valve.VR;

public class RobotHeadRotation : MonoBehaviour
{  	
    public GameObject Headset;

    UDPserver udpServer;

    /*[Header("SteamVR Controller")]
    public SteamVR_Input_Sources leftContoller;
    public SteamVR_Input_Sources rightContoller;*/

    private int rot_x, rot_y, rot_z;
    private int old_rot_x, old_rot_y, old_rot_z;

    private IPEndPoint remoteEndPoint;
    private UdpClient udpClient;

    private bool isMax = true;
    private bool isMin = true;
    private bool stateIn = false;
    private bool stateIn2 = false;

    //private bool clickTrackpadState = false;
    //private int moveState = 0;

    //--------------------------------------------------------

    void Start()
    {
        udpServer = new UDPserver();
        udpServer.initialUDP(SceneManager.ROBOT_IP_ADDRESS, SceneManager.UDP_PORT);
        //udpServer.StartReceiveThread();
        //InvokeRepeating("SendingRobotMovement", 1, 0.1f);
    }

    void Update()
    {
        if (SceneManager.controlRobotHead == "start")
        {
            InvokeRepeating("SendingHeadRotation", 1, 0.1f);
            SceneManager.controlRobotHead = "nothing";
            //udpServer.StartReceiveThread();
            Debug.Log("Start Invoking");
        }
        else if (SceneManager.controlRobotHead == "stop")
        {
            if (IsInvoking("SendingHeadRotation"))
                CancelInvoke("SendingHeadRotation");   
        }
    }

    /// <summary>
    /// Read orientation of vive headset
    /// </summary>
    private void SendingHeadRotation()
    {
        rot_x = (int)(WrapAngle(Headset.transform.eulerAngles.x)); //tilt 
        rot_y = (int)(-WrapAngle(Headset.transform.eulerAngles.y)); //pan 
        rot_z = (int)(-WrapAngle(Headset.transform.eulerAngles.z)); //swing
        Evaluation.sendUdpOnce = true;

        // If rotation of Y more than maximum limit of the motor, the robot's head will not rotate
        // User should rotate back to activate motor again 
        if ((rot_y > 170 && isMin) || stateIn)  
        {
            isMax = false;
            stateIn = true;
            Debug.Log("> Maximum border");
            if (rot_y < 150 || rot_y > 170){}
            else
            {
                isMax = true;
                stateIn = false;
                Debug.Log("in border 1");
            }
        }

        // If rotation of Y less than minimum limit of the motor, the robot's head will not rotate
        // User should rotate back to activate motor again 
        if (rot_y < -170 && isMax || stateIn2)
        {
            isMin = false;
            stateIn2 = true;
            Debug.Log("< Minimum border");
            if (rot_y < -170 || rot_y > -150) { }
            else
            {
                isMin = true;
                stateIn2 = false;
                Debug.Log("in border 2");
            }
        }

        if ((rot_x >= -80 && rot_x <= 50) && (rot_y >= -170 && rot_y <= 170) && (rot_z >= -60 && rot_z <= 60) && isMin && isMax)
        {
            //Offset for reduce sensitivity of headset's orientation. 
            if (Mathf.Abs(rot_x - old_rot_x) > 0 ||
                Mathf.Abs(rot_y - old_rot_y) > 0 ||
                Mathf.Abs(rot_y - old_rot_y) > 0)
            {
                udpServer.UDPSendMessage(rot_x + "," + rot_y + "," + rot_z);

                old_rot_x = rot_x;
                old_rot_y = rot_y;
                old_rot_z = rot_z;
            }
        }

    }

    /// <summary>
    /// Mapping the orientation of robot head to follow the orientation of human head.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    private float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }

    /// <summary>
    /// Click any Trackpad to control the robot remotely.
    /// Left Controller --> Click west to turn left
    ///                 --> Click east to turn right
    /// Right Controller --> Click north to move forward
    ///                  --> Click south to move backward
    /// </summary>
    /*void SendingRobotMovement()
    {
        if (MoveForward.GetState(rightContoller) && (clickTrackpadState == false))
        {
            UDPSendMessage("1*");
            Debug.Log("Move Forward");
            clickTrackpadState = true;
            moveState = 1;
        }

        if (TurnRight.GetState(leftContoller) && (clickTrackpadState == false))
        {
            UDPSendMessage("2*");
            Debug.Log("Turn Right");
            clickTrackpadState = true;
            moveState = 2;
        }

        if (MoveBackward.GetState(rightContoller) && (clickTrackpadState == false))
        {
            UDPSendMessage("3*");
            Debug.Log("Move Backward");
            clickTrackpadState = true;
            moveState = 3;
        }

        if (TurnLeft.GetState(leftContoller) && (clickTrackpadState == false))
        {
            UDPSendMessage("4*");
            Debug.Log("Turn Left");
            clickTrackpadState = true;
            moveState = 4;
        }

        if (!MoveForward.GetState(rightContoller) && !TurnRight.GetState(leftContoller) &&
            !MoveBackward.GetState(rightContoller) && !TurnLeft.GetState(leftContoller) && (clickTrackpadState == true))
        {
            UDPSendMessage("0*");
            Debug.Log("Stop");
            clickTrackpadState = false;
            moveState = 0;
        }
    }*/
}