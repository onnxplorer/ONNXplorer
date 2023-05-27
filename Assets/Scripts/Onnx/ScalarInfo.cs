using Onnx;
using UnityEngine;

public class ScalarInfo {
    bool isConstInt;
    long constInt;
    double constFloat;

    public long AsConstInt() {
        if (!isConstInt) {
            Debug.LogError("Not actually a const int");
        }
        return constInt;
    }

    public static ScalarInfo FromInt(long n) {
        ScalarInfo result = new ScalarInfo();
        result.isConstInt = true;
        result.constInt = n;
        return result;
    }

    public static ScalarInfo FromFloat(double f) {
        ScalarInfo result = new ScalarInfo();
        result.constFloat = f;
        return result;
    }

    public static ScalarInfo FromTensorProto(TensorProto tensor, long index, byte[] rawData) {
        if (tensor.DataType == (int)TensorProto.Types.DataType.Int32) {
            if (index > tensor.Int32Data.Count) {
                //Debug.LogError("int32 index too big");
            }
            return FromInt(tensor.Int32Data[(int)index]);
        } else if (tensor.DataType == (int)TensorProto.Types.DataType.Float) {
            return FromFloat(System.BitConverter.ToSingle(rawData, (int)(4 * index)));
        }
        throw new System.Exception($"Cannot process data type {tensor.DataType}");
    }
}