using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OrbController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 5.0f;
    public float activeScale = 0.5f;
    public float inactiveScale = 0.2f;

    public OVRInput.RawButton orbActivationButton = OVRInput.RawButton.None;

    public float mapRadius = 15.0f;
    public float miniScale = 1.0f;
    public LayerMask worldLayer = 0;
    public string rootTag = "ObjectRoot";

    private Transform activeAnchor;
    private Transform inactiveAnchor;
    private Transform head;

    private Quaternion activeNorthRotation;
    private Quaternion inactiveNorthRotation;

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

        activeAnchor.rotation = Quaternion.LookRotation(head.position - activeAnchor.position, Vector3.down);
        activeAnchor.Rotate(Vector3.right, 90, Space.Self);
        activeNorthRotation = activeAnchor.localRotation;
        inactiveAnchor.rotation = Quaternion.LookRotation(head.position - inactiveAnchor.position, Vector3.down);
        inactiveAnchor.Rotate(Vector3.right, 90, Space.Self);
        inactiveNorthRotation = inactiveAnchor.localRotation;

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
            activeAnchor.localRotation = activeNorthRotation;
            activeAnchor.Rotate(Vector3.up, Mathf.Atan2(-head.forward.x, head.forward.z) * Mathf.Rad2Deg, Space.Self);

            //TODO: Change to Vector3.SmoothDamp?
            this.transform.position = Vector3.MoveTowards(this.transform.position, activeAnchor.position, 
                Time.deltaTime * Vector3.Distance(this.transform.position, activeAnchor.position) * moveSpeed);
            //this.transform.up = head.position - this.transform.position;    //TODO: Enable moving of this
            //this.transform.up = head.position - this.transform.position;
            //this.transform.rotation = Quaternion.LookRotation(head.position - this.transform.position, Vector3.down);
            //this.transform.Rotate(Vector3.right, 90, Space.Self);
            //this.transform.forward = Vector3.Scale(this.transform.forward, new Vector3(0, 1, 1));
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, activeAnchor.rotation, 
                Time.deltaTime * Quaternion.Angle(this.transform.rotation, activeAnchor.rotation) * rotateSpeed);
        }
        else
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, inactiveAnchor.position,
                Time.deltaTime * Vector3.Distance(this.transform.position, inactiveAnchor.position) * moveSpeed);
            //this.transform.up = head.position - this.transform.position;
            //this.transform.rotation = Quaternion.LookRotation(head.position - this.transform.position, Vector3.down);
            //this.transform.Rotate(Vector3.right, 90, Space.Self);
            //this.transform.forward = Vector3.Scale(this.transform.forward, new Vector3(0, 1, 1));
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, inactiveAnchor.rotation,
                Time.deltaTime * Quaternion.Angle(this.transform.rotation, inactiveAnchor.rotation) * rotateSpeed);
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
            //Find root object
            Transform curr = col.transform;
            while (curr != null && !curr.CompareTag(rootTag))
            {
                curr = curr.parent;
            }

            if (curr != null && !displayedObjects.Contains(curr.gameObject))
            {
                displayedObjects.Add(curr.gameObject);
            }
        }

        foreach(GameObject obj in displayedObjects)
        {
            //Create mini representation
            GameObject newMini = Instantiate(obj, this.transform);
            newMini.layer = uiLayer;
            foreach (Transform t in newMini.GetComponentInChildren<Transform>())
            {
                //TODO: Optimize?
                t.gameObject.layer = uiLayer;
            }
            foreach (Collider c in newMini.GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }
            newMini.tag = "Untagged";   //TODO: Figure out a better solution to house duplication issue
            newMini.transform.localScale = obj.transform.localScale * miniScale / mapRadius;
            //newMini.transform.localPosition = getPositionOnSphere(obj.transform.position - groundCoordinates);
            newMini.transform.localPosition = Vector3.up * (0.5f + obj.transform.position.y * (miniScale / mapRadius));
            newMini.transform.localRotation = obj.transform.rotation;
            newMini.transform.RotateAround(this.transform.position, Vector3.Cross(this.transform.up, 
                this.transform.TransformDirection(getPositionOnSphere(obj.transform.position - groundCoordinates))), 
                90 * (Vector3.Scale(obj.transform.position - groundCoordinates, new Vector3(1, 0, 1)).magnitude / mapRadius));
            //newMini.transform.up = newMini.transform.position - this.transform.position;

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
