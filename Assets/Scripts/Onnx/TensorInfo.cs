using Onnx;
using System.Collections.Generic;
using System;
using Google.Protobuf.Collections;
using UnityEngine;

public class TensorInfo {
    long[] d;
    bool is_input;
    bool is_constant;
    int layer;

    public int Rank {
        get { return d.Length; }
    }

    public static TensorInfo fromValueInfoProto(ValueInfoProto value, bool is_input, Dictionary<string, long> dim_params) {
        var result = new TensorInfo();
        var dims = value.Type.TensorType.Shape.Dim;

        result.is_input = is_input;
        result.d = new long[dims.Count];
        result.layer = 0;
        for (int i = 0; i < dims.Count; i++) {
            if (dim_params.ContainsKey(dims[i].DimParam)) {
                result.d[i] = dim_params[dims[i].DimParam];
            } else {
                result.d[i] = dims[i].DimValue;
            }
        }

        return result;
    }

    public static TensorInfo fromTensorProto(TensorProto tensor) {
        var result = new TensorInfo();
        var dims = tensor.Dims;

        result.is_input = false;
        result.is_constant = true;
        result.d = new long[dims.Count];
        result.layer = 0;
        for (int i = 0; i < dims.Count; i++) {
            result.d[i] = dims[i];
        }

        return result;
    }

    public static long[] repeatedLongToArray(RepeatedField<long> field) {
        var result = new long[field.Count];
        for (int i = 0; i < field.Count; i++) {
            result[i] = field[i];
        }
        return result;
    }

    public static TensorInfo fromOperatorOutput(NodeProto node, int output_index, Dictionary<string, TensorInfo> tensors) {
        var bag = new AttrBag(node.Attribute);
        TensorInfo result = null;
        if (node.OpType == "Add" && output_index == 0) {
            result = fromAdd(node, tensors);
        } else if (node.OpType == "Clip" && output_index == 0) {
            result = fromClip(node, tensors, bag.PullFloat("max", 3.402823e+38d), bag.PullFloat("min", -3.402823e+38d));
        } else if (node.OpType == "Constant" && output_index == 0) {
            result = fromConstant(node, tensors);
            bag.Clear();
        } else if (node.OpType == "Conv" && output_index == 0) {
            result = fromConv(
                node,
                tensors,
                bag.PullInt("group", 1),
                bag.PullRequiredInts("kernel_shape"),
                bag.PullInts("pads", new long[]{0,0,0,0}),
                bag.PullInts("strides", new long[]{1,1}),
                bag.PullInts("dilations", new long[]{0,0})
            );
        } else if (node.OpType == "GlobalAveragePool" && output_index == 0) {
            result = fromGlobalAveragePool(node, tensors);
        } else if (node.OpType == "Shape" && output_index == 0) {
            result = fromShape(node, tensors);
        } else {
            Debug.LogError($"Unhandled operator type: {node.OpType}");
        }
        var input_string = "";
        var layer = 0;
        foreach (var input in node.Input) {
            layer = Math.Max(layer, tensors[input].layer + 1);
            input_string += $"[{string.Join(",", tensors[input].d)}] ";
        }
        result.layer = layer;
        Debug.Log($"{node.OpType} Input dims: {input_string}. Output dims: {string.Join(",", result.d)}. Layer {result.layer}");
        bag.GripeIfNonempty(node.OpType);
        return result;
    }

    public static TensorInfo fromConv(NodeProto node, Dictionary<string, TensorInfo> tensors, long group, long[] kernel_shape, long[] pads, long[] strides, long[] dilations) {
        Debug.Log("Processing conv");
        var result = new TensorInfo();
        var x = tensors[node.Input[0]];
        var w = tensors[node.Input[1]];
        TensorInfo b = null;
        if (node.Input.Count > 2) {
            b = tensors[node.Input[2]];
        }
        var n = x.d[0];
        var c = x.d[1];
        var input_dims = new long[]{x.d[2], x.d[3]};
        var output_c = w.d[0];
        var output_dims = new long[]{0,0};

        for (int d = 0; d < 2; d++) {
            output_dims[d] = (input_dims[d] - kernel_shape[d] + pads[d] + pads[2+d]) / strides[d] + 1;
        }

        if (kernel_shape[0] != w.d[2] || kernel_shape[1] != w.d[3]) {
            Debug.LogError($"Unexpected kernel size: {string.Join(",", kernel_shape)} vs {string.Join(",", w.d)}");
        }

        Debug.Log($"Group: {group}, kernel_shape: {string.Join(",", kernel_shape)}, pads: {string.Join(",", pads)}, strides: {string.Join(",", strides)}");
        result.d = new long[]{n, output_c, output_dims[0], output_dims[1]};
        return result;
    }

    public static TensorInfo fromClip(NodeProto node, Dictionary<string, TensorInfo> tensors, double max, double min) {
        Debug.Log("Processing clip");
        var result = new TensorInfo();
        var input = tensors[node.Input[0]];
        /*TensorInfo min = null;
        if (node.Input.Count > 1) {
            min = tensors[node.Input[1]];
            if (min.Rank != 0) {
                Debug.LogError($"Bad rank for min: {min.Rank}");
            }
        }
        TensorInfo max = null;
        if (node.Input.Count > 2) {
            max = tensors[node.Input[2]];
            if (max.Rank != 0) {
                Debug.LogError($"Bad rank for max: {max.Rank}");
            }
        }*/

        result.d = input.d;
        return result;
    }

    private static (TensorInfo,TensorInfo) broadcast(TensorInfo a, TensorInfo b) {
        return (a,b);
    }

    private static TensorInfo fromAdd(NodeProto node, Dictionary<string, TensorInfo> tensors) {
        Debug.Log("Processing add");
        var result = new TensorInfo();
        var (a,b) = broadcast(tensors[node.Input[0]], tensors[node.Input[1]]);
        result.d = a.d;
        return result;
    }

    private static TensorInfo fromGlobalAveragePool(NodeProto node, Dictionary<string, TensorInfo> tensors) {
        Debug.Log("Processing GlobalAveragePool");
        var result = new TensorInfo();
        var input = tensors[node.Input[0]];
        if (input.Rank != 4) {
            Debug.LogError($"Expecting input rank 4");
        }
        result.d = new long[]{input.d[0], input.d[1], 1, 1};
        return result;
    }

    private static TensorInfo fromShape(NodeProto node, Dictionary<string, TensorInfo> tensors) {
        Debug.Log("Processing Shape");
        var result = new TensorInfo();
        var input = tensors[node.Input[0]];
        result.is_constant = true;
        result.d = new long[]{input.Rank};
        return result;
    }

    private static TensorInfo fromConstant(NodeProto node, Dictionary<string, TensorInfo> tensors) {
        Debug.Log("Processing Constant");
        foreach (var attribute in node.Attribute) {
            if (attribute.Name == "value") {
                return fromTensorProto(attribute.T);
            } else if (attribute.Name == "sparse_value") {
                Debug.LogError("Sparse values unhandled");
            } else {
                var result = new TensorInfo();
                result.is_constant = true;
                result.d = new long[]{};
                return result;
            }
        }
        Debug.LogError("No constant returned");
        return null;
    }
}