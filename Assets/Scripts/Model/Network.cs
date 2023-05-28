using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Network {
    //THINK Sets instead of Lists?
    public List<Neuron> neurons = new List<Neuron>();
    public List<Connection> connections = new List<Connection>();
    public Dictionary<Neuron, HashSet<Connection>> n2c = new Dictionary<Neuron, HashSet<Connection>>();

    public void addConnection(Connection c) {
        connections.Add(c);
        if (!n2c.ContainsKey(c.source)) {
            n2c[c.source] = new HashSet<Connection>();
        }
        if (!n2c.ContainsKey(c.target)) {
            n2c[c.target] = new HashSet<Connection>();
        }
        n2c[c.source].Add(c);
        n2c[c.target].Add(c);
    }

    public void updatedNeuron(Neuron n) { //DUMMY Should update colors too, presumably
        var cs = n2c[n];
        if (cs != null) {
            foreach (Connection c in cs) {
                c.line.va = c.source.point.v;
                c.line.vb = c.target.point.v;
            }
        }
    }
    
    /*
     * vertices
     *   +indices
     * colors
     *   +indices
     * flagDirty
     *   since WE'RE storing the arrays, no need to track which ones are dirty
     * point size
     * neuron weights
     * neuron activations
     * connection pairs
     */
}
