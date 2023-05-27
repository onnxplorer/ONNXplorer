set -ex

mkdir -p Assets/packages
rm -rf zzz_temp_download

curl -OL https://www.nuget.org/api/v2/package/Microsoft.ML.OnnxRuntime/1.15.0
unzip 1.15.0 -d Assets/packages/onnxruntime
rm 1.15.0

curl -OL https://www.nuget.org/api/v2/package/Microsoft.ML.OnnxRuntime.Managed/1.15.0
unzip 1.15.0 -d Assets/packages/onnxruntime-managed
rm 1.15.0

mv Assets/packages/onnxruntime/runtimes/linux-x64 zzz_temp_download
rm -r Assets/packages/onnxruntime/runtimes
mkdir -p Assets/packages/onnxruntime/runtimes
mv zzz_temp_download Assets/packages/onnxruntime/runtimes/linux-x64

mv Assets/packages/onnxruntime-managed/lib/netstandard2.0 zzz_temp_download
rm -r Assets/packages/onnxruntime-managed/lib
mkdir -p Assets/packages/onnxruntime-managed/lib
mv zzz_temp_download Assets/packages/onnxruntime-managed/lib/netstandard2.0

curl -OL https://www.nuget.org/api/v2/package/Google.Protobuf/3.23.2
unzip 3.23.2 -d Assets/packages/protobuf
rm 3.23.2

mv Assets/packages/protobuf/lib/netstandard2.0 zzz_temp_download
rm -r Assets/packages/protobuf/lib
mkdir -p Assets/packages/protobuf/lib
mv zzz_temp_download Assets/packages/protobuf/lib/netstandard2.0

curl -OL https://www.nuget.org/api/v2/package/System.Runtime.CompilerServices.Unsafe/4.5.2
unzip 4.5.2 -d Assets/packages/unsafe
rm 4.5.2

mv Assets/packages/unsafe/lib/netstandard2.0 zzz_temp_download
rm -r Assets/packages/unsafe/lib
mkdir -p Assets/packages/unsafe/lib
mv zzz_temp_download Assets/packages/unsafe/lib/netstandard2.0
rm -r Assets/packages/unsafe/ref

