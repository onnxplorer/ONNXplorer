using Onnx;

public class TensorInfo {
    long[] d;
    int rank;

    static TensorInfo fromValueInfoProto(ValueInfoProto value) {
        var result = new TensorInfo();
        var dims = value.Type.TensorType.Shape.Dim;
        return result;
    }
}