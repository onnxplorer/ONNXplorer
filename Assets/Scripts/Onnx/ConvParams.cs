using Onnx;
using System.Linq;

public class ConvParams {
    public long Group;
    public long[] KernelShape;
    public long[] Pads;
    public long[] Strides;
    public long[] Dilations;

    public ConvParams() {
        Group = 1;
        KernelShape = null;
        Pads = new long[] { 0, 0, 0, 0 };
        Strides = new long[] { 1, 1 };
        Dilations = new long[] { 1, 1 };
    }

    public static ConvParams FromProto(NodeProto node) {
        var convParams = new ConvParams();
        foreach (var attr in node.Attribute) {
            switch (attr.Name) {
                case "group":
                    convParams.Group = attr.I;
                    break;
                case "kernel_shape":
                    convParams.KernelShape = attr.Ints.ToArray();
                    break;
                case "pads":
                    convParams.Pads = attr.Ints.ToArray();
                    break;
                case "strides":
                    convParams.Strides = attr.Ints.ToArray();
                    break;
                case "dilations":
                    convParams.Dilations = attr.Ints.ToArray();
                    break;
            }
        }
        return convParams;
    }
}