using UnityEngine;

public class LineRenderer : MonoBehaviour {
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    private void Start() {
        // Create a new empty game object
        GameObject lineObject = new GameObject("LineObject");

        // Add the necessary components
        meshRenderer = lineObject.AddComponent<MeshRenderer>();
        meshFilter = lineObject.AddComponent<MeshFilter>();

        // Create the line mesh
        Mesh lineMesh = new Mesh();
        lineMesh.vertices = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),    // Start point of line 1
            new Vector3(1f, 1f, 0f),    // End point of line 1

            new Vector3(-1f, 0f, 0f),   // Start point of line 2
            new Vector3(-2f, 2f, 0f),   // End point of line 2

            // Define additional lines as needed
        };

        // Define the indices for line segments
        lineMesh.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Lines, 0);

        // Define the colors for each vertex
        Color[] colors = new Color[]
        {
            Color.red,   // Color for line 1
            Color.green, // Color for line 1

            Color.blue,  // Color for line 2
            Color.yellow // Color for line 2

            // Define additional colors as needed
        };

        // Assign the colors to the mesh
        lineMesh.colors = colors;

        // Assign the mesh to the mesh filter
        meshFilter.mesh = lineMesh;

        // Create a material with the custom shader
        Material lineMaterial = new Material(Shader.Find("Custom/LineShader"));

        // Assign the material to the mesh renderer
        meshRenderer.material = lineMaterial;
    }
}
