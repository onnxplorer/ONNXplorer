using Onnx;
using Google.Protobuf.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttrBag {
    List<AttributeProto> attrs;

    public AttrBag(RepeatedField<AttributeProto> a) {
        attrs = new List<AttributeProto>();
        foreach (var attr in a) {
            attrs.Add(attr);
        }
    }

    public void Clear() {
        attrs.Clear();
    }

    public long PullInt(string name, long def) {
        for (var i = 0; i < attrs.Count; i++) {
            if (attrs[i].Name == name) {
                var result = attrs[i].I;
                attrs.RemoveAt(i);
                return result;
            }
        }
        return def;
    }

    public double PullFloat(string name, double def) {
        for (var i = 0; i < attrs.Count; i++) {
            if (attrs[i].Name == name) {
                var result = attrs[i].F;
                attrs.RemoveAt(i);
                return result;
            }
        }
        return def;
    }

    public int Count {
        get {
            return attrs.Count;
        }
    }

    public void GripeIfNonempty(string opType) {
        if (Count > 0) {
            var names = "";
            foreach (var attr in attrs) {
                names += $" {attr.Name}";
            }
            Debug.LogError($"Unused attributes for {opType}: {names}");
        }
    }

    public long[] PullRequiredInts(string name) {
        for (var i = 0; i < attrs.Count; i++) {
            if (attrs[i].Name == name) {
                var result = TensorInfo.repeatedLongToArray(attrs[i].Ints);
                attrs.RemoveAt(i);
                return result;
            }
        }
        Debug.LogError($"Missing attribute: {name}");
        return null;
    }

    public long[] PullInts(string name, long[] def) {
        for (var i = 0; i < attrs.Count; i++) {
            if (attrs[i].Name == name) {
                var result = TensorInfo.repeatedLongToArray(attrs[i].Ints);
                attrs.RemoveAt(i);
                return result;
            }
        }
        return def;
    }
}