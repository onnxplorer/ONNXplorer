using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Neuron {
    //THINK The vectors in Connection are redundant with the vectors in its Neurons, keep them in sync; maybe reuse mesh vertices?
    public PointRef point; //THINK Possibly need better res than float32, with really big models

    //CHECK Consider how to abstract rendering - e.g. color mode, neuron hiding, etc.

    // Metadata to be added below.  Updates to metadata should (at some point during execution) update `line` with whatever visual properties they affect
    //THINK Mark/indicate/flag some of these as more transient than others?
    public float weight;
    public float activation;
}
