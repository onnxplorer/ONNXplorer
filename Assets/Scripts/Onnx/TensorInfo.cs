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
    ScalarInfo[,,,] scalars;

    public int Rank {
        get { return d.Length; }
    }

    public long Count {
        get { return Product(d); }
    }

    public long[] AsConstIntVector() {
        if (Rank != 1) {
            Debug.LogError("Rank must be 1 in AsConstIntVector");
        }
        long[] result = new long[d[0]];
        for (var i = 0; i < d[0]; i++) {
            result[i] = scalars[i,0,0,0].AsConstInt();
        }
        return result;
    }

    public static long Product(long[] values) {
        long result = 1;
        foreach (var value in values) {
            result *= value;
        }
        return result;
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

        if (dims.Count > 4) {
            Debug.LogError("Can't create constant with rank > 4");
        }

        long xmax = 1;
        long ymax = 1;
        long zmax = 1;
        long wmax = 1;
        if (dims.Count > 0) {
            xmax = dims[0];
        }
        if (dims.Count > 1) {
            ymax = dims[1];
        }
        if (dims.Count > 2) {
            zmax = dims[2];
        }
        if (dims.Count > 3) {
            wmax = dims[3];
        }
        if (tensor.Segment != null) {
            Debug.Log($"Segment {tensor.Segment.Begin} {tensor.Segment.End}");
        }
        Debug.Log($"Tensor {tensor.Name} {tensor.DataType} {xmax} {ymax} {zmax} {wmax} {tensor.RawData.Length}");
        result.scalars = new ScalarInfo[xmax,ymax,zmax,wmax];
        var rawData = tensor.RawData.ToByteArray();
        for (var x = 0; x < xmax; x++ ) {
            for (var y = 0; y < ymax; y++) {
                for (var z = 0; z < zmax; z++) {
                    for (var w = 0; w < wmax; w++) {
                        var index = (((x * ymax + y) * zmax + z) * wmax + w);
                        var scalar = ScalarInfo.FromTensorProto(tensor, index, rawData);
                        result.scalars[x,y,z,w] = scalar;
                    }
                }
            }
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
        if (output_index == 0) {
            if (node.OpType == "Add") {
                result = fromAdd(node, tensors);
            } else if (node.OpType == "Clip") {
                result = fromClip(node, tensors, bag.PullFloat("max", 3.402823e+38d), bag.PullFloat("min", -3.402823e+38d));
            } else if (node.OpType == "Concat") {
                result = fromConcat(node, tensors, bag.PullInt("axis", 0));
            } else if (node.OpType == "Constant") {
                result = fromConstant(node, tensors);
                bag.Clear();
            } else if (node.OpType == "Conv") {
                result = fromConv(
                    node,
                    tensors,
                    bag.PullInt("group", 1),
                    bag.PullRequiredInts("kernel_shape"),
                    bag.PullInts("pads", new long[]{0,0,0,0}),
                    bag.PullInts("strides", new long[]{1,1}),
                    bag.PullInts("dilations", new long[]{0,0})
                );
            } else if (node.OpType == "Gather") {
                result = fromGather(node, tensors, bag.PullInt("axis", 0));
            } else if (node.OpType == "GlobalAveragePool") {
                result = fromGlobalAveragePool(node, tensors);
            } else if (node.OpType == "Reshape") {
                result = fromReshape(node, tensors);
            } else if (node.OpType == "Shape") {
                result = fromShape(node, tensors);
            } else if (node.OpType == "Unsqueeze") {
                result = fromUnsqueeze(node, tensors, bag.PullRequiredInts("axes"));
            }
        }

        if (result == null) {
            Debug.LogError($"Unhandled operator type: {node.OpType} {output_index}");
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
        result.scalars = new ScalarInfo[input.Rank, 1, 1, 1];
        for (var i = 0; i < input.Rank; i++) {
            result.scalars[i,0,0,0] = ScalarInfo.FromInt(input.d[i]);
        }
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

    private static TensorInfo fromGather(NodeProto node, Dictionary<string, TensorInfo> tensors, long axis) {
        Debug.Log("Processing Gather");
        var data = tensors[node.Input[0]];
        if (data.Rank == 0) {
            Debug.LogError("Can't Gather into something of rank 0");
        }
        if (axis < 0 || axis >= data.Rank) {
            Debug.LogError("Gather: Axis out of range");
        }
        var indices = tensors[node.Input[1]];
        var d = new long[indices.Rank + data.Rank - 1];
        var j = 0;
        for (var i = 0; i < indices.Rank; i++) {
            d[j] = indices.d[i];
            j++;
        }
        for (var i = 0; i < data.Rank; i++) {
            if (i != axis) {
                d[j] = data.d[j];
                j++;
            }
        }
        if (j != d.Length) {
            Debug.LogError("Not supposed to happen");
        }
        var result = new TensorInfo();
        result.d = d;
        return result;
    }

    private static TensorInfo fromUnsqueeze(NodeProto node, Dictionary<string, TensorInfo> tensors, long[] axes) {
        Debug.Log("Processing Unsqueeze");
        var data = tensors[node.Input[0]];
        var d = new long[data.Rank + axes.Length];
        var j = 0;
        for (var i = 0; i < d.Length; i++) {
            if (Array.IndexOf(axes, (long)i) != -1) {
                d[i] = 1;
            } else {
                d[i] = data.d[j];
                j++;
            }
        }
        if (j != data.Rank) {
            Debug.LogError("Some kind of dimension mismatch in Unsqueeze");
        }
        var result = new TensorInfo();
        result.d = d;
        return result;
    }

    private static TensorInfo fromConcat(NodeProto node, Dictionary<string, TensorInfo> tensors, long axis) {
        Debug.Log("Processing Gather");
        
        var rank = tensors[node.Input[0]].Rank;
        if (axis < 0 || axis >= rank) {
            Debug.LogError("Concat: Axis out of range");
        }
        var d = new long[rank];
        for (var i = 0; i < rank; i++) {
            for (var j = 0; j < node.Input.Count; j++) {
                if (tensors[node.Input[j]].Rank != rank) {
                    Debug.LogError("Concat: rank mismatch");
                }
                var dim = tensors[node.Input[j]].d[i];
                if (i == axis) {
                    d[i] += dim;
                } else if (i == 0) {
                    d[i] = dim;
                } else {
                    if (d[i] != dim) {
                        Debug.LogError("Concat: dimension mismatch");
                    }
                }
            }
        }
        var result = new TensorInfo();
        result.d = d;
        return result;
    }

    private static TensorInfo fromReshape(NodeProto node, Dictionary<string, TensorInfo> tensors) {
        var data = tensors[node.Input[0]];
        var shape = tensors[node.Input[1]].AsConstIntVector();
        if (data.Count != Product(shape)) {
            Debug.LogError("Reshape: count mismatch");
        }
        var result = new TensorInfo();
        result.d = shape;
        return result;
    }
}