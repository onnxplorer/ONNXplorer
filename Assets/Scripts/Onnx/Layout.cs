using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;
using System.Linq;

public class Layout {
    public const float LITTLE_LINE_LENGTH = 0.01f;
    public const float LAYER_SX = 0.1f; //DUMMY This should probably normally be 1f or something, or maybe computed
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

    public static (List<CoordArrays>,List<CoordArrays>) GetCoordArrays(UsefulModelInfo info, IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
    {
        var random = new System.Random(0);
        var coordArrayList = new List<CoordArrays>();
        var connectionArrayList = new List<CoordArrays>();
        var tensorNeuronBlockIndex = new Dictionary<string, int>();
        var dimensions = new Dictionary<string, int[]>();
        foreach (var result in results) {
            var t0 = result.AsTensor<float>();
            if (t0 == null) {
                continue;
            }

            tensorNeuronBlockIndex[result.Name] = coordArrayList.Count;
            dimensions[result.Name] = t0.Dimensions.ToArray();

            var t = t0.ToDenseTensor();

            var coordArrays = new CoordArrays(2 * t0.Length);

            var indices = new int[t.Rank];
            var layerNum = info.LayerNums[result.Name];
            var tensorPos = TensorPosition(info.PlaceInLayer[result.Name]);

            for (var i = 0; i < t.Length; i++) {
                var position = Position(layerNum, indices, tensorPos);
                var activation = t[indices];
                var color = Color(activation);

                coordArrays.Positions[2*i] = position;
                coordArrays.Colors[2*i] = color;

                var position2 = new Vector3(position.x, position.y + LITTLE_LINE_LENGTH, position.z);

                coordArrays.Positions[2*i+1] = position2;
                coordArrays.Colors[2*i+1] = color;

                indices[0]++;
                for (var j = 0; j < t.Rank - 1; j++) {
                    if (indices[j] < t.Dimensions[j]) {
                        break;
                    }
                    indices[j] = 0;
                    indices[j + 1]++;
                }
            }

            coordArrayList.Add(coordArrays);

            // Now deal with connections
            if (info.OpTypes.ContainsKey(result.Name)) {
                var optype = info.OpTypes[result.Name];

                // Local function to factor out connectionArrays logic
                void AddElementwiseConnectionArrays(string input) {
                    if (tensorNeuronBlockIndex.ContainsKey(input)) {
                        var inputIndex = tensorNeuronBlockIndex[input];
                        var inputDims = dimensions[input];
                        var outputDims = dimensions[result.Name];

                        // Display an error and skip if the tensors are different shapes
                        if (!inputDims.SequenceEqual(outputDims)) {
                            Debug.LogError($"Elementwise: Tensor {result.Name} has shape {string.Join(",",outputDims)} but tensor {input} has shape {string.Join(",",inputDims)}");
                            return;
                        }

                        var connectionArrays = new CoordArrays(2 * t.Length);
                        for (var i = 0; i < t.Length; i++) {
                            var position0 = coordArrayList[inputIndex].Positions[2*i];
                            var position1 = coordArrays.Positions[2*i];
                            var color0 = new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
                            var color1 = new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

                            connectionArrays.Positions[2*i] = position0;
                            connectionArrays.Positions[2*i+1] = position1;
                            connectionArrays.Colors[2*i] = color0;
                            connectionArrays.Colors[2*i+1] = color1;

                            indices[0]++;
                            for (var j = 0; j < t.Rank - 1; j++) {
                                if (indices[j] < t.Dimensions[j]) {
                                    break;
                                }
                                indices[j] = 0;
                                indices[j + 1]++;
                            }
                        }
                        connectionArrayList.Add(connectionArrays);
                    } else {
                        Debug.Log($"No tensorNeuronBlockIndex entry for {input}. It's probably an input layer, parameter or constant");
                    }
                }

                if (optype == "Add") {
                    if (info.OpInputs[result.Name].Length == 2) {
                        var input0 = info.OpInputs[result.Name][0];
                        var input1 = info.OpInputs[result.Name][1];
                        AddElementwiseConnectionArrays(input0);
                        AddElementwiseConnectionArrays(input1);
                    } else {
                        Debug.LogError($"Unexpected number of inputs for Add: {info.OpInputs[result.Name].Length}");
                    }
                } else {
                    Debug.LogWarning($"Skipping connections for {result.Name} {optype} from {string.Join(", ", info.OpInputs[result.Name])}");
                }
            }
        }
        return (coordArrayList, connectionArrayList);
    }
}