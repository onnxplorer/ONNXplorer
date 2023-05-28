using Onnx;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Collections;

public class Manipulate {
    public static void AddActivationsToOutputs(ModelProto m, UsefulModelInfo info) {
        var existingOutputs = new HashSet<string>();
        info.OriginalOutputs = new string[m.Graph.Output.Count];
        var i = 0;
        foreach (var o in m.Graph.Output) {
            existingOutputs.Add(o.Name);
            info.OriginalOutputs[i] = o.Name;
        }
        foreach (var n in m.Graph.Node) {
            if (!existingOutputs.Contains(n.Output[0])) {
                var valueInfo = new ValueInfoProto();
                valueInfo.Name = n.Output[0];
                m.Graph.Output.Add(valueInfo);
            }
        }
    }

    public static UsefulModelInfo ModifyOnnxFile(string modelPath, string outputPath) {
        var info = new UsefulModelInfo();
        var model = ModelProto.Parser.ParseFrom(System.IO.File.OpenRead(modelPath));
        AddActivationsToOutputs(model, info);
        System.IO.File.WriteAllBytes(outputPath, model.ToByteArray());
        return info;
    }
}