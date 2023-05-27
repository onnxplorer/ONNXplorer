using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Renderer : MonoBehaviour {
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh lineMesh;
    private HashSet<RenderElementRef> dirty = new HashSet<RenderElementRef>();

    private void flagDirty(RenderElementRef r) {
        dirty.Add(r);
    }

    public void recompute() {
        if (dirty.Count > 0) {
            Vector3[] vs = lineMesh.vertices;
            Color[] cs = lineMesh.colors;
            foreach (LineRef lr in dirty) {
                vs[lr._idx] = lr.va;
                vs[lr._idx+1] = lr.vb;
                cs[lr._idx] = lr.ca;
                cs[lr._idx+1] = lr.cb;
            }
            dirty.Clear();
        }
    }

    private void Awake() {
        Debug.Log("-->Renderer awake");
        // Create a new empty game object
        GameObject lineObject = new GameObject("LineObject");
        lineObject.transform.parent = this.transform;

        // Add the necessary components
        meshRenderer = lineObject.AddComponent<MeshRenderer>();
        meshFilter = lineObject.AddComponent<MeshFilter>();

        // Set up the material for the lines
        meshRenderer.material = new Material(Shader.Find("Custom/LineShader"));

        // Create the initial line mesh
        lineMesh = new Mesh();
        lineMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //WARNING Note that this does not work on all systems, apparently...but probably most of them.

        lineMesh.vertices = new Vector3[0];
        lineMesh.colors = new Color[0];
        lineMesh.SetIndices(new int[0], MeshTopology.Lines, 0);

        // Assign the initial mesh to the mesh filter
        meshFilter.mesh = lineMesh;
        Debug.Log("<--Renderer awake");
    }

    private void Update() {
        //if (dirty.Count > 0) {
        //    recompute();
        //}
    }

    private T[] AddToArray<T>(T[] array, params T[] elements) {
        T[] newArray = new T[array.Length + elements.Length];
        array.CopyTo(newArray, 0);
        elements.CopyTo(newArray, array.Length);
        return newArray;
    }

    //THINK Return tokens, to delete/overwrite elements later?  Return an actual object to be changed?
    //DUMMY Turns out this method of doing points doesn't work too well; too small and you can't see it, too large and it's not a point, and either way it changes apparent height based on your closeness
    public LineRef addPoint(Vector3 pos, Color color) {
        return addLine(pos, color, new Vector3(pos.x, pos.y+0.001f, pos.z), color);
    }

    public LineRef addPoint(Vector3 pos) {
        return addLine(pos, Color.white, new Vector3(pos.x, pos.y+0.001f, pos.z), Color.white);
    }

    public LineRef addLine(Vector3 pos1, Vector3 pos2) {
        return addLine(pos1, Color.white, pos2, Color.white);
    }

    public LineRef addLine(Vector3 pos1, Vector3 pos2, Color color) {
        return addLine(pos1, color, pos2, color);
    }

    public LineRef addLine(Vector3 pos1, Color color1, Vector3 pos2, Color color2) {
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

        return new LineRef(flagDirty, newIndex, pos1, color1, pos2, color2);
    }

    public void addLines(Vector3[] vertices, Color[] colors) { //RAINY Make better, like BetterLines
        //THINK Check even count, equal counts?
        //RAINY Return LineRef[]?  Have separate functions, to avoid OOM errors?

        Vector3[] newVertices = lineMesh.vertices;
        Color[] newColors = lineMesh.colors;

        // Add the new vertices and colors to the arrays
        int newIndex = newVertices.Length;
        newVertices = AddToArray(newVertices, vertices);
        newColors = AddToArray(newColors, colors);

        // Update the mesh with the new vertices and colors
        lineMesh.vertices = newVertices;
        lineMesh.colors = newColors;

        // Update the indices to include the new line segment
        int[] newIndices = lineMesh.GetIndices(0); //THINK Submesh?
        int[] additionalIndices = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            additionalIndices[i] = newIndex + i;
        }
        newIndices = AddToArray(newIndices, additionalIndices);
        lineMesh.SetIndices(newIndices, MeshTopology.Lines, 0);

        // Notify Unity that the mesh has been updated
        lineMesh.RecalculateBounds();
        //lineMesh.RecalculateNormals();
    }
}
