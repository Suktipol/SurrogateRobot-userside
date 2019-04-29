using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Used to catch different events and forward them to the UI
/// </summary>
public class VideoPanelEventHandler : MonoBehaviour
{
    private CallAppUi mParent;
    private float mLastClick;

    private void Start()
    {
        mParent = this.GetComponentInParent<CallAppUi>();
        if(mParent == null)
        {
            Debug.LogError("Failed to find CallAppUi. Deactivating VideoPanelEventHandler");
            this.gameObject.SetActive(false);
            return;
        }
    }
    
}
