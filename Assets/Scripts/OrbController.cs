using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float activeScale = 0.5f;
    public float inactiveScale = 0.2f;

    public OVRInput.RawButton orbActivationButton = OVRInput.RawButton.None;

    private Transform activeAnchor;
    private Transform inactiveAnchor;
    private Transform head;

    private bool active = false;

    // Start is called before the first frame update
    void Start()
    {
        activeAnchor = GameObject.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor/OrbActiveAnchor").transform;
        inactiveAnchor = GameObject.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor/OrbInactiveAnchor").transform;
        head = GameObject.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor").transform;

        this.transform.localScale = Vector3.one * inactiveScale;
    }

    // Update is called once per frame
    void Update()
    {
        //Check for mode switch
        if (OVRInput.GetDown(orbActivationButton))
        {
            if (active)
            {
                active = false;
                this.transform.localScale = Vector3.one * inactiveScale;
            } 
            else
            {
                active = true;
                this.transform.localScale = Vector3.one * activeScale;
            }
        }

        //Update orb location
        if (active)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, activeAnchor.position, 
                Time.deltaTime * Vector3.Distance(this.transform.position, activeAnchor.position) * moveSpeed);
        }
        else
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, inactiveAnchor.position,
                Time.deltaTime * Vector3.Distance(this.transform.position, inactiveAnchor.position) * moveSpeed);
            this.transform.forward = head.position - this.transform.position;
        }

        //TODO: Update world representation
    }
}
