set -ex
mkdir -p models
# curl and output to the models/ directory
curl -L https://github.com/onnx/models/raw/main/vision/classification/mobilenet/model/mobilenetv2-10.onnx -o models/mobilenetv2-10.onnx
