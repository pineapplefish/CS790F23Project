using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportationController : MonoBehaviour
{
    public LayerMask teleportLayer = Physics.DefaultRaycastLayers;

    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    public enum SelectionMode { WIM, ARC }
    public SelectionMode activeMode = SelectionMode.ARC;

    private Gradient validLine;
    private Gradient invalidLine;

    private WIMSelectionController wimController;
    private ParabolicSelectionController arcController;

    // Start is called before the first frame update
    void Start()
    {
        //LineRenderer colors
        validLine = new Gradient();
        validLine.SetKeys(new GradientColorKey[] { new GradientColorKey(validColor, 0), new GradientColorKey(validColor, 1) },
            new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
        invalidLine = new Gradient();
        invalidLine.SetKeys(new GradientColorKey[] { new GradientColorKey(invalidColor, 0), new GradientColorKey(invalidColor, 1) },
            new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });

        //Controller references
        wimController = GetComponent<WIMSelectionController>();
        arcController = GetComponent<ParabolicSelectionController>();
        SetMode(activeMode);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Teleport(float x, float z)
    {
        if (Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out RaycastHit hit, 120, teleportLayer))
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

    public Gradient ValidLineGradient() { return validLine; }
    public Gradient InvalidLineGradient() { return invalidLine; }

    public void SetMode(SelectionMode mode)
    {
        if (mode == SelectionMode.WIM)
        {
            wimController.enabled = true;
            arcController.enabled = false;
        }
        else if (mode == SelectionMode.ARC)
        {
            wimController.enabled = false;
            arcController.enabled = true;
        }
        activeMode = mode;
    }
}
