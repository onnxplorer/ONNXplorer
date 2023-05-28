using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public class InferenceComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        new Inference().run((result) => {
            Debug.Log("Got result: " + result);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
