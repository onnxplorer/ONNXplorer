using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;

public class Layout {
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

    public static List<CoordArrays> GetCoordArrays(UsefulModelInfo info, IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
    {
        var coordArrayList = new List<CoordArrays>();
        foreach (var result in results) {
            var t0 = result.AsTensor<float>();
            if (t0 == null) {
                continue;
            }

            var t = t0.ToDenseTensor();

            var coordArrays = new CoordArrays();
            coordArrays.Positions = new Vector3[t0.Length];
            coordArrays.Colors = new Color[t0.Length];

            var indices = new int[t.Rank];
            var layerNum = info.LayerNums[result.Name];
            var tensorPos = TensorPosition(info.PlaceInLayer[result.Name]);

            for (var i = 0; i < t.Length; i++) {
                var position = Position(layerNum, indices, tensorPos);
                var activation = t[indices];

                coordArrays.Positions[i] = position;
                coordArrays.Colors[i] = Color(activation);

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
        }
        return coordArrayList;
    }
}