set -ex
mkdir -p models
# curl and output to the models/ directory
curl -L https://github.com/onnx/models/raw/main/vision/classification/mobilenet/model/mobilenetv2-10.onnx -o models/mobilenetv2-10.onnx
curl -L https://github.com/onnx/onnx/raw/main/onnx/backend/test/data/node/test_sigmoid/model.onnx -o models/test_sigmoid.onnx