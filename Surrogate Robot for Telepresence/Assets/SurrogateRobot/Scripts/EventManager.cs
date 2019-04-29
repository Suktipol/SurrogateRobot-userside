using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class EventManager : MonoBehaviour {
    public SteamVR_Action_Boolean showFloor2UI = SteamVR_Input.__actions_default_in_ShowFloor2UI;
    public SteamVR_Action_Boolean menuBtn = SteamVR_Input.__actions_default_in_Menu;
    public SteamVR_Action_Boolean triggerBtn = SteamVR_Input.__actions_default_in_InteractUI;

    public SteamVR_Input_Sources anyControllers = SteamVR_Input_Sources.Any;

    public delegate void ButtonEvent();
    public static event ButtonEvent OnTriggerClick;
    public static event ButtonEvent OnTrackpadClick;
    public static event ButtonEvent OnMenuClick;

	
	void Update () {
        if (showFloor2UI.GetStateDown(anyControllers))
        {
            if (OnTrackpadClick != null)
                OnTrackpadClick();
        }

        if (menuBtn.GetStateDown(anyControllers))
        {
            if (OnMenuClick != null)
                OnMenuClick();
        }

        if (triggerBtn.GetStateDown(anyControllers))
        {
            if (OnTriggerClick != null)
                OnTriggerClick();
        }

	}
}
