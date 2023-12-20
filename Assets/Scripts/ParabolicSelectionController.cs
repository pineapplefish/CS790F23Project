using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParabolicSelectionController : MonoBehaviour
{
    public OVRInput.Axis1D teleportBind = OVRInput.Axis1D.PrimaryIndexTrigger;

    public LayerMask teleportLayer = Physics.DefaultRaycastLayers;

    private TeleportationController teleportationController;

    private GameObject rightControllerAnchor;
    private GameObject leftControllerAnchor;
    private LineRenderer rightLineRenderer;
    private LineRenderer leftLineRenderer;

    private bool indicating = false;
    private enum Hand { RIGHT, LEFT };
    private Hand activeHand = Hand.RIGHT;
    private Vector3 indicatedPoint = Vector3.zero;
    private bool validPoint = false;

    private float arcResolution = 0.05f; //Distance between points
    private float arcVelocity = 10.0f;
    private List<Vector3> arcPoints = new List<Vector3>();
    private int maxArcPoints = 50;

    // Start is called before the first frame update
    void Start()
    {
        teleportationController = GetComponent<TeleportationController>();

        rightControllerAnchor = this.transform.Find("TrackingSpace/RightHandAnchor/RightControllerAnchor").gameObject;
        leftControllerAnchor = this.transform.Find("TrackingSpace/LeftHandAnchor/LeftControllerAnchor").gameObject;
        rightLineRenderer = rightControllerAnchor.GetComponent<LineRenderer>();
        leftLineRenderer = leftControllerAnchor.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //Check for activation
        if (!indicating)
        {
            //Rstick activated
            if (OVRInput.Get(teleportBind, OVRInput.Controller.RTouch) > 0.8f)
            {
                indicating = true;

                //Start rstick indication
                activeHand = Hand.RIGHT;
                rightLineRenderer.enabled = true;
            }
            //Lstick activated
            else if (OVRInput.Get(teleportBind, OVRInput.Controller.LTouch) > 0.8f)
            {
                indicating = true;

                //Start lstick indication
                activeHand = Hand.LEFT;
                leftLineRenderer.enabled = true;
            }
        }

        //Indication active
        if (indicating)
        {
            //Try to find valid indicated surface + point with parabola
            //Code adapted from https://github.com/llamacademy/projectile-trajectory/
            Vector3 startPosition;
            Vector3 startVelocity;
            arcPoints.Clear();
            validPoint = false;
            if (activeHand == Hand.RIGHT)
            {
                startPosition = rightControllerAnchor.transform.position;
                startVelocity = arcVelocity * rightControllerAnchor.transform.forward;
            }
            else
            {
                startPosition = leftControllerAnchor.transform.position;
                startVelocity = arcVelocity * leftControllerAnchor.transform.forward;
            }

            Vector3 lastPosition = startPosition;
            arcPoints.Add(lastPosition);
            for (float time = 0.0f; arcPoints.Count < maxArcPoints && lastPosition.y > -10.0f; time += arcResolution)
            {
                Vector3 point = startPosition + time * startVelocity;
                point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

                arcPoints.Add(point);

                if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude, teleportLayer))
                {
                    indicatedPoint = hit.point;
                    validPoint = true;
                    break;
                }
                lastPosition = point;
            }

            //Draw arc
            if (activeHand == Hand.RIGHT)
            {
                rightLineRenderer.positionCount = arcPoints.Count;
                for (int i = 0; i < arcPoints.Count; i++)
                {
                    rightLineRenderer.SetPosition(i, arcPoints[i]);
                }
                if (validPoint)
                {
                    rightLineRenderer.colorGradient = teleportationController.ValidLineGradient();
                }
                else
                {
                    rightLineRenderer.colorGradient = teleportationController.InvalidLineGradient();
                }
            }
            else if (activeHand == Hand.LEFT)
            {
                leftLineRenderer.positionCount = arcPoints.Count;
                for (int i = 0; i < arcPoints.Count; i++)
                {
                    leftLineRenderer.SetPosition(i, arcPoints[i]);
                }
                if (validPoint)
                {
                    leftLineRenderer.colorGradient = teleportationController.ValidLineGradient();
                }
                else
                {
                    leftLineRenderer.colorGradient = teleportationController.InvalidLineGradient();
                }
            }

            //Trigger released
            if ((activeHand == Hand.RIGHT && OVRInput.Get(teleportBind, OVRInput.Controller.RTouch) < 0.2f) ||
                (activeHand == Hand.LEFT && OVRInput.Get(teleportBind, OVRInput.Controller.LTouch) < 0.2f))
            {
                //Disable indication
                indicating = false;
                if (activeHand == Hand.RIGHT)
                {
                    rightLineRenderer.enabled = false;
                }
                else if (activeHand == Hand.LEFT)
                {
                    leftLineRenderer.enabled = false;
                }

                //Teleport if valid target
                if (validPoint)
                {
                    teleportationController.Teleport(indicatedPoint);
                }
            }
        }
    }

    void OnEnable()
    {

    }
}
