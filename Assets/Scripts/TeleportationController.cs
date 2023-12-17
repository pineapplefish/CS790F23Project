using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportationController : MonoBehaviour
{
    public LayerMask teleportLayer = Physics.DefaultRaycastLayers;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Teleport(float x, float z)
    {
        if (Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out RaycastHit hit, 100, teleportLayer))
        {
            Teleport(hit.point);
        }
    }

    public void Teleport(float x, float y, float z) 
    {
        Teleport(new Vector3(x, y, z));
    }

    public void Teleport(Vector3 pos)
    {
        this.transform.position = pos;
    }
}
