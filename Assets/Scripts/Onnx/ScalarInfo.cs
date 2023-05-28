using System;
using Onnx;
using UnityEngine;

public class ScalarInfo {
    ScalarOp op;
    long constInt;
    double constFloat;
    Neuron neuron;
    Connection[] connections;

    public Neuron GetNeuron {
        get {
            return neuron;
        }
    }

    public Connection[] GetConnections {
        get {
            return connections;
        }
    }

    public long AsConstInt() {
        if (op != ScalarOp.ConstInt) {
            throw new System.Exception("Not a const int");
        }
        return constInt;
    }

    public static ScalarInfo FromInt(long n) {
        ScalarInfo result = new ScalarInfo();
        result.op = ScalarOp.ConstInt;
        result.constInt = n;
        return result;
    }

    public static ScalarInfo FromFloat(double f) {
        ScalarInfo result = new ScalarInfo();
        result.op = ScalarOp.ConstFloat;
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

    public static ScalarInfo InputActivation(int layer, System.Random random) {
        ScalarInfo result = Activation(layer, random);
        result.op = ScalarOp.InputFloat;
        return result;
    }

    public static ScalarInfo Activation(int layer, System.Random random) {
        float x = layer;
        float y = random.Next();
        float z = random.Next();
        float r = random.Next();
        float g = random.Next();
        float b = random.Next();
        var point = new PointRef(new Vector3(x, y, z), new Color(r, g, b));
        var neuron = new Neuron();
        neuron.point = point;
        ScalarInfo result = new ScalarInfo();
        result.neuron = neuron;
        return result;
    }
}