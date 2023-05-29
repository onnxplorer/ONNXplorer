using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;

public class Layout {
    public const float LAYER_SX = 1f; //DUMMY This should probably normally be 1f or something, or maybe computed
    public static readonly float[] OFFSET = { 0.5f, 0.5f, 0.5f };
    public const float BF = 0.01f; // Sorta depends on how wide the tensors are...
    public static readonly float[,] BASIS = { //RAINY There's probably a nice math library we've already imported that would make the multiplication more efficient or something
        { 0f, 0f, BF },
        { 0f, -BF, 0f },
        { BF, -BF/3, BF/3 },
        { BF/2, -BF/2, BF/2 },
        { BF/4, BF/8, -BF/8 },
        { -BF/8, BF/16, -BF/16 }, // These are quite arbitrary from here on
        { BF/32, -BF/32, -BF/32 },
        { -BF/64, -BF/64, BF/64 },
    };

    static Vector3 Position(int layer, int[] layerPosition, Vector3 positionOffset)
    {
        float x = LAYER_SX*layer + OFFSET[0] + positionOffset.x;
        float y = OFFSET[1] + positionOffset.y;
        float z = OFFSET[2] + positionOffset.z;
        int j = 0;
        for (int i = layerPosition.Length - 1; i >= 0; i--) {
            x += BASIS[j, 0] * layerPosition[i];
            y += BASIS[j, 1] * layerPosition[i];
            z += BASIS[j, 2] * layerPosition[i];
            j++; //SHAME shrug
        }
        return new Vector3(x, y, z);
    }

    static Color Color(float activation) {
        if (activation > 0) {
            return new Color(1, 1 - activation, 1 - activation);
        } else {
            return new Color(1 + activation, 1 + activation, 1);
        }
    }

    static Color WeightColor(float weight) {
        if (weight > 0) {
            return new Color(0, weight, 0);
        } else {
            return new Color(-weight, -weight, 0);
        }
    }

    static Vector3 TensorPosition(int placeInLayer)
    {
        return new Vector3(0, placeInLayer, 0);
    }

    public static (List<CoordArrays>,List<CoordArrays>) GetCoordArrays(UsefulModelInfo info, IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results, int maxNeuronsPerTensor, int maxConnectionsPerTensor)
    {
        var random = new System.Random(0);
        var coordArrayList = new List<CoordArrays>();
        var connectionArrayList = new List<CoordArrays>();
        var dimensions = new Dictionary<string, int[]>();
        var tensorDict = new Dictionary<string, DenseTensor<float>>();
        foreach (var result in results) {
            var t0 = result.AsTensor<float>();
            if (t0 == null) {
                continue;
            }

            dimensions[result.Name] = t0.Dimensions.ToArray();

            var t = t0.ToDenseTensor();
            tensorDict[result.Name] = t;

            var cutoff = NeuronCutoff(t, maxNeuronsPerTensor);

            if (info.Constants.Contains(result.Name)) {
                Debug.Log($"Constant Tensor Name = {result.Name}, Dimensions = {string.Join(", ", dimensions[result.Name])}");
                continue;
            }

            var opname = "";
            info.OpNames.TryGetValue(result.Name, out opname);

            var coordArrays = new CoordArrays(Math.Min(t0.Length, maxNeuronsPerTensor));

            var indices = new int[t.Rank];
            var layerNum = info.LayerNums[result.Name];
            var tensorPos = TensorPosition(info.PlaceInLayer[result.Name]);

            Debug.Log($"Tensor Name = {result.Name}, Op Name = {opname}, Layer = {layerNum}, Place = {info.PlaceInLayer[result.Name]}, Dimensions = {string.Join(", ", dimensions[result.Name])}");

            var coordI = 0;
            for (var i = 0; i < t.Length; i++) {
                var activation = t[indices];

                if (Math.Abs(activation) <= cutoff) {
                    indices[0]++;
                    for (var j = 0; j < t.Rank - 1; j++) {
                        if (indices[j] < t.Dimensions[j]) {
                            break;
                        }
                        indices[j] = 0;
                        indices[j + 1]++;
                    }
                    continue;
                }
                if (coordI >= maxNeuronsPerTensor) {
                    Debug.LogError("coordI >= maxNeuronsPerTensor");
                    break;
                }

                var position = Position(layerNum, indices, tensorPos);
                var color = Color(activation);

                coordArrays.Positions[coordI] = position;
                coordArrays.Colors[coordI] = color;

                indices[0]++;
                for (var j = 0; j < t.Rank - 1; j++) {
                    if (indices[j] < t.Dimensions[j]) {
                        break;
                    }
                    indices[j] = 0;
                    indices[j + 1]++;
                }
                coordI += 1;
            }
            coordArrays.Trim(2 * coordI);

            if (coordArrays.Length > 2) {
                coordArrayList.Add(coordArrays);
            }

            // Now deal with connections
            if (info.OpTypes.ContainsKey(result.Name)) {
                var optype = info.OpTypes[result.Name];

                // Local function to factor out connectionArrays logic
                void AddElementwiseConnectionArrays(string input) {
                    if (dimensions.ContainsKey(input)) {
                        var inputDims = dimensions[input];
                        var outputDims = dimensions[result.Name];

                        var connectionArrays = CreateElementwiseConnections(inputDims, outputDims, info.LayerNums[input], info.LayerNums[result.Name], info.PlaceInLayer[input], info.PlaceInLayer[result.Name], maxConnectionsPerTensor, random, tensorDict[input], tensorDict[result.Name], cutoff);
                        if (connectionArrays != null) {
                            connectionArrayList.Add(connectionArrays);
                        }
                    } else {
                        Debug.Log($"No tensorNeuronBlockIndex entry for {input}. It's probably an input layer, parameter or constant");
                    }
                }

                var numInputs = info.OpInputs[result.Name].Length;
                if (optype == "Add" && numInputs == 2) {
                    AddElementwiseConnectionArrays(info.OpInputs[result.Name][0]);
                    AddElementwiseConnectionArrays(info.OpInputs[result.Name][1]);
                } else if (optype == "Clip" && numInputs == 1) {
                    AddElementwiseConnectionArrays(info.OpInputs[result.Name][0]);
                } else if (optype == "Conv" && numInputs >= 2) {
                    var input = info.OpInputs[result.Name][0];
                    var wInput = info.OpInputs[result.Name][1];
                    if (dimensions.ContainsKey(input) && dimensions.ContainsKey(wInput)) {
                        var inputDims = dimensions[input];
                        var outputDims = dimensions[result.Name];
                        var inputT = tensorDict[input];
                        var weightT = tensorDict[wInput];
                        var outputT = tensorDict[result.Name];

                        var connectionArrays = CreateConvConnnections(
                            inputDims,
                            outputDims,
                            info.LayerNums[input],
                            info.LayerNums[result.Name],
                            info.PlaceInLayer[input],
                            info.PlaceInLayer[result.Name],
                            maxConnectionsPerTensor,
                            inputT, weightT, outputT, cutoff, info.ConvParams[result.Name]
                        );
                        connectionArrayList.Add(connectionArrays);
                    }
                } else {
                    Debug.LogWarning($"Skipping connections for {result.Name} {optype} from {string.Join(", ", info.OpInputs[result.Name])}");
                }
            }
        }
        return (coordArrayList, connectionArrayList);
    }

    static float NeuronCutoff(DenseTensor<float> t, int maxNeuronsPerTensor) {
        if (t.Length <= maxNeuronsPerTensor) {
            return -1;
        }
        var sorted = new float[t.Length];
        var i = 0;
        foreach (var x in t) {
            sorted[i] = Math.Abs(x);
            i++;
        }
        
        Array.Sort(sorted);
        var cutoff = sorted[Math.Max(0, sorted.Length - maxNeuronsPerTensor)];
        return cutoff;
    }

    static CoordArrays CreateConvConnnections(
        int[] inputDims, int[] outputDims, int layer0, int layer1, int posInLayer0, int posInLayer1, int maxConnectionsPerTensor, DenseTensor<float> t0, DenseTensor<float> w, DenseTensor<float> t1, float cutoff, ConvParams conv)
    {
        var indices = new int[4];
        var pqueue = new PQueue<(Vector3,Vector3,Color),float>();
        for (var i = 0; i < t1.Length; i++) {
            if (Math.Abs(t1[indices]) <= cutoff) {
                indices[0]++;
                for (var j = 0; j < 3; j++) {
                    if (indices[j] < outputDims[j]) {
                        break;
                    }
                    indices[j] = 0;
                    indices[j + 1]++;
                }
                continue;
            }

            var position1 = Position(layer1, indices, TensorPosition(posInLayer1));

            int numOutputChannels = outputDims[1] / (int)conv.Group;
            int whichGroup = indices[1] / numOutputChannels;
            int whichOutputChannel = indices[1] % numOutputChannels;

            int numInputChannels = inputDims[1] / (int)conv.Group;
            for (var whichInputChannel = 0; whichInputChannel < numInputChannels; whichInputChannel++) {
                for (var kx = 0; kx < conv.KernelShape[0]; kx++) {
                    for (var ky = 0; ky < conv.KernelShape[1]; ky++) {
                        var indices0 = new int[]{
                            indices[0],
                            whichInputChannel + whichGroup * numInputChannels,
                            indices[2] + kx * (int)conv.Strides[0] - (int)conv.Pads[0],
                            indices[3] + ky * (int)conv.Strides[1] - (int)conv.Pads[2],
                        };
                        if (indices0[2] < 0 || indices0[2] >= inputDims[2] || indices0[3] < 0 || indices0[3] >= inputDims[3]) {
                            // Padded.
                            continue;
                        }
                        if (t0[indices0] <= cutoff) {
                            continue;
                        }
                        var position0 = Position(layer0, indices0, TensorPosition(posInLayer0));
                        var weight = w[whichOutputChannel, whichInputChannel, kx, ky];
                        var color = WeightColor(weight);
                        pqueue.Enqueue((position0, position1, color), -Math.Abs(weight));
                        if (pqueue.Count > maxConnectionsPerTensor) {
                            pqueue.Dequeue();
                        }
                    }
                }
            }

            indices[0]++;
            for (var j = 0; j < 3; j++) {
                if (indices[j] < outputDims[j]) {
                    break;
                }
                indices[j] = 0;
                indices[j + 1]++;
            }
        }

        var coordI = 0;
        var coordArrays = new CoordArrays(pqueue.Count * 2);
        while (pqueue.Count > 0) {
            var (position0, position1, color) = pqueue.Dequeue();
            coordArrays.Positions[2*coordI] = position0;
            coordArrays.Colors[2*coordI] = color;
            coordArrays.Positions[2*coordI+1] = position1;
            coordArrays.Colors[2*coordI+1] = color;
            coordI++;
        }
        coordArrays.Trim(2 * coordI);
        return coordArrays;
    }

    static CoordArrays CreateElementwiseConnections(int[] inputDims, int[] outputDims, int layer0, int layer1, int posInLayer0, int posInLayer1, int maxConnectionsPerTensor, System.Random random, DenseTensor<float> t0, DenseTensor<float> t1, float cutoff)
    {
        // Display an error and skip if the tensors are different shapes
        if (!inputDims.SequenceEqual(outputDims)) {
            Debug.LogError($"Elementwise: Tensor has shape {string.Join(",",outputDims)} but other tensor has shape {string.Join(",",inputDims)}");
            return null;
        }

        var indices = new int[outputDims.Length];
        var pqueue = new PQueue<(Vector3,Vector3,Color),float>();
        for (var i = 0; i < t1.Length; i++) {
            if (Math.Abs(t0[indices]) <= cutoff || Math.Abs(t1[indices]) <= cutoff) {
                indices[0]++;
                for (var j = 0; j < outputDims.Length - 1; j++) {
                    if (indices[j] < outputDims[j]) {
                        break;
                    }
                    indices[j] = 0;
                    indices[j + 1]++;
                }
                continue;
            }
            var (pos0,pos1) = CreateConnectionCoords(layer0, posInLayer0, indices, layer1, posInLayer1, indices);
            var color = new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            var pseudo_weight = (float)random.NextDouble();
            
            pqueue.Enqueue((pos0,pos1,color), pseudo_weight);
            if (pqueue.Count > maxConnectionsPerTensor) {
                pqueue.Dequeue();
            }

            indices[0]++;
            for (var j = 0; j < outputDims.Length - 1; j++) {
                if (indices[j] < outputDims[j]) {
                    break;
                }
                indices[j] = 0;
                indices[j + 1]++;
            }
        }

        var coordI = 0;
        var coordArrays = new CoordArrays(pqueue.Count * 2);
        while (pqueue.Count > 0) {
            var (position0, position1, color) = pqueue.Dequeue();
            coordArrays.Positions[2*coordI] = position0;
            coordArrays.Colors[2*coordI] = color;
            coordArrays.Positions[2*coordI+1] = position1;
            coordArrays.Colors[2*coordI+1] = color;
            coordI++;
        }
        coordArrays.Trim(2 * coordI);
        return coordArrays;
    }

    static (Vector3, Vector3) CreateConnectionCoords(int layer0, int posInLayer0, int[] ix0, int layer1, int posInLayer1, int[] ix1) {
        var pos0 = Position(layer0, ix0, TensorPosition(posInLayer0));
        var pos1 = Position(layer1, ix1, TensorPosition(posInLayer1));
        return (pos0, pos1);
    }
}