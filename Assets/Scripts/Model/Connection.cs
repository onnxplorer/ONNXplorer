using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Connection {
    //NOTE See `Neuron` for comments common to both classes

    public LineRef line;

    public Neuron source;
    public Neuron target;

    // Metadata to be added below.  Updates to metadata should (at some point during execution) update `line` with whatever visual properties they affect
    public float weight;
    public float activation;

    public Connection() {
        this.source = null;
        this.target = null;
        this.line = new LineRef();
        this.line.va = Vector3.zero;
        this.line.vb = Vector3.zero;
        this.weight = 1;
        this.activation = 0;
    }

    public Connection(Neuron source, Neuron target) : this(source, target, new LineRef(), 1, 0) {
    }

    public Connection(Neuron source, Neuron target, float weight, float activation) : this(source, target, new LineRef(), weight, activation) {
    }

    public Connection(Neuron source, Neuron target, LineRef lineRef, float weight, float activation) {
        this.source = source;
        this.target = target;
        this.line = lineRef;
        this.line.va = source.point.v;
        this.line.vb = target.point.v;
        this.weight = weight;
        this.activation = activation;
    }
}
