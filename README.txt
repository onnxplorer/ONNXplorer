Opens an ONNX ( https://onnx.ai/ ) ML model and displays it in VR for inspection and analysis.

Demo:
https://youtu.be/6wkNMwZ_VAU

Install:
Run your platform's respective `download` scripts
Open project in Unity
  We used version 2022.1.14f1.  VR was tested on Windows 10 Home, with an HTC Vive and SteamVR.  Development also took place in Linux, VR could maybe work there, too.
Try to run some scenes (Assets/Scenes/)
  You may need to re-download the SteamVR package, but it may work out-of-the-box.
The OnnxTestScene is mainly for testing that ONNX parsing works, without involving the UI.
The UITestScene runs one (or more) thing (including ONNX parsing) and displays it in VR.
  I expect this to require SteamVR.
  Note that you use the grip buttons on your VR controllers to grab the view and scale/move it around.
    I tested this on an HTC Vive - it may work with others, but I don't know the button mappings.
  The `Director` object has properties controlling what gets run (as well as hosting most of the controlling code).
  `TEST_HIJACKED_NETWORK` runs the mobilenet network modified to output layer activations, test case "-2".
  TEST_CASE is used by `TEST_ONNX`, which uses the object-based rendering method (tidier and easier to understand, but much slower and memory intensive).
    Not all test cases will necessarily work - some of them may have become incompatible as development progressed.
  BREAK_ON_LAYER is only used with the old method of rendering, atm.
  The new method of rendering has a different limit available - see line ~95 of Inference.cs: the last two parameters of `GetCoordArrays` control the maximum number of neurons and connections displayed per tensor.  Some effort goes into making those the most relevant neurons, rather than e.g. the first ones found.
In general, files you may be particularly interested in, for the purpose of running the thing, include OnxxController.cs and Inference.cs.
  TensorInfo.cs and ScalarInfo.cs contain a lot of the code to parse the models and layout the render.  Layout.cs copies some of that code, and is used for the array-based rendering system.  They contain some rendering parameters, such as the distance between layers, or the sorta basis vectors used to project the tensors down into three dimensions.

Note - as it stands, I don't think connection weights are represented?  There was also some trouble with convolutional layers; like, their matrix multiplications were arguably representable by an extra layer of neurons cross-connected, but that SIGNIFICANLY increased the amount of stuff to render.  Both of these issues were tackled in a branch, but I had trouble getting them to work reliably, so I reverted the relevant merge and reverted them back into existence in the feature/conv_rendering branch.

Originally developed for the Apart Research Alignment Jam #5 (Scale Oversight), 2023.
  See the pdf.
This work released under the Apache 2.0 license.
