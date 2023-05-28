using Google.Protobuf;
using Onnx;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class OnnxHelper {
    public static (List<Neuron>, List<Connection>) CreateModelProto(string modelPath, Dictionary<string,long> dim_params, int breakEarly = 1) {
        var model = ModelProto.Parser.ParseFrom(File.OpenRead(modelPath));
        var tensors = new Dictionary<string, TensorInfo>();
        var random = new System.Random();

        Debug.Log("ModelProto created");

        // These are the inputs.
        foreach (var input in model.Graph.Input) {
            tensors[input.Name] = TensorInfo.FromInput(input, dim_params, random);
        }

        // These are the weights (and maybe some constants).
        foreach (var init in model.Graph.Initializer) {
            tensors[init.Name] = TensorInfo.FromTensorProto(init);
        }

        // These are the operators which make up the bulk of the graph.
        bool shouldBreak = false;
        var opnum = 0;
        foreach (var op in model.Graph.Node) {
            Debug.Log($"Processing operator {op.Name}");
            for (int i = 0; i < op.Output.Count; i++) {
                TensorInfo result = null;
                try {
                    result = TensorInfo.FromOperatorOutput(op, i, tensors, random);
                } catch (System.Exception e) {
                    Debug.LogError($"Error processing operator {op.Name}: {e}");
                }
                if (result == null) {
                    Debug.LogError("Bombing out");
                    shouldBreak = true;
                    break;
                }
                tensors[op.Output[i]] = result;
            }
            opnum++;
            if (opnum >= breakEarly) {
                Debug.LogError($"Quitting early on opnum {opnum}");
                break;
            }
            if (shouldBreak) {
                break;
            }
        }

        // Collect all the Neurons into a list
        var neurons = new List<Neuron>();
        var connections = new List<Connection>();
        var seen = new HashSet<Neuron>();
        foreach (var tensor in tensors.Values) {
            foreach (var scalar in tensor.GetAllScalars()) {
                if (scalar.GetNeuron != null && !seen.Contains(scalar.GetNeuron)) {
                    neurons.Add(scalar.GetNeuron);
                    seen.Add(scalar.GetNeuron);
                }
                if (scalar.GetConnections != null) {
                    foreach (var connection in scalar.GetConnections) {
                        connections.Add(connection);
                    }
                }
            }
        }

        Debug.Log($"That's it for values. Have {neurons.Count} neurons and {connections.Count} connections");
        return (neurons, connections);
    }
}