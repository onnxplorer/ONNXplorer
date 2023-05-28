using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron {
    //THINK The vectors in Connection are redundant with the vectors in its Neurons, keep them in sync; maybe reuse mesh vertices?
    public PointRef point; //THINK Possibly need better res than float32, with really big models

    //CHECK Consider how to abstract rendering - e.g. color mode, neuron hiding, etc.

    // Metadata to be added below.  Updates to metadata should (at some point during execution) update `line` with whatever visual properties they affect
    //THINK Mark/indicate/flag some of these as more transient than others?
    public float weight;
    public float activation;

    //DUMMY Color (and maybe size) depends somehow on weight/activation

    public Neuron() {
        this.point = new PointRef();
        this.weight = 1;
        this.activation = 0;
    }

    public Neuron(Vector3 v, float weight, float activation) {
        this.point = new PointRef(v, Color.white);
        this.weight = weight;
        this.activation = activation;
    }

    public Neuron(PointRef point, float weight, float activation) {
        this.point = point;
        this.weight = weight;
        this.activation = activation;
    }
}
