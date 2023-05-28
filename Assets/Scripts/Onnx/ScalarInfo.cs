using System;
using Onnx;
using UnityEngine;

public class ScalarInfo {
    bool isConstInt;
    long constInt;
    double constFloat;
    Neuron neuron;

    public Neuron GetNeuron {
        get {
            return neuron;
        }
    }

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
            return FromInt(tensor.Int32Data[(int)index]);
        } else if (tensor.DataType == (int)TensorProto.Types.DataType.Int64) {
            return FromInt(System.BitConverter.ToInt64(rawData, (int)(8 * index)));
        } else if (tensor.DataType == (int)TensorProto.Types.DataType.Float) {
            return FromFloat(System.BitConverter.ToSingle(rawData, (int)(4 * index)));
        }
        throw new System.Exception($"Cannot process data type {tensor.DataType}");
    }

    public static ScalarInfo Activation(int layer, System.Random random) {
        float x = layer;
        if (layer != 0) {
            Debug.Log("nonzero: " + layer);
        }
        float y = (float)random.NextDouble();
        float z = (float)random.NextDouble();
        float r = (float)random.NextDouble();
        float g = (float)random.NextDouble();
        float b = (float)random.NextDouble();
        var point = new PointRef(new Vector3(x, y, z), new Color(r, g, b));
        var neuron = new Neuron();
        neuron.point = point;
        ScalarInfo result = new ScalarInfo();
        result.neuron = neuron;
        return result;
    }
}