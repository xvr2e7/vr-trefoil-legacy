using UnityEngine;

public class ArrowRotationForDashed : MonoBehaviour
{
    public GameObject trefoilObject; // Reference to the Trefoil object
    private float rotationSpeed = 30.0f; // Degrees per second
    private int direction = 1;
    private float angle = 0f;
    private DashedTrefoilRotation trefoilScript;

    void Start()
    {
        // Find the Trefoil object if not assigned in the inspector
        if (trefoilObject == null)
        {
            trefoilObject = GameObject.Find("TrefoilDashed");
        }
    }

    void Update()
    {
        if (trefoilObject != null)
        {
            // Access the Trefoil's rotation speed and direction
            trefoilScript = trefoilObject.GetComponent<DashedTrefoilRotation>();
            if (trefoilScript != null)
            {
                rotationSpeed = trefoilScript.rotationSpeed;
                direction = (trefoilScript.rotationDirection == TrialManager.RotationDirection.CW) ? -1 : 1;

                if (trefoilScript.selfRotate)
                {
                    // Self-rotation with relative position maintained
                    FollowTrefoilSelfRotate();
                }
                else
                {
                    // Move along with the trefoilObject in its orbit
                    FollowTrefoilOrbit();
                }
            }
            else
            {
                Debug.LogError("DashedTrefoilRotation script not found on the Trefoil object. Please assign it in the inspector.");
            }
        }
        else
        {
            Debug.LogError("Trefoil object not found. Please assign it in the inspector.");
        }
    }

    private void FollowTrefoilSelfRotate()
    {
        transform.localPosition = trefoilObject.transform.localPosition;

        // Handle self-rotation around the object's center
        angle += rotationSpeed * Time.deltaTime * direction;

        Vector3 currentRotation = transform.localRotation.eulerAngles;
        currentRotation.z = angle;
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    private void FollowTrefoilOrbit()
    {
        angle += rotationSpeed * Time.deltaTime * direction;
        transform.localPosition = trefoilObject.transform.localPosition;
    }

    public void ResetRotation(bool inverseYAxis = false)
    {
        // Reset rotation
        angle = 0f;
        transform.localRotation = Quaternion.identity;

        Vector3 currentScale = transform.localScale;
        currentScale.y *= currentScale.y > 0 ^ !inverseYAxis ? 1 : -1;
        transform.localScale = currentScale;
    }
}
