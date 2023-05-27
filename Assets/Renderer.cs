using UnityEngine;

public class Renderer : MonoBehaviour {
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh lineMesh;

    private void Start() {
        // Create a new empty game object
        GameObject lineObject = new GameObject("LineObject");

        // Add the necessary components
        meshRenderer = lineObject.AddComponent<MeshRenderer>();
        meshFilter = lineObject.AddComponent<MeshFilter>();

        // Set up the material for the lines
        meshRenderer.material = new Material(Shader.Find("Custom/LineShader"));

        // Create the initial line mesh
        lineMesh = new Mesh();
        lineMesh.vertices = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),    // Start point of line 1
            new Vector3(1f, 1f, 0f),    // End point of line 1

            new Vector3(-1f, 0f, 0f),   // Start point of line 2
            new Vector3(-2f, 2f, 0f),   // End point of line 2

            // Define additional lines as needed
        };
        lineMesh.colors = new Color[]
        {
            Color.red,   // Color for line 1
            Color.green, // Color for line 1

            Color.blue,  // Color for line 2
            Color.yellow // Color for line 2

            // Define additional colors as needed
        };
        lineMesh.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Lines, 0);

        // Assign the initial mesh to the mesh filter
        meshFilter.mesh = lineMesh;
    }

    private T[] AddToArray<T>(T[] array, params T[] elements) {
        T[] newArray = new T[array.Length + elements.Length];
        array.CopyTo(newArray, 0);
        elements.CopyTo(newArray, array.Length);
        return newArray;
    }



    //THINK Return tokens, to delete/overwrite elements later?  Return an actual object to be changed?
    //DUMMY Turns out this method of doing points doesn't work too well; too small and you can't see it, too large and it's not a point, and either way it changes apparent height based on your closeness
    public void addPoint(Vector3 pos, Color color) {
        addLine(pos, color, new Vector3(pos.x, pos.y+0.001f, pos.z), color);
    }

    public void addPoint(Vector3 pos) {
        addLine(pos, Color.white, new Vector3(pos.x, pos.y+0.001f, pos.z), Color.white);
    }

    public void addLine(Vector3 pos1, Vector3 pos2) {
        addLine(pos1, Color.white, pos2, Color.white);
    }

    public void addLine(Vector3 pos1, Vector3 pos2, Color color) {
        addLine(pos1, color, pos2, color);
    }

    public void addLine(Vector3 pos1, Color color1, Vector3 pos2, Color color2) {
        // Example of updating the vectors and colors dynamically

        // Add a new line segment
        Vector3[] newVertices = lineMesh.vertices;
        Color[] newColors = lineMesh.colors;

        // Define the start and end points of the new line segment
        Vector3 startPoint = pos1;
        Vector3 endPoint = pos2;

        // Add the new vertices and colors to the arrays
        int newIndex = newVertices.Length;
        newVertices = AddToArray(newVertices, startPoint, endPoint);
        newColors = AddToArray(newColors, color1, color2);

        // Update the mesh with the new vertices and colors
        lineMesh.vertices = newVertices;
        lineMesh.colors = newColors;

        // Update the indices to include the new line segment
        int[] newIndices = lineMesh.GetIndices(0);
        newIndices = AddToArray(newIndices, newIndex, newIndex + 1);
        lineMesh.SetIndices(newIndices, MeshTopology.Lines, 0);

        // Notify Unity that the mesh has been updated
        lineMesh.RecalculateBounds();
        //lineMesh.RecalculateNormals();
    }
}
