using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnnxController : MonoBehaviour
{
    public Renderer renderer;

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

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(addLineLoop());
    }

    // Update is called once per frame
    void Update()
    {
    }
}
