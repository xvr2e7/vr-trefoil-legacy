using UnityEngine;

public class Plot3DKnot : MonoBehaviour
{
    public int resolution = 1000; // Number of points
    public float rotationSpeed = 5f; // Rotation speed
    public Vector2 areaSize = new(500, 500); // The size of the area (a*b) on screen in pixels
    public Vector2 areaPosition = new(750, 100); // The top-left corner position of the area in pixels

    private TubeRenderer tubeRenderer;
    private LineRenderer xAxisRenderer;
    private LineRenderer yAxisRenderer;
    private LineRenderer zAxisRenderer;
    private GameObject centerEyeAnchor;

    void Start()
    {
        // Add a LineRenderer component to the GameObject for the shape
        tubeRenderer = gameObject.AddComponent<TubeRenderer>();

        // Set the tube renderer's width equivalent to 0.1f of the line renderer
        tubeRenderer.radius = 0.05f;

        // Set this object to the BothEyes layer
        gameObject.layer = LayerMask.NameToLayer("BothEyes");

        // Generate and set points for the shape
        tubeRenderer.points = GeneratePoints();

        // Add LineRenderers for the coordinate axes with tick marks
        xAxisRenderer = CreateAxisRenderer(Color.red, new Vector3[] { new(0, 0, 0), new(4, 0, 0) }, "X");
        yAxisRenderer = CreateAxisRenderer(Color.green, new Vector3[] { new(0, 0, 0), new(0, 4, 0) }, "Y");
        zAxisRenderer = CreateAxisRenderer(Color.blue, new Vector3[] { new(0, 0, 0), new(0, 0, 8) }, "Z");

        // Get the CenterEyeAnchor GameObject
        centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
        if (centerEyeAnchor == null)
        {
            Debug.LogError("CenterEyeAnchor not found. Make sure to set the CenterEyeAnchor GameObject in the Inspector.");
        }
    }

    void Update()
    {
        // Update the shape with new points
        tubeRenderer.points = GeneratePoints();
    }

    Vector3[] GeneratePoints()
    {
        Vector3[] points = new Vector3[resolution + 1]; // +1 to close the loop

        for (int i = 0; i <= resolution; i++)
        {
            float t = i * 2 * Mathf.PI / resolution;
            float x = Mathf.Sin(t) + 1.5f * Mathf.Sin(2 * t);
            float y = Mathf.Cos(t) - 1.5f * Mathf.Cos(2 * t);
            float z = -Mathf.Sin(3 * t);

            points[i] = new Vector3(x, y, z);
        }

        return points;
    }

    LineRenderer CreateAxisRenderer(Color color, Vector3[] positions, string axisName)
    {
        GameObject axisObject = new(axisName + "Axis")
        {
            // Set the axis object to the BothEyes layer
            layer = LayerMask.NameToLayer("BothEyes")
        };

        LineRenderer axisRenderer = axisObject.AddComponent<LineRenderer>();

        axisRenderer.positionCount = positions.Length;
        axisRenderer.useWorldSpace = false;

        axisRenderer.startWidth = 0.005f;
        axisRenderer.endWidth = 0.005f;
        axisRenderer.material = new Material(Shader.Find("Sprites/Default"))
        {
            color = color
        };

        axisRenderer.SetPositions(positions);

        axisObject.transform.parent = this.transform;
        axisObject.transform.SetLocalPositionAndRotation(new Vector3(-3, -3, -5), Quaternion.identity);
        AddTickMarks(axisObject, axisName);
        axisObject.transform.localScale = new Vector3(1, 1, 1);

        return axisRenderer;
    }

    void AddTickMarks(GameObject axisObject, string axisName)
    {
        int tickCount = 10;
        if (axisName == "Z") tickCount = 20; // Fewer tick marks for the Z axis
        float tickSize = 0.2f;

        for (int i = 1; i <= tickCount; i++)
        {
            float position = i * 0.4f; // Adjust the tick positions along the axis

            GameObject tick = new(axisName + "Tick" + i)
            {
                // Set the tick object to the BothEyes layer
                layer = LayerMask.NameToLayer("BothEyes")
            };

            LineRenderer tickRenderer = tick.AddComponent<LineRenderer>();

            tickRenderer.positionCount = 2;
            tickRenderer.useWorldSpace = false;

            tickRenderer.startWidth = 0.005f;
            tickRenderer.endWidth = 0.005f;
            tickRenderer.material = new Material(Shader.Find("Sprites/Default"))
            {
                color = Color.black
            };

            Vector3 tickStart, tickEnd;

            if (axisName == "X")
            {
                tickStart = new Vector3(position, -tickSize, 0);
                tickEnd = new Vector3(position, tickSize, 0);
            }
            else if (axisName == "Y")
            {
                tickStart = new Vector3(-tickSize, position, 0);
                tickEnd = new Vector3(tickSize, position, 0);
            }
            else // Z axis
            {
                tickStart = new Vector3(0, -tickSize, position);
                tickEnd = new Vector3(0, tickSize, position);
            }

            tickRenderer.SetPositions(new Vector3[] { tickStart, tickEnd });

            tick.transform.parent = axisObject.transform;
            tick.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}
