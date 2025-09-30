using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ArrowGeneratorForDashed : MonoBehaviour
{
    public GameObject trefoilObject;
    public float stemLength;
    public float stemWidth;
    public float tipLength;
    public float tipWidth;
    public float tipOffset;
    public int pointIndex; // Index of the point in the LineRenderer to use as the tip origin

    [System.NonSerialized]
    public List<Vector3> verticesList;
    [System.NonSerialized]
    public List<int> trianglesList;

    private Mesh mesh;
    private LineRenderer trefoilLineRenderer;
    private MeshRenderer meshRenderer;


    void Start()
    {
        // Find the Trefoil object if not assigned in the inspector
        if (trefoilObject == null)
        {
            trefoilObject = GameObject.Find("TrefoilDashed");
        }

        // Make sure Mesh Renderer has a material
        mesh = new Mesh();
        this.GetComponent<MeshFilter>().mesh = mesh;

        // Create a red material and assign it to the MeshRenderer
        Material redMaterial = new(Shader.Find("Standard"))
        {
            color = Color.red
        };

        meshRenderer = this.GetComponent<MeshRenderer>();
        meshRenderer.material = redMaterial;
    }

    void Update()
    {
        if (trefoilObject != null)
        {
            GenerateArrow();
        }
        else
        {
            Debug.LogError("Trefoil object not found. Please assign it in the inspector.");
        }
    }

    // Arrow is generated with the arrowhead at the origin (as defined by pointIndex)
    // Arrow is generated facing the origin
    void GenerateArrow()
    {
        if (trefoilLineRenderer == null)
        {
            trefoilLineRenderer = trefoilObject.GetComponent<LineRenderer>();
            if (trefoilLineRenderer == null)
            {
                Debug.LogError("Invalid LineRenderer.");
                return;
            }
        }

        // Ensure pointIndex is within valid range
        pointIndex = Mathf.Clamp(pointIndex, 0, trefoilLineRenderer.positionCount - 1);

        // Setup
        verticesList = new List<Vector3>();
        trianglesList = new List<int>();

        // Retrieve the tip origin from the LineRenderer
        Vector3 tipOrigin = trefoilLineRenderer.GetPosition(pointIndex);

        // Calculate the direction from the tip origin to the origin
        Vector3 directionToOrigin = (tipOrigin - Vector3.zero).normalized;
        tipOrigin += (tipOffset + tipLength) * directionToOrigin;

        // Calculate the right vector for the arrow
        Vector3 rightVector = directionToOrigin;

        // Tip setup
        Vector3 tipPoint = tipOrigin - (Quaternion.Euler(0, 0, 90) * directionToOrigin * tipLength);
        float tipHalfWidth = tipWidth / 2;

        // Tip points
        verticesList.Add(tipPoint); // 0
        verticesList.Add(tipOrigin + (tipHalfWidth * rightVector)); // 1
        verticesList.Add(tipOrigin - (tipHalfWidth * rightVector)); // 2

        // Stem setup
        Vector3 stemEnd = tipOrigin + (Quaternion.Euler(0, 0, 90) * directionToOrigin * (stemLength));
        float stemHalfWidth = stemWidth / 2;

        // Stem points
        verticesList.Add(tipOrigin + (stemHalfWidth * rightVector)); // 3
        verticesList.Add(tipOrigin - (stemHalfWidth * rightVector)); // 4
        verticesList.Add(stemEnd + (stemHalfWidth * rightVector)); // 5
        verticesList.Add(stemEnd - (stemHalfWidth * rightVector)); // 6

        // Stem triangles
        trianglesList.Add(6);
        trianglesList.Add(5);
        trianglesList.Add(3);

        trianglesList.Add(6);
        trianglesList.Add(3);
        trianglesList.Add(4);

        // Tip triangles
        trianglesList.Add(1);
        trianglesList.Add(0);
        trianglesList.Add(2);

        // Add backface vertices (duplicate vertices)
        int verticesCount = verticesList.Count;
        for (int i = 0; i < verticesCount; i++)
        {
            verticesList.Add(verticesList[i]);
        }

        // Add backface triangles (reverse winding order)
        int trianglesCount = trianglesList.Count;
        for (int i = 0; i < trianglesCount; i += 3)
        {
            trianglesList.Add(trianglesList[i] + verticesCount);
            trianglesList.Add(trianglesList[i + 2] + verticesCount);
            trianglesList.Add(trianglesList[i + 1] + verticesCount);
        }

        // Assign lists to mesh
        mesh.vertices = verticesList.ToArray();
        mesh.triangles = trianglesList.ToArray();

        // Recalculate normals for lighting
        mesh.RecalculateNormals();
    }

    public void ResetTo(TrialManager.Trial trial)
    {
        // Clear the mesh data
        mesh.Clear();

        // Reset other properties to their initial values if needed
        pointIndex = trial.arrowPointIndex;
    }

    public void EnableMesh()
    {
        if (meshRenderer != null)
            meshRenderer.enabled = true;
    }

    public void DisableMesh()
    {
        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }
}
