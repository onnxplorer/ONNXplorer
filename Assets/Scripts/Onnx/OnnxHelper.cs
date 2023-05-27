using Google.Protobuf;
using Onnx;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class OnnxHelper {
    public static void CreateModelProto(string modelPath) {
        var model = ModelProto.Parser.ParseFrom(File.OpenRead(modelPath));
        var tensors = new Dictionary<string, TensorInfo>();
        Debug.Log("ModelProto created");

        foreach (var input in model.Graph.Input) {
            tensors[input.Name] = TensorInfo.fromValueInfoProto(input, true);
        }

        foreach (var init in model.Graph.Initializer) {
            tensors[init.Name] = TensorInfo.fromTensorProto(init);
        }

        foreach (var op in model.Graph.Node) {
            Debug.Log($"Processing operator {op.Name}");
            for (int i = 0; i < op.Output.Count; i++) {
                tensors[op.Output[i]] = TensorInfo.fromOperatorOutput(op, i, tensors);
            }
        }
        Debug.Log("That's it for values");
    }
}