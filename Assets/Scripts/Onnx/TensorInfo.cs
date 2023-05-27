using Onnx;
using System.Collections.Generic;

public class TensorInfo {
    long[] d;
    int rank;
    bool is_input;

    public static TensorInfo fromValueInfoProto(ValueInfoProto value, bool is_input) {
        var result = new TensorInfo();
        var dims = value.Type.TensorType.Shape.Dim;

        result.is_input = is_input;
        result.d = new long[dims.Count];
        result.rank = dims.Count;
        for (int i = 0; i < dims.Count; i++) {
            result.d[i] = dims[i].DimValue;
        }

        return result;
    }

    public static TensorInfo fromOperatorOutput(NodeProto node, int output_index, Dictionary<string, TensorInfo> inputs) {
        var result = new TensorInfo();
        if (node.OpType == "conv" && output_index == 0) {
            
        }
        return result;
    }
}