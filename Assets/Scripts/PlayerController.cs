using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float rotateStep = 45.0f;
    public OVRInput.RawAxis2D rotateBind = OVRInput.RawAxis2D.LThumbstick;

    private bool rotateEnabled = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rotateEnabled)
        {
            //Rotate step
            if (Mathf.Abs(OVRInput.Get(rotateBind).x) > 0.8)
            {
                rotateEnabled = false;
                if (OVRInput.Get(rotateBind).x > 0)
                {
                    this.transform.Rotate(Vector3.up, rotateStep, Space.World);
                }
                else
                {
                    this.transform.Rotate(Vector3.up, -rotateStep, Space.World);
                }
            }
        }
        //Reset rotation disable
        else if (Mathf.Abs(OVRInput.Get(rotateBind).x) < 0.1)
        {
            rotateEnabled = true;
        }
    }
}
