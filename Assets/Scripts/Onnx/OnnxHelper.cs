using Google.Protobuf;
using Onnx;
using System.IO;
using UnityEngine;

public class OnnxHelper {
    public static void CreateModelProto(string modelPath) {
        var model = ModelProto.Parser.ParseFrom(File.OpenRead(modelPath));
        Debug.Log("ModelProto created");
        foreach (var valueinfo in model.Graph.ValueInfo) {
            Debug.Log(valueinfo.Name);
        }
        Debug.Log("That's it for values");
    }
}