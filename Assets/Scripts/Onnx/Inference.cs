using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public class Inference {
    public (List<Neuron>, List<Connection>) run() {
        Debug.Log("Start function called");
        string modelPath = "models/mobilenetv2-10.onnx";
        var session = new InferenceSession(modelPath);
        Debug.Log("Session created");
        var labels = LoadLabels();
        var tensor = CreateTensorFromKitten();
        var inputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor<float>("input", tensor)
            };
        Debug.Log("Input created");
        using (var results = session.Run(inputs)) {
            Debug.Log("Results created");
            foreach (var result in results) {
                var t = result.AsTensor<float>().ToDenseTensor();
                PrintTensor(t, labels);
            }
        }

        var dim_params = new Dictionary<string, long>();
        dim_params.Add("batch_size", 1);

        return OnnxHelper.CreateModelProto(modelPath, dim_params);
    }

    List<string> LoadLabels() {
        var labels = new List<string>();
        var sr = new StreamReader("Assets/OnnxData/synset.txt");
        while (!sr.EndOfStream) {
            labels.Add(sr.ReadLine());
        }
        return labels;
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
