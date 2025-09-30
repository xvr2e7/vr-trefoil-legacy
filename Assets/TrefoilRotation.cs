using UnityEngine;

public class TrefoilRotation : MonoBehaviour
{
    public int n = 3;
    public float R_1 = 2f, R_2 = 2f;
    public float width = 0.12f;
    public int segments = 1000;
    public float rotationSpeed = 60f;
    public TrialManager.RotationDirection rotationDirection;

    private LineRenderer lineRenderer;
    private float angle;
    private int direction;

    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.positionCount = segments;
        
        // Set the material color to black
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;

        direction = rotationDirection == TrialManager.RotationDirection.CW ? -1 : 1;

        DrawCurve();
    }

    private void Update()
    {
        // Increment the angle for rotation
        angle += rotationSpeed * Time.deltaTime * direction;

        Vector3 currentRotation = transform.localRotation.eulerAngles;
        currentRotation.z = angle;
        transform.localRotation = Quaternion.Euler(currentRotation);

        angle += rotationSpeed * Time.deltaTime * direction;
    }

    private void DrawCurve()
    {
        float x;
        float y;
        float z = 0f;
        float total_angle = 361f;

        float angle = 0f;

        for (int i = 0; i < segments; i++)
        {
            x = R_1 * Mathf.Cos(Mathf.Deg2Rad * angle) + R_2 * Mathf.Cos((1 - n) * Mathf.Deg2Rad * angle);
            y = R_1 * Mathf.Sin(Mathf.Deg2Rad * angle) + R_2 * Mathf.Sin((1 - n) * Mathf.Deg2Rad * angle);

            lineRenderer.SetPosition(i, new Vector3(x, y, z));

            angle += total_angle / segments;
        }

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
        lineRenderer.positionCount = segments;
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
