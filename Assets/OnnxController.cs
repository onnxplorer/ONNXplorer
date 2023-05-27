using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnnxController : MonoBehaviour
{
    public Renderer renderer;

    //DUMMY This is test code
    private IEnumerator addLineLoop() {
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
        Debug.Log("Lines done!");
    }

    public bool RAND_COLOR = false;

    // Start is called before the first frame update
    void Start() {
        Debug.Log("-->OnnxController start");

        var t = new Timing().push("benchmark");

        int EDGEP_START = 9;
        t.log("Random color: " + RAND_COLOR);
        for (int edgep = EDGEP_START; edgep <= 14; edgep++) {
            int edge = 2;
            for (int i = 1; i < edgep; i++) {
                edge *= 2;
            }
            t.push(edge+"x"+edge);
            //RAINY Benchmark this, and vs normal addLine
            //DUMMY This is test code
            int NX = edge;
            int NZ = edge;
            t.log("vertices: " + (NX * NZ * 2 * 2));
            Vector3[] vertices = new Vector3[NX * NZ * 2 * 2];
            Color[] colors = new Color[NX * NZ * 2 * 2];
            //StartCoroutine(addLineLoop());
            int n = 0;
            for (int z = 0; z < NZ; z++) {
                for (int x = 0; x < NX; x++) {
                    vertices[n] = new Vector3(x / (NX*1f), edgep-EDGEP_START, z / (NZ*1f));
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

        Debug.Log("<--OnnxController start");
    }

    // Update is called once per frame
    void Update() {
    }
}
