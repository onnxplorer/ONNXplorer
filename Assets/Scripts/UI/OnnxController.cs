using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnnxController : MonoBehaviour {
    public bool SHOW_ORIGIN = true;
    public bool TEST_LINES_CR = true;
    public bool TEST_STRESS = false;
    public bool TEST_LINES_UPDATE = false;
    public bool TEST_LINES_UPDATE_1N = false;
    public bool TEST_POINTS_UPDATE_1N = false;
    public bool TEST_NET = false;
    public bool TEST_ONNX = false;

    public Renderer renderer;

    private IEnumerator testAddLines() {
        for (int i = 0; i < 1000; i++) {
            yield return new WaitForSeconds(0.1f);
            Vector3 va = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Vector3 vb = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Color ca = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            Color cb = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            renderer.addLine(va, ca, vb, cb);
            //renderer.addPoint(va, ca);
            //renderer.addPoint(vb, cb);
        }
        Debug.Log("Add lines done!");
    }

    private IEnumerator testUpdateRendererEveryNseconds(float n) {
        while (true) {
            yield return new WaitForSeconds(n);
            Debug.Log("Triggering recompute");
            renderer.recompute();
        }
    }

    private IEnumerator testUpdateLines() {
        int N = 100;
        LineRef[] lrs = new LineRef[N];
        for (int i = 0; i < N; i++) {
            Vector3 va = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Vector3 vb = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Color ca = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            Color cb = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            lrs[i] = renderer.addLine(va, ca, vb, cb);
        }
        Debug.Log("Lines added");
        StartCoroutine(testUpdateRendererEveryNseconds(1f));
        while (true) {
            yield return new WaitForSeconds(0.2f);
            for (int i = 0; i < N; i++) {
                lrs[i].va += new Vector3(0.05f, 0.01f, 0f);
                lrs[i].vb += new Vector3(0.05f, 0.01f, 0f);
            }
        }
    }

    private IEnumerator testUpdateOneOfManyLines() {
        int N = 10;
        var t = new Timing().push("OnnxController testUpdateOneOfManyLines");
        t.log("N = " + N);
        LineRef[] lrs = new LineRef[N];
        var batch = renderer.startLines(); //CHECK This could over-allocate memory
        for (int i = 0; i < N; i++) {
            Vector3 va = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Vector3 vb = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Color ca = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            Color cb = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            lrs[i] = batch.addLine(va, ca, vb, cb);
        }
        batch.stop();
        t.log(N + " lines added");
        while (true) {
            yield return new WaitForSeconds(10f);
            t.push("updating");
            int i = Random.Range(0, N);
            lrs[i].va += new Vector3(0.05f, 0.01f, 0f);
            lrs[i].vb += new Vector3(0.05f, 0.01f, 0f);
            lrs[i].ca = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            lrs[i].cb = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            t.pop();
        }
        t.pop();
    }

    private IEnumerator testUpdateOneOfManyPoints() {
        int N = 10;
        var t = new Timing().push("OnnxController testUpdateOneOfManyPoints");
        t.log("N = " + N);
        PointRef[] prs = new PointRef[N];
        //var batch = renderer.startLines(); //CHECK This could over-allocate memory
        for (int i = 0; i < N; i++) {
            Vector3 v = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Color c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            float s = Random.Range(0f, 0.1f);

            prs[i] = renderer.addPoint(v, c, s);
        }
        //batch.stop();
        t.log(N + " points added");
        while (true) {
            yield return new WaitForSeconds(10f);
            t.push("updating");
            int i = Random.Range(0, N);
            prs[i].v += new Vector3(0.05f, 0.01f, 0f);
            prs[i].c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            prs[i].size = Random.Range(0f, 0.1f);
            t.pop();
        }
        t.pop();
    }

    private IEnumerator testUpdateNet() {
        int N = 10;
        int C = 3;
        var t = new Timing().push("OnnxController testUpdateNet");
        t.log("N = " + N + " ; C = " + C);
        Network net = new Network();
        //DUMMY Add batch point processing
        //var batch = renderer.startLines(); //CHECK This could over-allocate memory
        for (int i = 0; i < N; i++) {
            Vector3 v = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Color c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            float s = Random.Range(0f, 0.1f);

            float weight = Random.Range(0f, 1f);
            float activation = Random.Range(0f, 1f);

            Neuron n = new Neuron(new PointRef(v, c, s), weight, activation);
            net.neurons.Add(n);
        }
        foreach (Neuron source in net.neurons) {
            for (int i = 0; i < C; i++) {
                var target = net.neurons[Random.Range(0, net.neurons.Count)];
                Color c1 = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                Color c2 = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                float weight = Random.Range(0f, 1f);
                float activation = Random.Range(0f, 1f);
                //DUMMY Feels like you should be able to specify the color of a connection without duplicating your point code
                Connection conn = new Connection(source, target, new LineRef(Vector3.zero, c1, Vector3.zero, c2), weight, activation);
                net.addConnection(conn);
            }
        }

        //DUMMY Batch, once relevant
        foreach (Neuron n in net.neurons) {
            renderer.addPoint(n.point);
        }
        foreach (Connection c in net.connections) {
            renderer.addLine(c.line);
        }

        //batch.stop();
        t.log(N + " neurons added");
        while (true) {
            yield return new WaitForSeconds(0.2f);
            t.push("updating");
            int i = Random.Range(0, N);
            net.neurons[i].point.v += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
            net.neurons[i].point.c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            net.neurons[i].point.size = Random.Range(0f, 0.1f);
            net.updatedNeuron(net.neurons[i]);
            t.pop();
        }
        t.pop();
    }

    public bool RAND_COLOR = false;

    // Start is called before the first frame update
    void Start() {
        var t = new Timing().push("OnnxController start");

        if (SHOW_ORIGIN) {
            renderer.addLine(Vector3.zero, Vector3.right, Color.red);
            renderer.addLine(Vector3.zero, Vector3.up, Color.green);
            renderer.addLine(Vector3.zero, Vector3.forward, Color.blue);
        }

        if (TEST_STRESS) {
            t.push("stress");
            int EDGEP_START = 9;
            t.log("Random color: " + RAND_COLOR);
            for (int edgep = EDGEP_START; edgep <= 14; edgep++) {
                int edge = 2;
                for (int i = 1; i < edgep; i++) {
                    edge *= 2;
                }
                t.push(edge + "x" + edge);
                //RAINY Benchmark this, and vs normal addLine
                //DUMMY This is test code
                int NX = edge;
                int NZ = edge;
                t.log("vertices: " + (NX * NZ * 2 * 2));
                Vector3[] vertices = new Vector3[NX * NZ * 2 * 2];
                Color[] colors = new Color[NX * NZ * 2 * 2];
                int n = 0;
                for (int z = 0; z < NZ; z++) {
                    for (int x = 0; x < NX; x++) {
                        vertices[n] = new Vector3(x / (NX * 1f), edgep - EDGEP_START, z / (NZ * 1f));
                        if (RAND_COLOR) {
                            colors[n] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                        } else {
                            colors[n] = Color.white;
                        }
                        n++;
                        vertices[n] = new Vector3((x + 1) / (NX * 1f), edgep - EDGEP_START, (z + 1) / (NZ * 1f));
                        if (RAND_COLOR) {
                            colors[n] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                        } else {
                            colors[n] = Color.white;
                        }
                        n++;
                        vertices[n] = new Vector3(x / (NX * 1f), edgep - EDGEP_START, z / (NZ * 1f));
                        if (RAND_COLOR) {
                            colors[n] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                        } else {
                            colors[n] = Color.white;
                        }
                        n++;
                        vertices[n] = new Vector3((x + 1) / (NX * 1f), edgep - EDGEP_START, (z - 1) / (NZ * 1f));
                        if (RAND_COLOR) {
                            colors[n] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                        } else {
                            colors[n] = Color.white;
                        }
                        n++;
                    }
                }
                renderer.addLines(vertices, colors);
                t.pop();
            }
        }

        if (TEST_LINES_CR) {
            t.push("lines coroutine");
            StartCoroutine(testAddLines());
            t.pop();
        }

        if (TEST_LINES_UPDATE) {
            t.push("lines update");
            StartCoroutine(testUpdateLines());
            t.pop();
        }

        if (TEST_LINES_UPDATE_1N) {
            t.push("lines 1-in-N update");
            StartCoroutine(testUpdateOneOfManyLines());
            t.pop();
        }

        if (TEST_POINTS_UPDATE_1N) {
            t.push("points 1-in-N update");
            StartCoroutine(testUpdateOneOfManyPoints());
            t.pop();
        }

        if (TEST_NET) {
            t.push("net");
            StartCoroutine(testUpdateNet());
            t.pop();
        }

        if (TEST_ONNX) {
            t.push("onnx");
            var inf = new Inference().run(null, int.MaxValue);
            t.log("got result: " + inf);

            Network net = new Network();
            //DUMMY Add batch point processing
            //var batch = renderer.startLines(); //CHECK This could over-allocate memory
            foreach (Neuron n in inf.Item1) {
                net.neurons.Add(n);
            }
            foreach (Connection c in inf.Item2) {
                net.addConnection(c);
            }

            renderer.addPoints(net.neurons.Select(n => n.point).ToArray());
            renderer.addLines(net.connections.Select(c => c.line).ToArray());

            t.pop();
        }

        t.pop();
    }

    // Update is called once per frame
    void Update() {
    }
}
