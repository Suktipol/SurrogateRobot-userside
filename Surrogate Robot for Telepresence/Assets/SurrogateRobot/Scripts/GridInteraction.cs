using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GridInteraction : MonoBehaviour
{
    public SteamVR_Action_Boolean chooseGrid = SteamVR_Input.__actions_default_in_ChooseGrid;
    public SteamVR_Action_Boolean interactUI = SteamVR_Input.__actions_default_in_InteractUI;
    public SteamVR_Input_Sources rightContoller;

    public GameObject GridUI;
    public GameObject StartGrid;

    Transform previousContact = null;
    Transform choseButton = null;
    GameObject previousGrid;
    Color previousColor;

	void Start () {
        previousGrid = StartGrid;
        previousGrid.GetComponent<Renderer>().material.color = Color.red;
	}
	
	void Update () {
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit);

        // Show GridUI when press on the trackpad.
        if (chooseGrid.GetState(rightContoller))
        {
            GridUI.SetActive(true);
            if (previousContact && previousContact != hit.transform)
            {
                //Change color back when laser is out.
                Debug.Log("Laser Out");
                previousContact.gameObject.GetComponent<Renderer>().material.color = previousColor;
                previousContact = null;
                choseButton = null;
            }
            if (bHit && previousContact != hit.transform)
            {
                //Change color to blue when laser is In.
                Debug.Log("Laser In");
                previousContact = hit.transform;
                choseButton = hit.transform;
                previousColor = hit.transform.gameObject.GetComponent<Renderer>().material.GetColor("_Color");
                hit.transform.gameObject.GetComponent<Renderer>().material.color = Color.blue;

            }
            if (!bHit)
            {
                previousContact = null;
                choseButton = null;
            }
        }
        else if (chooseGrid.GetStateUp(rightContoller))
        {
            if (choseButton != null)
            {
                //When release touchpad, the selected button will turn red and the previous one turn white.
                previousGrid.gameObject.GetComponent<Renderer>().material.color = Color.white;
                choseButton.gameObject.GetComponent<Renderer>().material.color = Color.red;
                previousGrid = choseButton.gameObject;

                previousContact = null;
            }
        }

        else
        {
            GridUI.SetActive(false);
        }
        
	}
}
