using Onnx;
using System.Collections.Generic;
using System;
using System.Linq;
using Google.Protobuf.Collections;
using UnityEngine;

public class TensorInfo {
    string op_name;
    string tensor_name;
    long[] d;
    bool is_input;
    bool is_constant;
    int layer;
    ScalarInfo[,,,] scalars;
    List<ScalarInfo> hiddenScalars;

    public int Rank {
        get { return d.Length; }
    }

    public long Count {
        get { return Product(d); }
    }

    public List<ScalarInfo> GetAllScalars() { //NEXT Optionize include hidden?
        List<ScalarInfo> result = new List<ScalarInfo>();
        if (scalars != null) {
            foreach (var s in scalars) {
                result.Add(s);
            }
        }
        if (hiddenScalars != null) {
            foreach (var s in hiddenScalars) {
                result.Add(s);
            }
        }
        return result;
    }

    public int GetDim(int i) {
        if (i < d.Length) {
            return (int)d[i];
        } else {
            return 1;
        }
    }

    public static TensorInfo ZerosFloat(long[] d) {
        var result = new TensorInfo();
        result.d = d;
        result.scalars = CreateScalars(d);
        result.is_constant = true;
        for (var x = 0; x < d[0]; x++) {
            for (var y = 0; y < d[1]; y++) {
                for (var z = 0; z < d[2]; z++) {
                    for (var w = 0; w < d[3]; w++) {
                        result.scalars[x, y, z, w] = ScalarInfo.FromFloat(0);
                    }
                }
            }
        }
        return result;
    }

    public TensorInfo MatrixTranspose() {
        if (Rank != 2) {
            throw new Exception("Rank must be 2 for MatrixTranspose");
        }
        TensorInfo result = new TensorInfo();
        result.d = new long[]{d[1], d[0]};
        result.scalars = new ScalarInfo[d[1], d[0], 1, 1];
        for (var x = 0; x < d[1]; x++) {
            for (var y = 0; y < d[0]; y++) {
                result.scalars[x, y, 0, 0] = scalars[y, x, 0, 0];
            }
        }
        result.is_constant = is_constant;
        return result;
    }

    static ScalarInfo[,,,] CreateScalars(long[] d) {
        if (d.Length == 0) {
            return new ScalarInfo[1,1,1,1];
        } else if (d.Length == 1) {
            return new ScalarInfo[d[0],1,1,1];
        } else if (d.Length == 2) {
            return new ScalarInfo[d[0],d[1],1,1];
        } else if (d.Length == 3) {
            return new ScalarInfo[d[0],d[1],d[2],1];
        } else if (d.Length == 4) {
            return new ScalarInfo[d[0],d[1],d[2],d[3]];
        } else {
            throw new System.Exception($"Cannot create scalars for rank {d.Length}");
        }
    }

    public long[] AsConstIntVector() {
        if (Rank != 1) {
            Debug.LogError("Rank must be 1 in AsConstIntVector");
        }
        if (!is_constant) {
            throw new System.Exception($"{op_name} {tensor_name} Not a constant");
        }
        if (scalars == null) {
            throw new System.Exception("No scalars");
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

    public static TensorInfo FromInput(ValueInfoProto value, Dictionary<string, long> dim_params, System.Random random = null) {
        var result = new TensorInfo();
        var dims = value.Type.TensorType.Shape.Dim;

        result.is_input = true;
        result.d = new long[dims.Count];
        result.layer = 0;
        result.tensor_name = value.Name;
        result.op_name = "[input]";
        for (int i = 0; i < dims.Count; i++) {
            if (dim_params.ContainsKey(dims[i].DimParam)) {
                result.d[i] = dim_params[dims[i].DimParam];
            } else {
                result.d[i] = dims[i].DimValue;
            }
        }

        //CHECK Are extra dimensions (>4) problematic?  Probably, if they happen
        result.scalars = CreateScalars(result.d);
        for (var x = 0; x < result.GetDim(0); x++) {
            for (var y = 0; y < result.GetDim(1); y++) {
                for (var z = 0; z < result.GetDim(2); z++) {
                    for (var w = 0; w < result.GetDim(3); w++) {
                        result.scalars[x,y,z,w] = ScalarInfo.InputActivation(result.layer, new[] { x, y, z, w }, random);
                    }
                }
            }
        }

        return result;
    }

    public static TensorInfo FromTensorProto(TensorProto tensor) {
        var result = new TensorInfo();
        var dims = tensor.Dims;

        result.is_input = false;
        result.is_constant = true;
        result.d = new long[dims.Count];
        result.layer = 0;
        result.tensor_name = tensor.Name;
        result.op_name = "[constant]";
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
            throw new System.Exception("Segment not supported");
        }
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

    public static TensorInfo FromOperatorOutput(NodeProto node, int output_index, Dictionary<string, TensorInfo> tensors, System.Random random) {
        var bag = new AttrBag(node.Attribute);
        TensorInfo result = null;

        var input_string = "";
        var layer = 0;
        foreach (var input in node.Input) {
            layer = Math.Max(layer, tensors[input].layer + 1);
            input_string += $"[{string.Join(",", tensors[input].d)}] ";
        }

        if (output_index == 0) {
            if (node.OpType == "Add") {
                result = fromAdd(node, tensors);
            } else if (node.OpType == "Clip") {
                result = FromClip(node, tensors, bag.PullFloat("max", 3.402823e+38d), bag.PullFloat("min", -3.402823e+38d), layer, random);
            } else if (node.OpType == "Concat") {
                result = fromConcat(node, tensors, bag.PullInt("axis", 0));
            } else if (node.OpType == "Constant") {
                result = fromConstant(node, tensors);
                bag.Clear();
            } else if (node.OpType == "Conv") {
                result = FromConv(
                    node,
                    tensors,
                    bag.PullInt("group", 1),
                    bag.PullRequiredInts("kernel_shape"),
                    bag.PullInts("pads", new long[] { 0, 0, 0, 0 }),
                    bag.PullInts("strides", new long[] { 1, 1 }),
                    bag.PullInts("dilations", new long[] { 0, 0 }),
                    random,
                    layer
                );
            } else if (node.OpType == "Gather") {
                result = fromGather(node, tensors, bag.PullInt("axis", 0));
            } else if (node.OpType == "Gemm") {
                result = FromGemm(
                    node,
                    tensors,
                    bag.PullFloat("alpha", 1.0),
                    bag.PullFloat("beta", 1.0),
                    bag.PullInt("transA", 0),
                    bag.PullInt("transB", 0)
                );
            } else if (node.OpType == "GlobalAveragePool") {
                result = fromGlobalAveragePool(node, tensors);
            } else if (node.OpType == "Reshape") {
                result = fromReshape(node, tensors);
            } else if (node.OpType == "Shape") {
                result = fromShape(node, tensors);
            } else if (node.OpType == "Unsqueeze") {
                result = fromUnsqueeze(node, tensors, bag.PullRequiredInts("axes"));
            } else if (node.OpType == "Sigmoid") {
                result = FromSigmoid(node, tensors, layer, random);
            }
        }

        if (result == null) {
            Debug.LogError($"Unhandled operator type: {node.OpType} {output_index}");
            return null;
        }
        result.layer = layer;
        result.tensor_name = node.Output[output_index];
        result.op_name = node.Name;
        Debug.Log($"{node.OpType} Input dims: {input_string}. Output dims: {string.Join(",", result.d)}. Layer {result.layer}. Scalars {result.scalars != null}. Constant {result.is_constant}");
        bag.GripeIfNonempty(node.OpType);
        return result;
    }

    public static TensorInfo FromConv(NodeProto node, Dictionary<string, TensorInfo> tensors, long group, long[] kernel_shape, long[] pads, long[] strides, long[] dilations, System.Random random, int layer) {
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

        if (output_c != w.d[0]) {
            Debug.LogError($"Unexpected output channel count: {output_c} vs {w.d[0]}");
        }
        if (c != w.d[1] * group) {
            Debug.LogError($"Unexpected input channel count: {c} vs {w.d[1]}, group = {group}");
        }

        result.d = new long[]{n, output_c, output_dims[0], output_dims[1]};
        Debug.Log($"Group: {group}, kernel_shape: {string.Join(",", kernel_shape)}, pads: {string.Join(",", pads)}, strides: {string.Join(",", strides)}");

        var opcount = 0;
        var hidden = new List<ScalarInfo>();
        if (x.scalars != null && w.scalars != null) {
            result.scalars = new ScalarInfo[result.d[0], result.d[1], result.d[2], result.d[3]];
            for (int instance = 0; instance < n; instance++) {
                for (int output_channel = 0; output_channel < output_c; output_channel++) {
                    var g = output_channel / (w.d[0] / group);
                    for (int output_y = 0; output_y < output_dims[0]; output_y++) {
                        for (int output_x = 0; output_x < output_dims[1]; output_x++) {
                            var list_to_sum = new List<ScalarInfo>();
                            for (int input_channel = 0; input_channel < w.d[1]; input_channel++) {
                                for (int kernel_y = 0; kernel_y < kernel_shape[0]; kernel_y++) {
                                    for (int kernel_x = 0; kernel_x < kernel_shape[1]; kernel_x++) {
                                        var input_y = output_y * strides[0] + kernel_y - pads[0];
                                        var input_x = output_x * strides[1] + kernel_x - pads[2];
                                        if (input_y >= 0 && input_y < input_dims[0] && input_x >= 0 && input_x < input_dims[1]) {
                                            var xscal = x.scalars[instance, g * w.d[1] + input_channel, input_y, input_x];
                                            var wscal = w.scalars[output_channel, input_channel, kernel_y, kernel_x];
                                            //CHECK I didn't really think too hard about these layerPosition values, not sure if they make sense
                                            var product = ScalarInfo.MulFloats(layer, new[] { instance, output_channel, output_y, output_x, input_channel, kernel_y, kernel_x }, random, xscal, wscal);
                                            hidden.Add(product);
                                            list_to_sum.Add(product);
                                            opcount += 1;
                                            if (opcount % 1000000 == 0) {
                                                Debug.Log($"Opcount: {opcount}");
                                            }
                                        }
                                    }
                                }
                            }
                            if (b != null) {
                                list_to_sum.Add(b.scalars[output_channel,0,0,0]);
                            }
                            var sum = ScalarInfo.SumFloats(layer, new[] { instance, output_channel, output_y, output_x }, random, list_to_sum);
                            result.scalars[instance, output_channel, output_y, output_x] = sum;
                        }
                    }
                }
            }
        }
        result.hiddenScalars = hidden;

        return result;
    }

    public static TensorInfo FromClip(NodeProto node, Dictionary<string, TensorInfo> tensors, double max, double min, int layer, System.Random random) {
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

        if (input.scalars != null) {
            result.scalars = CreateScalars(result.d);
            for (var x = 0; x < result.GetDim(0); x++) {
                for (var y = 0; y < result.GetDim(1); y++) {
                    for (var z = 0; z < result.GetDim(2); z++) {
                        for (var w = 0; w < result.GetDim(3); w++) {
                            result.scalars[x,y,z,w] = ScalarInfo.ClipFloat(layer, new[] { x, y, z, w }, random, input.scalars[x,y,z,w]);
                        }
                    }
                }
            }
        }

        return result;
    }

    public static TensorInfo FromSigmoid(NodeProto node, Dictionary<string, TensorInfo> tensors, int layer, System.Random random) {
        Debug.Log("Processing sigmoid");
        var result = new TensorInfo();
        var input = tensors[node.Input[0]];

        result.d = input.d;

        if (input.scalars != null) {
            result.scalars = CreateScalars(result.d);
            for (var x = 0; x < result.GetDim(0); x++) {
                for (var y = 0; y < result.GetDim(1); y++) {
                    for (var z = 0; z < result.GetDim(2); z++) {
                        for (var w = 0; w < result.GetDim(3); w++) {
                            //RAINY It bugs me that it just appends "1" for dimensions we don't have...and it kinda messes up the rendering.  ...So, the fix I came up with, hopefully it's not too heavy for this loop.
                            //THINK Consider applying this logic to other places
                            result.scalars[x, y, z, w] = ScalarInfo.SigmoidFloat(layer, (new[] { x, y, z, w }).Take(result.Rank).ToArray(), random, input.scalars[x, y, z, w]);
                        }
                    }
                }
            }
        }

        return result;
    }

    private static (TensorInfo,TensorInfo) Broadcast(TensorInfo a, TensorInfo b) {
        if (!a.d.SequenceEqual(b.d)) {
            throw new NotImplementedException($"Broadcasting not implemented between {string.Join(",", a.d)} and {string.Join(",", b.d)}");
        }
        return (a,b);
    }

    private TensorInfo BroadcastTo(long[] dims) {
        if (dims.SequenceEqual(d)) {
            return this;
        } else if (this.Rank == 1 && dims.Length == 2 && dims[1] == d[0]) {
            var result = new TensorInfo();
            result.d = dims;
            result.scalars = new ScalarInfo[dims[0], dims[1], 1, 1];
            for (var x = 0; x < dims[0]; x++) {
                for (var y = 0; y < dims[1]; y++) {
                    result.scalars[x,y,0,0] = scalars[y,0,0,0];
                }
            }
            result.is_constant = is_constant;
            return result;
        } else {
            throw new NotImplementedException($"Broadcasting not implemented for {string.Join(",", d)} to {string.Join(",", dims)}");
        }
    }

    private static TensorInfo fromAdd(NodeProto node, Dictionary<string, TensorInfo> tensors) {
        Debug.Log("Processing add");
        var result = new TensorInfo();
        var (a,b) = Broadcast(tensors[node.Input[0]], tensors[node.Input[1]]);
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
                return FromTensorProto(attribute.T);
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

        if (data.is_constant && indices.is_constant) {
            var scalars = CreateScalars(d);
            for (var x = 0; x < scalars.GetLength(0); x++) {
                for (var y = 0; y < scalars.GetLength(1); y++) {
                    for (var z = 0; z < scalars.GetLength(2); z++) {
                        for (var w = 0; w < scalars.GetLength(3); w++) {
                            var index_ix = new long[]{x,y,z,w};
                            for (var i = indices.Rank; i < 4; i++) {
                                index_ix[i] = 0;
                            }
                            var index_value = indices.scalars[index_ix[0],index_ix[1],index_ix[2],index_ix[3]].AsConstInt();
                            var data_ix = new long[4];
                            j = indices.Rank;
                            for (var i = 0; i < data.Rank; i++) {
                                if (i == axis) {
                                    data_ix[i] = index_value;
                                } else {
                                    data_ix[i] = data.d[j];
                                    j++;
                                }
                            }
                            scalars[x,y,z,w] = data.scalars[data_ix[0],data_ix[1],data_ix[2],data_ix[3]];
                        }
                    }
                }
            }
            result.scalars = scalars;
            result.is_constant = true;
        }

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
        if (data.Rank > 4) {
            throw new NotImplementedException("Unsqueeze for rank > 4");
        }
        var result = new TensorInfo();
        result.d = d;

        if (data.is_constant) {
            var scalars = CreateScalars(result.d);
            for (var x = 0; x < data.GetDim(0); x++) {
                for (var y = 0; y < data.GetDim(1); y++) {
                    for (var z = 0; z < data.GetDim(2); z++) {
                        for (var w = 0; w < data.GetDim(3); w++) {
                            var src = new long[]{x,y,z,w};
                            var dest = new long[4];
                            for (var i = 0; i < d.Length; i++) {
                                if (Array.IndexOf(axes, (long)i) != -1) {
                                    dest[i] = 0;
                                } else {
                                    dest[i] = src[j];
                                    j++;
                                }
                            }
                            scalars[dest[0], dest[1], dest[2], dest[3]] = data.scalars[x,y,z,w];
                        }
                    }
                }
            }
            result.scalars = scalars;
            result.is_constant = true;
        } else {
            Debug.LogError($"Unsqueeze: non-constant input: {data.op_name}");
        }

        return result;
    }

    private static TensorInfo fromConcat(NodeProto node, Dictionary<string, TensorInfo> tensors, long axis) {
        Debug.Log("Processing Concat");
        
        var rank = tensors[node.Input[0]].Rank;
        if (axis < 0 || axis >= rank) {
            Debug.LogError("Concat: Axis out of range");
        }
        if (rank > 4) {
            Debug.LogError("Concat: rank too high");
        }
        var d = new long[rank];
        var all_constant = true;
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
                all_constant &= tensors[node.Input[j]].is_constant;
                if (!tensors[node.Input[j]].is_constant) {
                    Debug.LogError($"Concat: non-constant input: {tensors[node.Input[j]].op_name}");
                }
            }
        }
        var result = new TensorInfo();
        result.d = d;

        if (all_constant) {
            result.scalars = CreateScalars(d);
            var offsets = new long[4];
            for (var i = 0; i < node.Input.Count; i++) {
                all_constant &= tensors[node.Input[i]].is_constant;
                var input = tensors[node.Input[i]];

                if (input.scalars == null) {
                    throw new Exception($"Concat: input scalars null, constant = {input.is_constant}");
                }

                for (var x = 0; x < input.GetDim(0); x++) {
                    for (var y = 0; y < input.GetDim(1); y++) {
                        for (var z = 0; z < input.GetDim(2); z++) {
                            for (var w = 0; w < input.GetDim(3); w++) {
                                result.scalars[offsets[0] + x,offsets[1] + y,offsets[2] + z,offsets[3] + w] = input.scalars[x,y,z,w];
                            }
                        }
                    }
                }
                offsets[axis] += input.d[axis];
            }
        }
        result.is_constant = all_constant;

        return result;
    }

    private static TensorInfo fromReshape(NodeProto node, Dictionary<string, TensorInfo> tensors) {
        var data = tensors[node.Input[0]];
        var shape = tensors[node.Input[1]].AsConstIntVector();
        // See if any elements of shape are -1
        if (Array.IndexOf(shape, -1) != -1) {
            var product = Product(data.d);
            var index = Array.IndexOf(shape, -1);
            shape[index] = 1;
            shape[index] = Product(data.d) / Product(shape);
        }
        if (data.Count != Product(shape)) {
            throw new Exception($"Reshape: count mismatch product({string.Join(",", shape)}) = product({string.Join(",", data.d)})");
        }
        var result = new TensorInfo();
        result.d = shape;
        return result;
    }

    private static TensorInfo FromGemm(NodeProto node, Dictionary<string, TensorInfo> tensors, double alpha, double beta, long transA, long transB) {
        var a = tensors[node.Input[0]];
        var b = tensors[node.Input[1]];
        if (a.Rank != 2 || b.Rank != 2) {
            throw new Exception($"Rank should be 2 for Gemm, got {a.Rank} {b.Rank}");
        }
        if (transA != 0) {
            a = a.MatrixTranspose();
        }
        if (transB != 0) {
            b = b.MatrixTranspose();
        }

        if (a.d[1] != b.d[0]) {
            throw new Exception("Middle dimension error for Gemm");
        }

        TensorInfo c = null;
        var d = new long[]{a.d[0], b.d[1]};
        if (node.Input.Count > 2) {
            c = tensors[node.Input[2]].BroadcastTo(d);
        } else {
            c = ZerosFloat(d);
        }

        var result = new TensorInfo();
        result.d = d;
        return result;
    }
}