using UnityEngine;

public class CubeRenderer : MonoBehaviour {
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    //public Shader dummy;

    private void Start() {
        // Create a new empty game object
        GameObject cubeObject = new GameObject("CubeObject");

        // Add the necessary components
        meshRenderer = cubeObject.AddComponent<MeshRenderer>();
        meshFilter = cubeObject.AddComponent<MeshFilter>();

        /*
        // Set up the material for the cube
        meshRenderer.material = new Material(Shader.Find("Standard"));
        */


        // Create a material with the desired color
        //Material cubeMaterial = new Material(Shader.Find("Custom/LineShader"));
        //Material cubeMaterial = new Material(Shader.Find("Standard"));
        //Material cubeMaterial = new Material(dummy);
        Material cubeMaterial = new Material(Shader.Find("Legacy Shaders/Self-Illumin/VertexLit"));
        cubeMaterial.color = new Color(0f, 1f, 0.35f);
        // Assign the material to the mesh renderer
        meshRenderer.material = cubeMaterial;


        // Create the cube mesh
        Mesh cubeMesh = new Mesh();

        // Define the vertices of the cube
        Vector3[] vertices = new Vector3[]
        {
            // Front face
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),

            // Back face
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
        };

        // Define the triangles of the cube
        int[] triangles = new int[]
        {
            // Front face
            0, 1, 2,
            2, 3, 0,

            // Back face
            5, 4, 7,
            7, 6, 5,

            // Top face
            3, 2, 6,
            6, 7, 3,

            // Bottom face
            1, 0, 4,
            4, 5, 1,

            // Left face
            4, 0, 3,
            3, 7, 4,

            // Right face
            1, 5, 6,
            6, 2, 1
        };

        // Calculate normals for the cube
        Vector3[] normals = new Vector3[]
        {
            Vector3.back,
            Vector3.back,
            Vector3.forward,
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            Vector3.right
        };

        // Set the vertices, triangles, and normals of the cube mesh
        cubeMesh.vertices = vertices;
        cubeMesh.triangles = triangles;
        cubeMesh.normals = normals;

        // Assign the mesh to the mesh filter
        meshFilter.mesh = cubeMesh;
    }
}
