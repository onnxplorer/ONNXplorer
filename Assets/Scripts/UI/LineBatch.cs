using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * <summary>
 * Example:<br/>
 * <br/>
 * <code>
 *   var greenLines = LineBatch.start(lineRenderer);
 *   greenLines.lgl.LineColor = Color.green; // Optional
 *   greenLines.lgl.LineRadius = 0.0001f; // Optional.  You can mess with the other properties, too, though not ALL will work.
 *   
 *   greenLines.addLine(new Vector3(0, 0, 1), new Vector3(1, 1, 1)); // from 0,0,1 to 1,1,1
 *   greenLines.addRay(new Vector3(0, 0, 1), new Vector3(1, 1, 1)); // from 0,0,1 to 1,1,2
 *   greenLines.addLine(new Vector3(1, 0, 1), new Vector3(0, 1, 0), Color.red); // red line
 *   
 *   greenLines.stop(); // Finalizes the lines
 *</code>
 *</summary>
 */
public class LineBatch { //CHECK Maybe more general than just lines?
    public Renderer lineRenderer;
    public List<LineRef> lgl = new List<LineRef>();

    public LineBatch(Renderer lineRenderer) {
        this.lineRenderer = lineRenderer;
    }

    /**
     * Start drawing lines.  Just returns a new LineBatch; a decoration method to pair start and stop.
     * 
     * lineRenderer - a reference to whatever Renderer you have running in your scene
     */
    public static LineBatch start(Renderer lineRenderer) {
        return new LineBatch(lineRenderer);
    }

    /**
     * Resets the points list.  Mostly decorative - the stop function resets the list, too.
     */
    public void start() {
        lgl = new List<LineRef>();
    }

    /**
     * Creates a line from va to vb.
     * Warning: the LineRef has not been given to the Renderer and assigned an index etc until `stop` has been called.
     */
    public LineRef addLine(Vector3 va, Color ca, Vector3 vb, Color cb) {
        var lr = new LineRef(va, ca, vb, cb);
        lgl.Add(lr);
        return lr;
    }

    /**
     * Add a line from p to (p+q).
     */
    public LineRef addRay(Vector3 p, Color c1, Vector3 q, Color c2) {
        var lr = new LineRef(p, c1, new Vector3(p.x + q.x, p.y + q.y, p.z + q.z), c2);
        lgl.Add(lr);
        return lr;
    }

    //RAINY Add the other overloads giving default params

    /**
     * Add the queued lines to the renderer.
     */
    public void stop() {
        lineRenderer.addLines(lgl.ToArray());
        lgl = new List<LineRef>();
    }
}
