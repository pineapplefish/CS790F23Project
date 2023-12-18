using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OrbController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float activeScale = 0.5f;
    public float inactiveScale = 0.2f;

    public OVRInput.RawButton orbActivationButton = OVRInput.RawButton.None;

    public float mapRadius = 15.0f;
    public float miniScale = 1.0f;
    public LayerMask worldLayer = 0;

    private Transform activeAnchor;
    private Transform inactiveAnchor;
    private Transform head;

    private int uiLayer;
    private List<GameObject> displayedObjects = new List<GameObject>();
    private List<GameObject> miniObjects = new List<GameObject>();

    private bool active = false;

    // Start is called before the first frame update
    void Start()
    {
        activeAnchor = GameObject.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor/OrbActiveAnchor").transform;
        inactiveAnchor = GameObject.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor/OrbInactiveAnchor").transform;
        head = GameObject.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor").transform;

        uiLayer = LayerMask.NameToLayer("UI");

        this.transform.localScale = Vector3.one * inactiveScale;
    }

    // Update is called once per frame
    void Update()
    {
        //Check for mode switch
        if (OVRInput.GetDown(orbActivationButton))
        {
            toggleActive();
        }

        //Update orb location
        if (active)
        {
            //TODO: Change to Vector3.SmoothDamp?
            this.transform.position = Vector3.MoveTowards(this.transform.position, activeAnchor.position, 
                Time.deltaTime * Vector3.Distance(this.transform.position, activeAnchor.position) * moveSpeed);
            //this.transform.up = head.position - this.transform.position;    //TODO: Enable moving of this
            //this.transform.up = head.position - this.transform.position;
            this.transform.rotation = Quaternion.LookRotation(head.position - this.transform.position, Vector3.down);
            this.transform.Rotate(Vector3.right, 90, Space.Self);
            //this.transform.forward = Vector3.Scale(this.transform.forward, new Vector3(0, 1, 1));
        }
        else
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, inactiveAnchor.position,
                Time.deltaTime * Vector3.Distance(this.transform.position, inactiveAnchor.position) * moveSpeed);
            //this.transform.up = head.position - this.transform.position;
            this.transform.rotation = Quaternion.LookRotation(head.position - this.transform.position, Vector3.down);
            this.transform.Rotate(Vector3.right, 90, Space.Self);
            //this.transform.forward = Vector3.Scale(this.transform.forward, new Vector3(0, 1, 1));
        }

        //Update world representation
        foreach (GameObject obj in miniObjects)
        {
            Destroy(obj);
        }
        displayedObjects.Clear();
        miniObjects.Clear();
        Vector3 groundCoordinates = Vector3.Scale(head.position, new Vector3(1, 0, 1));
        foreach (Collider col in Physics.OverlapCapsule(groundCoordinates + Vector3.up * 100, groundCoordinates, mapRadius, worldLayer))
        {
            displayedObjects.Add(col.gameObject);

            //Create mini representation
            GameObject newMini = Instantiate(col.gameObject, this.transform);
            newMini.layer = uiLayer;
            newMini.transform.localScale = Vector3.one * miniScale / mapRadius;
            newMini.transform.localPosition = getPositionOnSphere(col.transform.position - groundCoordinates);
            newMini.transform.up = newMini.transform.position - this.transform.position;
            //TODO: Account for original objects rotation

            miniObjects.Add(newMini);
        }
    }

    private Vector3 getPositionOnSphere(Vector3 relativePosition)
    {
        float angle1 = 90 * (relativePosition.magnitude / mapRadius) * Mathf.Deg2Rad;
        float angle2 = Vector3.SignedAngle(Vector3.forward, Vector3.Scale(relativePosition, new Vector3(1, 0, 1)), Vector3.up) * Mathf.Deg2Rad;
        //TODO: Account for vertical position
        return new Vector3(Mathf.Sin(angle1) * Mathf.Sin(angle2), Mathf.Cos(angle1), Mathf.Sin(angle1) * Mathf.Cos(angle2)) * 0.5f;
    }

    public bool isActive() { return active; }

    public void toggleActive()
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
}
