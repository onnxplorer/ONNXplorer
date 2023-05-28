using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;
using UnityEngine;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Text;

public class Inference {
    Thread thread;

    public (List<Neuron>, List<Connection>) run(Consumer<(List<Neuron>, List<Connection>)> objectCallback, Consumer<(List<CoordArrays>, List<CoordArrays>)> arrayCallback, int testCase = -2, int breakEarly = 1) {
        Debug.Log("Start function called");
        string modelPath;
        DenseTensor<float> tensor;
        var inputs = new List<NamedOnnxValue>();
        List<string> labels;
        UsefulModelInfo usefulInfo = null;
        switch (testCase) {
            case -3: {
                modelPath = "models/test_conv_with_autopad_same.onnx";
                string modifiedModelPath = "models/test_conv_with_autopad_same_modified.onnx";

                usefulInfo = Manipulate.ModifyOnnxFile(modelPath, modifiedModelPath);
                Debug.Log("Model modified");
                modelPath = modifiedModelPath;
                inputs.Add(NamedOnnxValue.CreateFromTensor<float>("x", RandomTensor(new[] { 1, 1, 5, 5 })));
                inputs.Add(NamedOnnxValue.CreateFromTensor<float>("W", RandomTensor(new[] { 1, 1, 3, 3 })));
                labels = null;
                break;
            }
            case -2: {
                modelPath = "models/mobilenetv2-10.onnx";
                string modifiedModelPath = "models/mobilenetv2-10-modified.onnx";

                usefulInfo = Manipulate.ModifyOnnxFile(modelPath, modifiedModelPath);
                Debug.Log("Model modified");
                modelPath = modifiedModelPath;
                inputs.Add(NamedOnnxValue.CreateFromTensor<float>("input", CreateTensorFromKitten()));
                labels = LoadLabels(); //THINK Not sure if this is only for mobilenet or what
                break;
            }
            case -1: {
                tensor = RandomTensor(new[] { 2, 2 });
                Debug.Log(PrintTensor(tensor));
                return (null, null);
            }
            case 0: { // mobilenetv2
                modelPath = "models/mobilenetv2-10.onnx";
                inputs.Add(NamedOnnxValue.CreateFromTensor<float>("input", CreateTensorFromKitten()));
                labels = LoadLabels(); //THINK Not sure if this is only for mobilenet or what
                break;
            }
            case 1: { // test_sigmoid
                modelPath = "models/test_sigmoid.onnx";
                inputs.Add(NamedOnnxValue.CreateFromTensor<float>("x", RandomTensor(new[] { 3, 4, 5 })));
                labels = null;
                break;
            }
            case 2: { // test_conv_with_autopad_same
                modelPath = "models/test_conv_with_autopad_same.onnx";
                inputs.Add(NamedOnnxValue.CreateFromTensor<float>("x", RandomTensor(new[] { 1, 1, 5, 5 })));
                inputs.Add(NamedOnnxValue.CreateFromTensor<float>("W", RandomTensor(new[] { 1, 1, 3, 3 })));
                labels = null;
                break;
            }
            default: {
                Debug.LogWarning("Unhandled test case number: " + testCase);
                return (null, null);
            }
        }
        var session = new InferenceSession(modelPath);
        Debug.Log("Session created");
        Debug.Log("Input created");
        using (var results = session.Run(inputs)) {
            Debug.Log($"Results created. There are {results.Count} results.");
            foreach (var result in results) {
                if (usefulInfo == null || result.Name == usefulInfo.OriginalOutputs[0]) {
                    var t = result.AsTensor<float>().ToDenseTensor();
                    if (labels != null) {
                        PrintTensor(t, labels);
                    } else {
                        Debug.Log("result " + result.Name + " : " + PrintTensor(t));
                    }
                }
            }

            if (usefulInfo != null) {
                long totalNeuronCoordCount = 0;
                long totalConnectionCoordCount = 0;
                var (neuronCoords, connectionCoords) = Layout.GetCoordArrays(usefulInfo, results, 10000, 1000);

                foreach (var coordArray in neuronCoords) {
                    totalNeuronCoordCount += coordArray.Length;
                }
                foreach (var coordArray in connectionCoords) {
                    totalConnectionCoordCount += coordArray.Length;
                }

                Debug.Log($"Total coord count. Neurons: {totalNeuronCoordCount}. Connections: {totalConnectionCoordCount}");

                if (arrayCallback != null) {
                    arrayCallback((neuronCoords, connectionCoords));
                }
            }
        }

        if (usefulInfo == null) { //DUMMY Deal with if it is
            var dim_params = new Dictionary<string, long>();
            dim_params.Add("batch_size", 1);

            if (objectCallback != null) {
                thread = new Thread(new ThreadStart(() => {
                    objectCallback(OnnxHelper.CreateModelProto(modelPath, dim_params, breakEarly));
                }));
                thread.Start();
                return (null, null);
            } else {
                return OnnxHelper.CreateModelProto(modelPath, dim_params, breakEarly);
            }
        } else {
            return (null, null);
        }
    }

    List<string> LoadLabels() {
        var labels = new List<string>();
        var sr = new StreamReader("Assets/OnnxData/synset.txt");
        while (!sr.EndOfStream) {
            labels.Add(sr.ReadLine());
        }
        return labels;
    }

    String PrintTensor(DenseTensor<float> tensor) {
        var dims = tensor.Dimensions;
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        for (int i = 0; i < dims[0]; i++) {
            //CHECK reverseStride?
            if (dims.Length > 1) {
                var subtensor = new DenseTensor<float>(tensor.Buffer.Slice(i * tensor.Strides[0], tensor.Strides[0]), dims.Slice(1, dims.Length - 1));
                sb.Append(PrintTensor(subtensor)+(i == dims[0]-1 ? "" : ",\n"));
            } else {
                sb.Append(tensor[i]+(i == dims[0]-1 ? "" : ","));
            }
        }
        sb.Append("]");
        return sb.ToString();
    }

    void PrintTensor(DenseTensor<float> tensor, List<string> labels) {
        var count = 5;
        Debug.Log($"Top {count} values:");
        var values = new List<(float, int)>();
        for (int j = 0; j < tensor.Dimensions[1]; j++) {
            values.Add((tensor[0, j], j));
        }
        values.Sort((a, b) => b.Item1.CompareTo(a.Item1));
        for (int k = 0; k < count; k++) {
            var (value, j) = values[k];
            Debug.Log($"Value: {value}, Index: {labels[j]}");
        }
    }

    DenseTensor<float> RandomTensor(ReadOnlySpan<int> dimensions) {
        var tensor = new DenseTensor<float>(dimensions);
        int N = dimensions.Length;
        int[] idx = new int[N];
        int i = N - 1;
        bool done = false;
        idx[i] = -1;
        while (!done) {
            if (i < 0) {
                done = true;
            } else if (idx[i] == dimensions[i] - 1) {
                idx[i] = 0;
                i--;
                continue;
            } else {
                idx[i]++;
                i = N - 1;
            }
            tensor[idx] = UnityEngine.Random.Range(0f, 1f);
        }
        return tensor;
    }

    DenseTensor<float> CreateTensorFromKitten() {
        // Load the image
        var originalImage = LoadImage("Assets/OnnxData/kitten.jpg");

        Debug.Log("Image loaded");

        // Crop the image to 224x224 pixels
        var resizedImage = ResizeImage(originalImage, 256, 256);
        var croppedImage = CropImage(resizedImage, 224, 224);

        // Normalize each channel to the specified mean and standard deviation
        var mean = new[] { 0.485f, 0.456f, 0.406f };
        var std = new[] { 0.229f, 0.224f, 0.225f };
        var tensor = new DenseTensor<float>(new[] { 1, 3, 224, 224 });
        for (int y = 0; y < 224; y++) {
            for (int x = 0; x < 224; x++) {
                var color = croppedImage.GetPixel(x, y);
                tensor[0, 0, y, x] = (color.r - mean[0]) / std[0];
                tensor[0, 1, y, x] = (color.g - mean[1]) / std[1];
                tensor[0, 2, y, x] = (color.b - mean[2]) / std[2];
            }
        }

        Debug.Log("Tensor created");

        return tensor;
    }

    Texture2D LoadImage(string path) {
        var fileData = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        return tex;
    }

    Texture2D ResizeImage(Texture2D image, int width, int height) {
        var resizedImage = new Texture2D(width, height);
        for (int y = 0; y < height; y++) {
            var dy = (float)y / height;
            for (int x = 0; x < width; x++) {
                var dx = (float)x / width;
                resizedImage.SetPixel(x, y, image.GetPixelBilinear(dx, dy));
            }
        }
        resizedImage.Apply();

        return resizedImage;
    }

    Texture2D CropImage(Texture2D image, int width, int height) {
        var x = (image.width - width) / 2;
        var y = (image.height - height) / 2;
        var croppedImage = new Texture2D(width, height);
        croppedImage.SetPixels(image.GetPixels(x, y, width, height));
        croppedImage.Apply();

        return croppedImage;
    }
}
