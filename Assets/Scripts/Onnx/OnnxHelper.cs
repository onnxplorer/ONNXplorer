using Google.Protobuf;
using Onnx;
using System.IO;
using UnityEngine;

public class OnnxHelper {
    public static void CreateModelProto() {
        var model_proto = ModelProto.Parser.ParseFrom(File.OpenRead("models/mobilenetv2-12-int8/mobilenetv2-12-int8.onnx"));
        Debug.Log("ModelProto created");
    }
}