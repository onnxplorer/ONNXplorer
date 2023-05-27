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
}
