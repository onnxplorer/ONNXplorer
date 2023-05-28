using System;
using System.Collections.Generic;
using Onnx;
using UnityEngine;

public class ScalarInfo {
    ScalarOp op;
    long constInt;
    double constFloat;
    Neuron neuron;
    List<Connection> connections;

    public Neuron GetNeuron {
        get {
            return neuron;
        }
    }

    public List<Connection> GetConnections {
        get {
            return connections;
        }
    }

    public bool IsConstFloatZero() {
        return op == ScalarOp.ConstFloat && constFloat == 0;
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

    public static ScalarInfo AddFloats(int layer, System.Random random, ScalarInfo a, ScalarInfo b) {
        if (a.IsConstFloatZero()) {
            return b;
        }
        if (b.IsConstFloatZero()) {
            return a;
        }
        var result = Activation(layer, random);
        result.op = ScalarOp.AddFloat;
        result.connections = new List<Connection>();
        if (a.neuron != null) {
            result.connections.Add(new Connection(a.neuron, result.neuron));
        }
        if (b.neuron != null) {
            result.connections.Add(new Connection(b.neuron, result.neuron));
        }
        return result;
    }

    public static ScalarInfo SumFloats(int layer, System.Random random, List<ScalarInfo> scalars) {
        var result = Activation(layer, random);
        result.op = ScalarOp.AddFloat;
        result.connections = new List<Connection>();
        foreach (var scalar in scalars) {
            if (scalar.IsConstFloatZero()) {
                continue;
            }
            if (scalar.neuron != null) {
                result.connections.Add(new Connection(scalar.neuron, result.neuron));
            }
        }
        return result;
    }

    public static ScalarInfo MulFloats(int layer, System.Random random, ScalarInfo a, ScalarInfo b) {
        if (a.IsConstFloatZero() || b.IsConstFloatZero()) {
            return FromFloat(0);
        }
        var result = Activation(layer, random);
        result.op = ScalarOp.MulFloat;
        result.connections = new List<Connection>();
        if (a.neuron != null) {
            result.connections.Add(new Connection(a.neuron, result.neuron));
        }
        if (b.neuron != null) {
            result.connections.Add(new Connection(b.neuron, result.neuron));
        }
        return result;
    }

    public static ScalarInfo ClipFloat(int layer, System.Random random, ScalarInfo a) {
        var result = Activation(layer, random);
        result.op = ScalarOp.ClipFloat;
        result.connections = new List<Connection>();
        if (a.neuron != null) {
            result.connections.Add(new Connection(a.neuron, result.neuron));
        }
        return result;
    }

    public static ScalarInfo SigmoidFloat(int layer, System.Random random, ScalarInfo a) {
        var result = Activation(layer, random);
        result.op = ScalarOp.SigmoidFloat;
        result.connections = new List<Connection>();
        if (a.neuron != null) {
            result.connections.Add(new Connection(a.neuron, result.neuron));
        }
        return result;
    }
}