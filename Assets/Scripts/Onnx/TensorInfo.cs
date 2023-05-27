using Onnx;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using UnityEngine;

public class TensorInfo {
    long[] d;
    int rank;
    bool is_input;
    int layer;

    public static TensorInfo fromValueInfoProto(ValueInfoProto value, bool is_input) {
        var result = new TensorInfo();
        var dims = value.Type.TensorType.Shape.Dim;

        result.is_input = is_input;
        result.d = new long[dims.Count];
        result.rank = dims.Count;
        result.layer = 0;
        for (int i = 0; i < dims.Count; i++) {
            result.d[i] = dims[i].DimValue;
        }

        return result;
    }

    public static TensorInfo fromTensorProto(TensorProto tensor) {
        var result = new TensorInfo();
        var dims = tensor.Dims;

        result.is_input = false;
        result.d = new long[dims.Count];
        result.rank = dims.Count;
        result.layer = 0;
        for (int i = 0; i < dims.Count; i++) {
            result.d[i] = dims[i];
        }

        return result;
    }

    static long[] repeatedLongToArray(RepeatedField<long> field) {
        var result = new long[field.Count];
        for (int i = 0; i < field.Count; i++) {
            result[i] = field[i];
        }
        return result;
    }

    public static TensorInfo fromOperatorOutput(NodeProto node, int output_index, Dictionary<string, TensorInfo> tensors) {
        if (node.OpType == "Conv" && output_index == 0) {
            return fromConv(node, tensors);
        }
        Debug.Log($"Unhandled operator type: {node.OpType}");
        return null;
    }

    public static TensorInfo fromConv(NodeProto node, Dictionary<string, TensorInfo> tensors) {
        Debug.Log("Processing conv");
        var result = new TensorInfo();
        var x = tensors[node.Input[0]];
        var w = tensors[node.Input[1]];
        TensorInfo b = null;
        if (node.Input.Count >= 2) {
            b = tensors[node.Input[2]];
        }
        var n = x.d[0];
        var c = x.d[1];
        var input_dims = new long[]{x.d[2], x.d[3]};

        long group = 1;
        long[] kernel_shape = null;
        long[] pads = new long[]{0,0,0,0};
        long[] strides = new long[]{1,1};
        long[] dilations = new long[]{1,1};
        long[] output_dims = new long[]{0,0};

        foreach (var attribute in node.Attribute) {
            if (attribute.Name == "group") {
                group = attribute.I;
            } else if (attribute.Name == "kernel_shape") {
                kernel_shape = repeatedLongToArray(attribute.Ints);
            } else if (attribute.Name == "pads") {
                pads = repeatedLongToArray(attribute.Ints);
            } else if (attribute.Name == "strides") {
                strides = repeatedLongToArray(attribute.Ints);
            } else {
                Debug.Log($"Unhandled Conv Attr: {attribute.Name} {attribute.Type}");
            }
        }

        for (int d = 0; d < 2; d++) {
            output_dims[d] = (input_dims[d] - kernel_shape[d] + pads[d] + pads[2+d]) / strides[d] + 1;
        }

        Debug.Log($"Group: {group}, kernel_shape: {string.Join(",", kernel_shape)}, pads: {string.Join(",", pads)}, strides: {string.Join(",", strides)}");
        Debug.Log($"Input dims: {string.Join(",", input_dims)}. Output dims: {string.Join(",", output_dims)}");
        return result;
    }
}