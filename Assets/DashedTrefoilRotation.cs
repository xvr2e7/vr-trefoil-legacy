using UnityEngine;

public class DashedTrefoilRotation : MonoBehaviour
{
    public int n = 3;
    public float R_1 = 1f, R_2 = 1.5f;
    public float width = 0.1f;
    public int segments = 1000;
    public float rotationSpeed = 60f;
    public TrialManager.RotationDirection rotationDirection;
    public float dashSpeed = 1f; // Speed at which the dashes move along the curve
    public bool selfRotate = true; // True for self-rotation, false for orbiting
    public float orbitRadius = 0.58474f; // Orbit radius given that xyz scale is 0.26 (=2.249*scale)

    public Material dashedLineMaterial;

    private LineRenderer lineRenderer;
    private float angle;
    private Vector3[] positions;
    private int direction;
    private Vector3 defaultPosition; // Store the default position here
    private bool isMoving = false;
    private float motionStartTime;

    private void Start()
    {
        Application.targetFrameRate = 144;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.material = dashedLineMaterial;

        direction = rotationDirection == TrialManager.RotationDirection.CW ? -1 : 1;

        positions = new Vector3[segments + 1];
        DrawCurve();

        // Store the current position as the default position
        defaultPosition = transform.localPosition;
    }

    private void Update()
    {
        if (selfRotate)
        {
            SelfRotate();
        }
        else
        {
            Orbit();
        }

        if (isMoving)
        {
            // Animate the texture offset to move the dashes
            float offset = (Time.time - motionStartTime) * dashSpeed;
            lineRenderer.material.mainTextureOffset = new Vector2(-offset, 0);
        }
        else
        {
            lineRenderer.material.mainTextureOffset = new Vector2(0, 0);
        }
    }

    public void StartMotion()
    {
        motionStartTime = Time.time;
        isMoving = true;
    }

    public void StopMotion()
    {
        isMoving = false;
    }

    private void SelfRotate()
    {
        // Increment the angle for rotation
        angle += rotationSpeed * Time.deltaTime * direction;

        Vector3 currentRotation = transform.localRotation.eulerAngles;
        currentRotation.z = angle;
        transform.SetLocalPositionAndRotation(defaultPosition, Quaternion.Euler(currentRotation));
    }

    private void Orbit()
    {
        // Handle orbiting around a circle with a radius of orbitRadius
        angle += rotationSpeed * Time.deltaTime * direction;

        // Calculate the position along the orbit
        float orbitX = orbitRadius * Mathf.Cos(Mathf.Deg2Rad * angle);
        float orbitY = orbitRadius * Mathf.Sin(Mathf.Deg2Rad * angle);

        // Set the object's position to follow the orbit without self-rotation
        transform.localPosition = new Vector3(orbitX + defaultPosition.x, orbitY + defaultPosition.y, transform.localPosition.z);
    }


    private void DrawCurve()
    {
        float x, y, z = 0f;
        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = R_1 * Mathf.Cos(Mathf.Deg2Rad * angle) + R_2 * Mathf.Cos((1 - n) * Mathf.Deg2Rad * angle);
            y = R_1 * Mathf.Sin(Mathf.Deg2Rad * angle) + R_2 * Mathf.Sin((1 - n) * Mathf.Deg2Rad * angle);
            positions[i] = new Vector3(x, y, z);

            angle += 360f / segments; // Fixed the angle increment to 360 degrees
        }

        lineRenderer.SetPositions(positions);
        lineRenderer.material.mainTextureScale = new Vector2(segments / 10.0f, 1);
    }

    public void ResetTo(TrialManager.Trial trial)
    {
        lineRenderer.enabled = false;
        // Reset rotation
        angle = 0f;
        transform.localRotation = Quaternion.identity;

        n = trial.n;
        R_1 = trial.R_1;
        R_2 = trial.R_2;
        width = trial.width;
        segments = trial.segments;
        rotationSpeed = trial.rotationSpeed;
        rotationDirection = trial.rotationDirection;

        direction = rotationDirection == TrialManager.RotationDirection.CW ? -1 : 1;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        dashSpeed = trial.dashSpeed;

        selfRotate = trial.isSelfRotating;

        DrawCurve();
    }

    public void EnableLine()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = true;
    }

    public void DisableLine()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }
}

