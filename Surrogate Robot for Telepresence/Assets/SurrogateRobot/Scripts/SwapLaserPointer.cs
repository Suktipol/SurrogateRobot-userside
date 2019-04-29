using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

public class SwapLaserPointer : MonoBehaviour {
    public SteamVR_Action_Boolean interactUI = SteamVR_Input.__actions_default_in_InteractUI;
    public SteamVR_Input_Sources leftHand, rightHand;

    private Transform controllerLeft;
    private Transform controllerRight;

	void Start () {
        controllerLeft = transform.GetChild(0);
        controllerRight = transform.GetChild(1);

        // Set up the laser at the right hand side on start
        //SwapLaser(true);
	}

	void Update ()
    {
        if (interactUI.GetStateDown(rightHand))
            SwapLaser(true);
        if (interactUI.GetStateDown(leftHand))
            SwapLaser(false);
	}

    void SwapLaser(bool isRightHand)
    {
        controllerLeft.GetChild(1).gameObject.SetActive(!isRightHand);
        controllerRight.GetChild(1).gameObject.SetActive(isRightHand);

        controllerLeft.GetComponent<SteamVR_LaserPointer>().enabled = !isRightHand;
        controllerRight.GetComponent<SteamVR_LaserPointer>().enabled = isRightHand;
    }
}
