using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class InteractiveAR : MonoBehaviour
{
    public SteamVR_Action_Boolean arInteraction = SteamVR_Input.__actions_default_in_ArInteraction;
    Vector3 startScale;

    Shader standardShader;
    private Shader highlightShader;

    public Transform leftHand, rightHand;
    int controllerIndex = 0;

    float startDist, currentDist, diffDist, scaleValue;
    bool getStartDist = true;

    FixedJoint fx;

    void Start()
    {
        standardShader = Shader.Find("Standard");
        highlightShader = Shader.Find("Custom/Silhouetted Diffuse");
    }

    void Update()
    {
        if (ControllerGrabObject.isGrabbing && arInteraction.GetState(SteamVR_Input_Sources.LeftHand) && arInteraction.GetState(SteamVR_Input_Sources.RightHand))
        {
            // Get start object's scale 
            if (getStartDist)
            {
                startDist = Vector3.Distance(leftHand.position, rightHand.position);
                startScale = transform.localScale;
                getStartDist = false;
            }

            // Update object's scale according to the distance of both controller.
            currentDist = Vector3.Distance(leftHand.position, rightHand.position);
            diffDist = currentDist - startDist;
            transform.localScale = startScale + (Vector3.one * diffDist * 10);
        }
        else
        {
            getStartDist = true;
        }

    }

    // Highlight object when Vive controller hover the object
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("ViveController"))
            gameObject.transform.GetChild(0).GetComponent<Renderer>().material.shader = highlightShader;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ViveController"))
            gameObject.transform.GetChild(0).GetComponent<Renderer>().material.shader = standardShader;
    }
}
