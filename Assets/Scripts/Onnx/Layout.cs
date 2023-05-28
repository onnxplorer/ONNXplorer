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

            var coordArrays = new CoordArrays(Math.Min(t0.Length, maxNeuronsPerTensor));

            var indices = new int[t.Rank];
            var layerNum = info.LayerNums[result.Name];
            var tensorPos = TensorPosition(info.PlaceInLayer[result.Name]);

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
            Debug.Log($"coordI = {coordI} for {result.Name}");
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
                } else {
                    Debug.LogWarning($"Skipping connections for {result.Name} {optype} from {string.Join(", ", info.OpInputs[result.Name])}");
                }
            }
        }
        return (coordArrayList, connectionArrayList);
    }

    static float NeuronCutoff(DenseTensor<float> t, int maxNeuronsPerTensor) {
        if (t.Length <= maxNeuronsPerTensor) {
            return float.PositiveInfinity;
        }
        var sorted = new float[t.Length];
        var i = 0;
        foreach (var x in t) {
            sorted[i] = Math.Abs(x);
            i++;
        }
        
        Array.Sort(sorted);
        var cutoff = sorted[Math.Max(0, sorted.Length - maxNeuronsPerTensor)];
        Debug.Log($"Cutoff: {cutoff}. {sorted[0]} {sorted[sorted.Length-1]}");
        return cutoff;
    }

    static CoordArrays CreateElementwiseConnections(int[] inputDims, int[] outputDims, int layer0, int layer1, int posInLayer0, int posInLayer1, int maxConnectionsPerTensor, System.Random random, DenseTensor<float> t0, DenseTensor<float> t1, float cutoff)
    {
        var coordArrays = new CoordArrays(maxConnectionsPerTensor * 2);
 
        // Display an error and skip if the tensors are different shapes
        if (!inputDims.SequenceEqual(outputDims)) {
            Debug.LogError($"Elementwise: Tensor has shape {string.Join(",",outputDims)} but other tensor has shape {string.Join(",",inputDims)}");
            return null;
        }

        var indices = new int[outputDims.Length];
        var coordI = 0;
        for (var i = 0; i < t0.Length; i++) {
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
            if (coordI >= maxConnectionsPerTensor) {
                Debug.LogWarning("Too many connections");
                break;
            }
            var positions = CreateConnectionCoords(layer0, posInLayer0, indices, layer1, posInLayer1, indices);
            var color0 = new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            var color1 = new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            coordArrays.Positions[2*coordI] = positions.Item1;
            coordArrays.Positions[2*coordI+1] = positions.Item2;
            coordArrays.Colors[2*coordI] = color0;
            coordArrays.Colors[2*coordI+1] = color1;
            coordI++;

            indices[0]++;
            for (var j = 0; j < outputDims.Length - 1; j++) {
                if (indices[j] < outputDims[j]) {
                    break;
                }
                indices[j] = 0;
                indices[j + 1]++;
            }
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