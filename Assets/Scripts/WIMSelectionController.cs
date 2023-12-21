using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WIMSelectionController : MonoBehaviour
{
    public OVRInput.RawAxis1D teleportBind = OVRInput.RawAxis1D.RIndexTrigger;
    public OVRInput.RawAxis1D grabBind = OVRInput.RawAxis1D.LIndexTrigger;

    public float grabDistance = 1.2f;
    public float rotSpeed = 360.0f;

    private Transform theOrb;
    private LayerMask orbLayer = 1 << 6;
    private TeleportationController teleportationController;
    private OrbController orbController;

    private GameObject head;
    private GameObject rightControllerAnchor;
    private GameObject leftControllerAnchor;

    private bool indicating = false;
    private Vector3 spherePoint = Vector3.zero;
    private bool validPoint = false;

    private bool grabbing = false;
    private Vector3 grabPoint = Vector3.zero;
    private Quaternion grabRotation = Quaternion.identity;
    private Vector3 prevRot = Vector3.zero;
    private float forceMult = 0.025f;

    // Start is called before the first frame update
    void Start()
    {
        theOrb = GameObject.Find("The Orb(tm)").transform;
        orbLayer = LayerMask.GetMask("The Orb(tm)");
        teleportationController = GetComponent<TeleportationController>();
        orbController = theOrb.GetComponent<OrbController>();

        head = this.transform.Find("TrackingSpace/CenterEyeAnchor").gameObject;
        rightControllerAnchor = this.transform.Find("TrackingSpace/RightHandAnchor/RightControllerAnchor").gameObject;
        leftControllerAnchor = this.transform.Find("TrackingSpace/LeftHandAnchor/LeftControllerAnchor").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        //Check for activation
        if (!indicating)
        {
            if (OVRInput.Get(teleportBind) > 0.8f)
            {
                indicating = true;
                rightControllerAnchor.GetComponent<LineRenderer>().enabled = true;
            }
        }
        if (!grabbing)
        {
            if (Vector3.Distance(theOrb.position, leftControllerAnchor.transform.position) < orbController.activeScale * 0.5 * grabDistance && OVRInput.Get(grabBind) > 0.8f)
            {
                grabbing = true;
                grabPoint = Vector3.Scale(head.transform.InverseTransformPoint(leftControllerAnchor.transform.position), new Vector3(1, 1, 0));
                grabRotation = theOrb.rotation;
                orbController.PauseRotation();
                theOrb.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        //Indication active
        if (indicating)
        {
            rightControllerAnchor.GetComponent<LineRenderer>().positionCount = 2;
            rightControllerAnchor.GetComponent<LineRenderer>().SetPosition(0, rightControllerAnchor.transform.position);
            RaycastHit hit;
            if (Physics.Raycast(rightControllerAnchor.transform.position, rightControllerAnchor.transform.forward, out hit, 5, orbLayer))
            {
                rightControllerAnchor.GetComponent<LineRenderer>().SetPosition(1, hit.point);
                spherePoint = hit.point;
                if (theOrb.InverseTransformPoint(spherePoint).y >= 0)   //Ensure point is on the right half of the sphere
                {
                    validPoint = true;
                    rightControllerAnchor.GetComponent<LineRenderer>().colorGradient = teleportationController.ValidLineGradient();
                } 
                else
                {
                    validPoint = false;
                    rightControllerAnchor.GetComponent<LineRenderer>().colorGradient = teleportationController.InvalidLineGradient();
                }
            }
            else
            {
                validPoint = false;
                rightControllerAnchor.GetComponent<LineRenderer>().SetPosition(1, rightControllerAnchor.transform.position + rightControllerAnchor.transform.forward * 5);
                rightControllerAnchor.GetComponent<LineRenderer>().colorGradient = teleportationController.InvalidLineGradient();
            }

            //Trigger released
            if (OVRInput.Get(teleportBind) < 0.2f)
            {
                //Disable indication
                indicating = false;
                rightControllerAnchor.GetComponent<LineRenderer>().enabled = false;

                if (validPoint)
                {
                    //Convert sphere point to teleport coordinates and teleport
                    float dist = orbController.mapRadius * (Vector3.Angle(theOrb.up, spherePoint - theOrb.position) / 90);
                    float angle = Vector3.SignedAngle(theOrb.forward, Vector3.ProjectOnPlane(spherePoint - theOrb.position, theOrb.up), theOrb.up) * Mathf.Deg2Rad;
                    print(dist + " " + angle);
                    teleportationController.Teleport(this.transform.position.x + (dist * Mathf.Sin(angle)),
                        this.transform.position.z + (dist * Mathf.Cos(angle)));
                    orbController.ToggleActive();
                }
            }
        }

        //Grab active
        if (grabbing)
        {
            //Trigger released
            if (OVRInput.Get(grabBind) < 0.2f)
            {
                //Disable grab
                grabbing = false;
                orbController.ResumeRotation(2);

                //Rotation force
                theOrb.GetComponent<Rigidbody>().isKinematic = false;
                theOrb.GetComponent<Rigidbody>().AddTorque(((theOrb.eulerAngles - prevRot) / Time.deltaTime) * forceMult, ForceMode.VelocityChange);
            }
            else
            {
                //Rotate sphere
                prevRot = theOrb.eulerAngles;
                theOrb.rotation = grabRotation;
                Vector3 headCoords = head.transform.InverseTransformPoint(leftControllerAnchor.transform.position);
                theOrb.Rotate(Vector3.Cross(head.transform.forward, head.transform.TransformDirection(new Vector3(grabPoint.x - headCoords.x, grabPoint.y - headCoords.y))),
                    new Vector3(grabPoint.x - headCoords.x, grabPoint.y - headCoords.y).magnitude * rotSpeed, Space.World);
            }
        }
    }

}
