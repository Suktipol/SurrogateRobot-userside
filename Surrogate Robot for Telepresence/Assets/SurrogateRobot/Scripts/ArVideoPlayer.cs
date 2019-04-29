using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.Video;

public class ArVideoPlayer : MonoBehaviour
{
    private Material initialMat;
    public Material highlightMat;

    public SteamVR_Action_Boolean arInteractive = SteamVR_Input.__actions_default_in_ArInteraction;
    
    public GameObject videoPlane;
    public Texture playTexture;
    public Texture stopTexture;

    private VideoPlayer videoPlayer;

    private Transform camera;
    private Transform arCollider;

    private bool playVideo = false;

    private void Start()
    {
        initialMat = gameObject.GetComponent<Renderer>().material;
        highlightMat.mainTexture = playTexture;

        camera = GameObject.Find("LeftCamera").transform;
        arCollider = transform.parent.GetChild(1).transform;

        videoPlayer = videoPlane.GetComponent<VideoPlayer>();
    }

    private void Update()
    {
        transform.position = arCollider.position + (Vector3.up * 0.5f);
        transform.LookAt(camera);
        transform.Rotate(90, 0, 0);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("ViveController"))
            gameObject.GetComponent<Renderer>().material = highlightMat;

        // if click Trigger button when hover the video button --> run the video at the video plane
        if (arInteractive.GetStateDown(SteamVR_Input_Sources.Any) && transform.CompareTag("VideoPlayer"))
        {
            playVideo = !playVideo;
            if (playVideo)
            {
                // Play the video when click the video button
                videoPlane.SetActive(playVideo);
                videoPlayer.clip = AR_Manager.choosedVideo;
                if (!videoPlayer.isPlaying)
                    videoPlayer.Play();

                highlightMat.mainTexture = stopTexture;
                initialMat.mainTexture = stopTexture;
            }
            else
            {
                videoPlane.SetActive(playVideo);
                if (videoPlayer.isPlaying)
                    videoPlayer.Stop();

                highlightMat.mainTexture = playTexture;
                initialMat.mainTexture = playTexture;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ViveController"))
            gameObject.GetComponent<Renderer>().material = initialMat;
    }
}
