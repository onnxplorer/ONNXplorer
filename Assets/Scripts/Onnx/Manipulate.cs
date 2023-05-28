using Onnx;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Collections;
using System.Linq;

public class Manipulate {
    public static void AddActivationsToOutputs(ModelProto m, UsefulModelInfo info) {
        var existingOutputs = new HashSet<string>();
        var layerTensorCounts = new Dictionary<int, int>();

        info.OriginalOutputs = new string[m.Graph.Output.Count];
        var i = 0;
        foreach (var o in m.Graph.Output) {
            existingOutputs.Add(o.Name);
            info.OriginalOutputs[i] = o.Name;
        }
        foreach (var n in m.Graph.Node) {
            int layer = 0;
            foreach (var input in n.Input) {
                if (info.LayerNums.ContainsKey(input)) {
                    layer = System.Math.Max(layer, info.LayerNums[input] + 1);
                }
            }
            info.LayerNums[n.Output[0]] = layer;
            info.OpTypes[n.Output[0]] = n.OpType;
            info.OpInputs[n.Output[0]] = n.Input.ToArray();

            if (!layerTensorCounts.ContainsKey(layer)) {
                layerTensorCounts[layer] = 0;
            }
            info.PlaceInLayer[n.Output[0]] = layerTensorCounts[layer];
            layerTensorCounts[layer] += 1;

            if (!existingOutputs.Contains(n.Output[0])) {
                var valueInfo = new ValueInfoProto();
                valueInfo.Name = n.Output[0];
                m.Graph.Output.Add(valueInfo);
            }
        }
    }

    public static UsefulModelInfo ModifyOnnxFile(string modelPath, string outputPath) {
        var info = new UsefulModelInfo();
        info.LayerNums = new Dictionary<string, int>();
        info.PlaceInLayer = new Dictionary<string, int>();
        info.OpTypes = new Dictionary<string, string>();
        info.OpInputs = new Dictionary<string, string[]>();

        var model = ModelProto.Parser.ParseFrom(System.IO.File.OpenRead(modelPath));
        AddActivationsToOutputs(model, info);
        System.IO.File.WriteAllBytes(outputPath, model.ToByteArray());
        return info;
    }
}