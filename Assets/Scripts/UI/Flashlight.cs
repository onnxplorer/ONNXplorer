using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

//RAINY //CHECK Should it also permit rotate in a y plane?
public class Flashlight : MonoBehaviour {
    public Transform flashlight;
    public Transform networkTransform;
    public Network network;

    //DUMMY Possible optimization
    private HashSet<Connection> reds = new HashSet<Connection>();
    private HashSet<Connection> blues = new HashSet<Connection>();

    private void Update() {
        if (flashlight != null && networkTransform != null && network != null) {
            var p = networkTransform.InverseTransformPoint(flashlight.transform.position);
            var s0 = networkTransform.lossyScale;
            var s = System.Math.Pow(System.Math.Abs(s0.x * s0.y * s0.z), 1.0 / 3);
            //DUMMY Make faster
            //HashSet<Neuron> close = new HashSet<Neuron>();
            HashSet<Connection> colored = new HashSet<Connection>();
            foreach (Neuron n in network.neurons) {
                if (Vector3.Distance(p, n.point.v) < (0.01f / s)) {
                    //close.Add(n);
                    var cs = network.n2c[n];
                    foreach (Connection c in cs) {
                        colored.Add(c);
                        if (n == c.target) {
                            c.line.ca = Color.blue;
                            c.line.cb = Color.blue;
                            reds.Remove(c);
                            blues.Add(c);
                        } else if (n == c.source) {
                            c.line.ca = Color.red;
                            c.line.cb = Color.red;
                            blues.Remove(c);
                            reds.Add(c);
                        } else {
                            //???
                        }
                    }
                } else {
                    // Not close, I guess?
                }
            }
            // Change back deselected Connections
            foreach (Connection c in network.connections) {
                if ((reds.Contains(c) || blues.Contains(c)) && !colored.Contains(c)) {
                    c.line.ca = Color.grey; //RAINY Should have some way of specifying default color, or maybe some kind of override hierarchy
                    c.line.cb = Color.grey;
                }
            }
            //foreach (Neuron n in close) {
            //    n.point.c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            //}
        }
    }
}