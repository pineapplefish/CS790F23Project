using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OrbController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 5.0f;
    public float zoomSpeed = 5.0f;

    public float activeScale = 0.5f;
    public float inactiveScale = 0.2f;

    public OVRInput.RawButton orbActivationButton = OVRInput.RawButton.None;
    public OVRInput.RawAxis2D zoomBind = OVRInput.RawAxis2D.LThumbstick;

    public float mapRadius = 15.0f;
    public float maxRadius = 30.0f;
    public float miniScale = 1.0f;
    public LayerMask worldLayer = 0;
    public string rootTag = "ObjectRoot";
    public string ignoreTag = "MiniIgnore";

    private Transform activeAnchor;
    private Transform inactiveAnchor;
    private Transform head;

    private Quaternion activeNorthRotation;
    private Quaternion inactiveNorthRotation;

    private int uiLayer;
    private List<GameObject> displayedObjects = new List<GameObject>();
    private List<GameObject> miniObjects = new List<GameObject>();

    private bool active = false;
    private bool autoRotate = true;

    private TeleportationController teleportationController;

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
        teleportationController = GameObject.Find("OVRCameraRig").GetComponent<TeleportationController>();
    }

    // Update is called once per frame
    void Update()
    {
        //Check for mode switch
        if (OVRInput.GetDown(orbActivationButton))
        {
            ToggleActive();
        }

        //Update orb position
        if (active)
        {
            activeAnchor.localRotation = activeNorthRotation;
            activeAnchor.Rotate(Vector3.up, Mathf.Atan2(-head.forward.x, head.forward.z) * Mathf.Rad2Deg, Space.Self);

            this.transform.position = Vector3.MoveTowards(this.transform.position, activeAnchor.position, 
                Time.deltaTime * Vector3.Distance(this.transform.position, activeAnchor.position) * moveSpeed);

            if (autoRotate)
            {
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, activeAnchor.rotation,
                    Time.deltaTime * Quaternion.Angle(this.transform.rotation, activeAnchor.rotation) * rotateSpeed);
            }

            //Update zoom
            if (Mathf.Abs(OVRInput.Get(zoomBind).y) > 0.1f)
            {
                mapRadius = Mathf.Min(mapRadius - OVRInput.Get(zoomBind).y * zoomSpeed * Time.deltaTime, maxRadius);
            }
        }
        else
        {
            inactiveAnchor.localRotation = inactiveNorthRotation;
            inactiveAnchor.Rotate(Vector3.up, Mathf.Atan2(-head.forward.x, head.forward.z) * Mathf.Rad2Deg, Space.Self);

            this.transform.position = Vector3.MoveTowards(this.transform.position, inactiveAnchor.position,
                Time.deltaTime * Vector3.Distance(this.transform.position, inactiveAnchor.position) * moveSpeed);

            if (autoRotate)
            {
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, inactiveAnchor.rotation,
                    Time.deltaTime * Quaternion.Angle(this.transform.rotation, inactiveAnchor.rotation) * rotateSpeed);
            }
        }

        //Update main object lists
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

        //Create mini objects
        foreach(GameObject obj in displayedObjects)
        {
            //Create mini representation
            GameObject newMini = Instantiate(obj, this.transform);
            newMini.layer = uiLayer;
            /*foreach (Transform t in newMini.GetComponentInChildren<Transform>())
            {
                t.gameObject.layer = uiLayer;
                if (t.CompareTag(ignoreTag))
                {
                    t.gameObject.SetActive(false);
                }
            }*/
            foreach (ParticleSystem p in newMini.GetComponentsInChildren<ParticleSystem>())
            {
                p.Stop();
            }
            foreach (Collider c in newMini.GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }
            foreach (LODGroup group in newMini.GetComponentsInChildren<LODGroup>())
            {
                group.ForceLOD(group.lodCount - 1);
            }
            newMini.tag = "Untagged";
            newMini.transform.localScale = obj.transform.localScale * miniScale / mapRadius;
            newMini.transform.localPosition = Vector3.up * (0.5f + obj.transform.position.y * (miniScale / mapRadius));
            newMini.transform.localRotation = obj.transform.rotation;
            newMini.transform.RotateAround(this.transform.position, Vector3.Cross(this.transform.up,
                this.transform.TransformDirection(Vector3.Scale(obj.transform.position - groundCoordinates, new Vector3(1, 0, 1)))),
                90 * (Vector3.Scale(obj.transform.position - groundCoordinates, new Vector3(1, 0, 1)).magnitude / mapRadius));

            miniObjects.Add(newMini);
        }
    }

    public bool IsActive() { return active; }

    public void ToggleActive()
    {
        if (active)
        {
            active = false;
            this.transform.localScale = Vector3.one * inactiveScale;
            teleportationController.SetMode(TeleportationController.SelectionMode.ARC);
        }
        else
        {
            active = true;
            this.transform.localScale = Vector3.one * activeScale;
            teleportationController.SetMode(TeleportationController.SelectionMode.WIM);
        }
    }

    public void PauseRotation() 
    {
        CancelInvoke("ResumeRotation");
        autoRotate = false; 
    }
    public void ResumeRotation()
    {
        this.GetComponent<Rigidbody>().isKinematic = true;
        autoRotate = true;
    }
    public void ResumeRotation(float delay) { Invoke("ResumeRotation", delay); }
}
