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
            var t = new Timing().push("Renderer recompute");
            t.log("Renderer recomputing " + dirty.Count + " elements");
            Vector3[] vs = lineMesh.vertices;
            Color[] cs = lineMesh.colors;
            foreach (RenderElementRef r in dirty) {
                if (r is LineRef) {
                    LineRef lr = (LineRef)r;
                    vs[lr._idx] = lr.va;
                    vs[lr._idx + 1] = lr.vb;
                    cs[lr._idx] = lr.ca;
                    cs[lr._idx + 1] = lr.cb;
                } else if (r is PointRef) { //CHECK This is pretty terrible
                    PointRef pr = (PointRef)r;
                    float hs = pr.size / 2;
                    var xyz = new Vector3(pr.v.x - hs, pr.v.y - hs, pr.v.z - hs);
                    var xyZ = new Vector3(pr.v.x - hs, pr.v.y - hs, pr.v.z + hs);
                    var xYz = new Vector3(pr.v.x - hs, pr.v.y + hs, pr.v.z - hs);
                    var xYZ = new Vector3(pr.v.x - hs, pr.v.y + hs, pr.v.z + hs);
                    var Xyz = new Vector3(pr.v.x + hs, pr.v.y - hs, pr.v.z - hs);
                    var XyZ = new Vector3(pr.v.x + hs, pr.v.y - hs, pr.v.z + hs);
                    var XYZ = new Vector3(pr.v.x + hs, pr.v.y + hs, pr.v.z + hs);
                    var XYz = new Vector3(pr.v.x + hs, pr.v.y + hs, pr.v.z - hs);

                    for (int i = 0; i < 24; i++) {
                        cs[pr._idx + i] = pr.c;
                    }

                    int j = 0;
                    vs[pr._idx + j] = xyz; j++;
                    vs[pr._idx + j] = xYz; j++;
                    vs[pr._idx + j] = xyz; j++;
                    vs[pr._idx + j] = xyZ; j++;
                    vs[pr._idx + j] = xYZ; j++;
                    vs[pr._idx + j] = xYz; j++;
                    vs[pr._idx + j] = xYZ; j++;
                    vs[pr._idx + j] = xyZ; j++;
                    vs[pr._idx + j] = Xyz; j++;
                    vs[pr._idx + j] = XYz; j++;
                    vs[pr._idx + j] = Xyz; j++;
                    vs[pr._idx + j] = XyZ; j++;
                    vs[pr._idx + j] = XYZ; j++;
                    vs[pr._idx + j] = XYz; j++;
                    vs[pr._idx + j] = XYZ; j++;
                    vs[pr._idx + j] = XyZ; j++;
                    vs[pr._idx + j] = xyz; j++;
                    vs[pr._idx + j] = Xyz; j++;
                    vs[pr._idx + j] = xYz; j++;
                    vs[pr._idx + j] = XYz; j++;
                    vs[pr._idx + j] = xyZ; j++;
                    vs[pr._idx + j] = XyZ; j++;
                    vs[pr._idx + j] = xYZ; j++;
                    vs[pr._idx + j] = XYZ; j++;
                }
            }
            dirty.Clear();
            lineMesh.vertices = vs; // I was setting "= lineMesh.vertices" and it wasn't working; why not???  Does it duplicate the whole array or something???
            lineMesh.colors = cs;
            lineMesh.RecalculateBounds();
            //meshFilter.sharedMesh = lineMesh;
            t.pop();
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
        if (dirty.Count > 0) {
            recompute();
        }
    }

    private T[] AddToArray<T>(T[] array, params T[] elements) {
        T[] newArray = new T[array.Length + elements.Length];
        array.CopyTo(newArray, 0);
        elements.CopyTo(newArray, array.Length);
        return newArray;
    }

    //THINK Return tokens, to delete/overwrite elements later?  Return an actual object to be changed?
    /*
    //DUMMY Turns out this method of doing points doesn't work too well; too small and you can't see it, too large and it's not a point, and either way it changes apparent height based on your closeness
    public LineRef addPoint(Vector3 pos, Color color) {
        return addLine(pos, color, new Vector3(pos.x, pos.y+0.001f, pos.z), color);
    }

    public LineRef addPoint(Vector3 pos) {
        return addLine(pos, Color.white, new Vector3(pos.x, pos.y+0.001f, pos.z), Color.white);
    }
    //DUMMY Also add from PointRef
    */

    public PointRef addPoint(Vector3 pos) {
        return addPoint(pos, Color.white, PointRef.DEFAULT_SIZE);
    }

    public PointRef addPoint(Vector3 pos, Color color) {
        return addPoint(pos, color, PointRef.DEFAULT_SIZE);
    }

    //DUMMY Currently just wireframes a cube - make more cubelike, or make proper "point" rendering
    public PointRef addPoint(Vector3 pos, Color color, float size) {
        Vector3[] newVertices = lineMesh.vertices;
        Color[] newColors = lineMesh.colors;
        int newIndex = newVertices.Length;
        float hs = size / 2;
        var xyz = new Vector3(pos.x - hs, pos.y - hs, pos.z - hs);
        var xyZ = new Vector3(pos.x - hs, pos.y - hs, pos.z + hs);
        var xYz = new Vector3(pos.x - hs, pos.y + hs, pos.z - hs);
        var xYZ = new Vector3(pos.x - hs, pos.y + hs, pos.z + hs);
        var Xyz = new Vector3(pos.x + hs, pos.y - hs, pos.z - hs);
        var XyZ = new Vector3(pos.x + hs, pos.y - hs, pos.z + hs);
        var XYz = new Vector3(pos.x + hs, pos.y + hs, pos.z - hs);
        var XYZ = new Vector3(pos.x + hs, pos.y + hs, pos.z + hs);
        newVertices = AddToArray(newVertices,
            xyz, xYz,
            xyz, xyZ,
            xYZ, xYz,
            xYZ, xyZ,
            Xyz, XYz,
            Xyz, XyZ,
            XYZ, XYz,
            XYZ, XyZ,
            xyz, Xyz,
            xYz, XYz,
            xyZ, XyZ,
            xYZ, XYZ
            );
        newColors = AddToArray(newColors,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color
            );

        lineMesh.vertices = newVertices;
        lineMesh.colors = newColors;

        int[] newIndices = lineMesh.GetIndices(0);
        int[] additionalIndices = new int[24];
        for (int i = 0; i < additionalIndices.Length; i++) {
            additionalIndices[i] = newIndex + i;
        }
        newIndices = AddToArray(newIndices, additionalIndices);
        lineMesh.SetIndices(newIndices, MeshTopology.Lines, 0);

        lineMesh.RecalculateBounds();
        //lineMesh.RecalculateNormals();

        return new PointRef(flagDirty, newIndex, pos, color, size);
    }

    public PointRef addPoint(PointRef pointRef) {
        Vector3[] newVertices = lineMesh.vertices;
        Color[] newColors = lineMesh.colors;
        int newIndex = newVertices.Length;
        var pos = pointRef.v;
        var color = pointRef.c;
        float hs = pointRef.size / 2;
        var xyz = new Vector3(pos.x - hs, pos.y - hs, pos.z - hs);
        var xyZ = new Vector3(pos.x - hs, pos.y - hs, pos.z + hs);
        var xYz = new Vector3(pos.x - hs, pos.y + hs, pos.z - hs);
        var xYZ = new Vector3(pos.x - hs, pos.y + hs, pos.z + hs);
        var Xyz = new Vector3(pos.x + hs, pos.y - hs, pos.z - hs);
        var XyZ = new Vector3(pos.x + hs, pos.y - hs, pos.z + hs);
        var XYz = new Vector3(pos.x + hs, pos.y + hs, pos.z - hs);
        var XYZ = new Vector3(pos.x + hs, pos.y + hs, pos.z + hs);
        newVertices = AddToArray(newVertices,
            xyz, xYz,
            xyz, xyZ,
            xYZ, xYz,
            xYZ, xyZ,
            Xyz, XYz,
            Xyz, XyZ,
            XYZ, XYz,
            XYZ, XyZ,
            xyz, Xyz,
            xYz, XYz,
            xyZ, XyZ,
            xYZ, XYZ
            );
        newColors = AddToArray(newColors,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color,
            color, color
            );

        lineMesh.vertices = newVertices;
        lineMesh.colors = newColors;

        int[] newIndices = lineMesh.GetIndices(0);
        int[] additionalIndices = new int[24];
        for (int i = 0; i < additionalIndices.Length; i++) {
            additionalIndices[i] = newIndex + i;
        }
        newIndices = AddToArray(newIndices, additionalIndices);
        lineMesh.SetIndices(newIndices, MeshTopology.Lines, 0);

        lineMesh.RecalculateBounds();
        //lineMesh.RecalculateNormals();

        pointRef._flagDirty = flagDirty;
        pointRef._idx = newIndex;

        return pointRef;
    }

    public LineRef addLine(Vector3 pos1, Vector3 pos2) {
        return addLine(pos1, Color.white, pos2, Color.white);
    }

    public LineRef addLine(Vector3 pos1, Vector3 pos2, Color color) {
        return addLine(pos1, color, pos2, color);
    }
    
    //CHECK Refactor this to be less messy

    /// <summary>
    /// Uses `lineRef`'s properties to add the line, then overwrites `lineRef`'s idx and flagDirty function
    /// </summary>
    /// <param name="lineRef"></param>
    /// <returns></returns>
    public LineRef addLine(LineRef lineRef) {
        // Example of updating the vectors and colors dynamically

        // Add a new line segment
        Vector3[] newVertices = lineMesh.vertices;
        Color[] newColors = lineMesh.colors;

        // Add the new vertices and colors to the arrays
        int newIndex = newVertices.Length;
        newVertices = AddToArray(newVertices, lineRef.va, lineRef.vb);
        newColors = AddToArray(newColors, lineRef.ca, lineRef.cb);

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

        lineRef._flagDirty = flagDirty;
        lineRef._idx = newIndex;

        return lineRef;
    }

    public LineRef addLine(Vector3 pos1, Color color1, Vector3 pos2, Color color2) {
        Vector3[] newVertices = lineMesh.vertices;
        Color[] newColors = lineMesh.colors;
        int newIndex = newVertices.Length;
        newVertices = AddToArray(newVertices, pos1, pos2);
        newColors = AddToArray(newColors, color1, color2);

        lineMesh.vertices = newVertices;
        lineMesh.colors = newColors;

        int[] newIndices = lineMesh.GetIndices(0);
        newIndices = AddToArray(newIndices, newIndex, newIndex + 1);
        lineMesh.SetIndices(newIndices, MeshTopology.Lines, 0);

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

    public LineRef[] addLines(LineRef[] lines) { //RAINY Make better, like BetterLines
        Vector3[] newVertices = lineMesh.vertices;
        Color[] newColors = lineMesh.colors;

        // Add the new vertices and colors to the arrays
        int newIndex = newVertices.Length;
        {
            Vector3[] newVertexArray = new Vector3[newVertices.Length + lines.Length*2];
            Color[] newColorArray = new Color[newColors.Length + lines.Length * 2];
            newVertices.CopyTo(newVertexArray, 0);
            newColors.CopyTo(newColorArray, 0);
            for (int i = 0; i < lines.Length; i++) {
                newVertexArray[newVertices.Length + 2 * i + 0] = lines[i].va;
                newVertexArray[newVertices.Length + 2 * i + 1] = lines[i].vb;
                newColorArray[newColors.Length + 2 * i + 0] = lines[i].ca;
                newColorArray[newColors.Length + 2 * i + 1] = lines[i].cb;
            }
            newVertices = newVertexArray;
            newColors = newColorArray;
        }

        // Update the mesh with the new vertices and colors
        lineMesh.vertices = newVertices;
        lineMesh.colors = newColors;

        // Update the indices to include the new line segment
        int[] newIndices = lineMesh.GetIndices(0); //THINK Submesh?
        int[] additionalIndices = new int[lines.Length*2];
        for (int i = 0; i < lines.Length; i++) {
            additionalIndices[2*i+0] = newIndex + 2*i+0;
            additionalIndices[2*i+1] = newIndex + 2*i+1;
            lines[i]._idx = newIndex + 2*i+0;
            lines[i]._flagDirty = flagDirty;
        }
        newIndices = AddToArray(newIndices, additionalIndices);
        lineMesh.SetIndices(newIndices, MeshTopology.Lines, 0);

        // Notify Unity that the mesh has been updated
        lineMesh.RecalculateBounds();
        //lineMesh.RecalculateNormals();

        return lines;
    }

    /**
     * Starts a LineBatch on this Renderer.  Equivalent to LineBatch.start(renderer).
     */
    public LineBatch startLines() {
        return LineBatch.start(this);
    }
}
