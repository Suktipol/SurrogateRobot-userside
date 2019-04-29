using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.Extras;
using UnityEngine.UI;
using Valve.VR;

/// <summary>
/// This script is for the first scene that user controllers can interact with "Start" button
///     1. "LaserClick" function : fade_in the dark scene
///     2. Hide the environments and show the camera view 
/// </summary>
public class SceneManager : MonoBehaviour
{
    [Header("Floor2UI")]
    public GameObject floor2UI;

    [Header("MenuBar")]
    public GameObject menuBar;

    [Header("Tutorial")]
    public GameObject tutorialPlane;
    public List<Texture> tutorialTexture;

    [Header("Scene")]
    public Animator animator;
    public GameObject enableArButton;
    public GameObject environment;
    public GameObject leftQuad;
    public GameObject rightQuad;
    public GameObject interactiveController;
    public AudioSource backgroundAudio;      
   
    [Header("TCP & UDP")]
    public string userIP;
    public string robotIP;
    public int tcpPort;
    public int udpPortSender;   // For Sending Robot's head Rotation.

    //This variable is in the "ExtendUI.cs"
    public static string USER_IP_ADDRESS;
    public static int TCP_PORT;

    //This variable is in the "RobotHeadRotation.cs"
    public static string ROBOT_IP_ADDRESS;
    public static int UDP_PORT;
    public static string controlRobotHead = "nothing";

    // This variable is in the "CallAppUi.cs"
    public static string joinRoomState;

    Color previousColor;

    private bool isMenuShow = false;
    public static bool isFloor2UiShow = false;
    public static bool isArEnable = true;
    private int textureIndex = 0;

    // TcpSendMessage is used at ExtendUI.cs
    // TcpListener is used at AR_Manager.cs
    public static TCPserver tcpServer;

    bool IsClick = false;

    public Text startButtonText;
    public static string startButtonState = "IDLE";
    public static string buttonMessage;

    //==========================================================\\
    //                        MainProgram                       \\
    //==========================================================\\

    void Start()
    {
        USER_IP_ADDRESS = userIP;
        ROBOT_IP_ADDRESS = robotIP;
        TCP_PORT = tcpPort;
        UDP_PORT = udpPortSender;

        tcpServer = new TCPserver();

        tcpServer.InitialTCP(USER_IP_ADDRESS, TCP_PORT);
        tcpServer.InitialTcpListener();
    }

    private void OnEnable()
    {
        SteamVR_LaserPointer.PointerIn += LaserInObj;
        SteamVR_LaserPointer.PointerOut += LaserOutObj;
        SteamVR_LaserPointer.PointerClick += LaserClick;
    }

    private void OnDisable()
    {
        SteamVR_LaserPointer.PointerIn -= LaserInObj;
        SteamVR_LaserPointer.PointerOut -= LaserOutObj;
        SteamVR_LaserPointer.PointerClick -= LaserClick;    
    }

    private void Update()
    {
        switch (startButtonState)
        {
            case "IDLE":
                startButtonText.text = "Connect";
                break;

            case "CONNECTING":
                startButtonText.text = "Connecting...";
                break;

            case "CONNECTED":
                startButtonText.text = "Start";
                break;
        }

    }

    /// <summary>
    /// Click the button to make something if button is...
    /// 1. "Start" button --> Join room @ CallAppUI.cs
    ///                   --> Pause background song
    ///                   --> Change scene and show the camera view in 5 senconds after press button
    ///                   

    /// 2. "Exit" button --> Get back to the first scene in 5 seconds after press button
    ///                  --> Terminate the call @ CallAppUI.cs
    ///                  
    /// 3. "Tutorial" button --> Show tutorials again
    /// </summary>
    void LaserClick(object sender, PointerEventArgs e)
    {
        // Start button is clicked.
        if (e.target.CompareTag("Button") && (IsClick == false))
        {
            if (startButtonState == "IDLE")
            {
                startButtonState = "CONNECTING";
                joinRoomState = "Join";
            }
            else if (startButtonState == "CONNECTED")
            {
                backgroundAudio.Pause();
                animator.SetTrigger("ChangeScene");
                Invoke("ShowCameraView", 5.0f);

                IsClick = true;
            }       
        }
        else
        {
            if (e.target.gameObject.name == "Exit Button") // Exit button is clicked.
            {
                // Fade back to the first scene & Terminate the call.
                floor2UI.SetActive(false);
                menuBar.SetActive(false);

                Debug.Log("Click Exit button");
                Invoke("BackToFirstScene", 5.0f);
                animator.SetTrigger("ChangeScene");

                IsClick = false;
            }
            else if (e.target.gameObject.name == "Tutorial Button")   // Tutorial button is clicked.
            {
                ShowTutorial();
                menuBar.SetActive(false);
                floor2UI.SetActive(false);
                isMenuShow = false;
                isFloor2UiShow = false;

                EventManager.OnTriggerClick += ShowTutorial;
                EventManager.OnMenuClick -= ShowMenuBar;
                EventManager.OnTrackpadClick -= ShowFloor2Ui;
            }
            else if (e.target.gameObject.name == "EnableAr Button")   // if EnableArButton is clicked --> Toggle Texture
            {
                isArEnable = !isArEnable;
                if (isArEnable)
                    enableArButton.transform.GetChild(0).GetComponent<TextMesh>().text = "Click to disable AR mode";
                else
                    enableArButton.transform.GetChild(0).GetComponent<TextMesh>().text = "Click to enable AR mode";
            }
        }
    }

    /// <summary>
    /// Change button's color to yellow when button is hovered.
    /// </summary>
    void LaserInObj(object sender, PointerEventArgs e)
    {
        if (e.target.CompareTag("Button"))
        {
            previousColor = e.target.GetComponent<Renderer>().material.color;
            e.target.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }

    /// <summary>
    /// Change back to the previous color when laser is getting out.
    /// </summary>
    void LaserOutObj(object sender, PointerEventArgs e)
    {
        if (e.target.CompareTag("Button"))
        {
            e.target.GetComponent<Renderer>().material.color = previousColor;
        }
    }

    /// <summary>
    /// Show stereo cameraView.
    /// </summary>
    void ShowCameraView()
    {
        environment.SetActive(false);
        leftQuad.SetActive(true);
        rightQuad.SetActive(true);
        animator.SetTrigger("ChangeScene");

        Invoke("ShowTutorial", 3f);
        EventManager.OnTriggerClick += ShowTutorial;

        controlRobotHead = "start";

        isArEnable = true;
        enableArButton.transform.GetChild(0).GetComponent<TextMesh>().text = "Click to disable AR mode";
    }

    /// <summary>
    /// Callback function when Trigger button is clicked
    /// </summary>
    void ShowTutorial()
    {
        //if index of sprite is maximum  -->  delete this function from event
        tutorialPlane.SetActive(true);
        if (textureIndex <= (tutorialTexture.Count - 1))
        {
            tutorialPlane.GetComponent<Renderer>().material.mainTexture = tutorialTexture[textureIndex];
            textureIndex++;                     
        }
        else
        {
            textureIndex = 0;

            tutorialPlane.SetActive(false);
            EventManager.OnTriggerClick -= ShowTutorial;
            EventManager.OnMenuClick += ShowMenuBar;
            EventManager.OnTrackpadClick += ShowFloor2Ui;
        }
    }

    /// <summary>
    /// Callback function when the "Trackpad" button is clicked.
    /// </summary>
    void ShowFloor2Ui()
    {
        if (menuBar.activeSelf == false)
        {
            isFloor2UiShow = !isFloor2UiShow;
            StartCoroutine(LerpingUI(floor2UI, isFloor2UiShow));
        }
    }

    /// <summary>
    /// Extend & Shrink "Floor2UI" continuously.
    /// </summary>
    IEnumerator LerpingUI (GameObject targetUI, bool lerpState)
    {
        float sizeRatio = 0;
        if (lerpState)
        {
            floor2UI.SetActive(lerpState);
            while (sizeRatio < 1)
            {
                sizeRatio += Time.deltaTime * 3.5f;
                floor2UI.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(0.48f, 0.48f, 0.48f), sizeRatio);

                yield return null;
            }  
        }
        else
        {
            sizeRatio = 1;
            while (sizeRatio > 0)
            {
                sizeRatio -= Time.deltaTime * 3.5f;
                floor2UI.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(0.48f, 0.48f, 0.48f), sizeRatio);

                yield return null;
            }
            floor2UI.SetActive(lerpState);
        }
    }

    /// <summary>
    /// Callback function when the "Menu" button is clicked.
    /// </summary>
    void ShowMenuBar()
    {
        isMenuShow = !isMenuShow;
        menuBar.SetActive(isMenuShow);
    }

    //This function is opposite to the "ShowCameraView" Script
    void BackToFirstScene()
    {
        // Destroy Call
        joinRoomState = "Exit";
        environment.SetActive(true);
        leftQuad.SetActive(false);
        rightQuad.SetActive(false);
        animator.SetTrigger("ChangeScene");

        EventManager.OnMenuClick -= ShowMenuBar;
        EventManager.OnTrackpadClick -= ShowFloor2Ui;

        controlRobotHead = "stop";

        isArEnable = false;

        //Robot get back to the first station
        SceneManager.tcpServer.TCPSendMessage("1*");

        startButtonState = "IDLE";

        backgroundAudio.Play();
    }

    private void OnDestroy()
    {
        tcpServer.DestroyTcpServer();
    }

}
