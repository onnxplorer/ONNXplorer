mkdir models
curl -OL https://github.com/onnx/models/raw/main/vision/classification/mobilenet/model/mobilenetv2-12-int8.tar.gz
REM Untar into the models directory
tar -zxvf mobilenetv2-12-int8.tar.gz -C models
REM Remove the tar file
rm mobilenetv2-12-int8.tar.gz
