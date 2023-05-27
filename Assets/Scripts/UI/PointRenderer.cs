using UnityEngine;

public class PointRenderer : MonoBehaviour {
    public Camera camera;
    public ComputeShader pointShader;
    public int pointCount = 1000000;
    public int textureWidth = 1280;
    public int textureHeight = 720;

    private ComputeBuffer pointBuffer;
    private RenderTexture resultTexture;
    private int kernelIndex;

    private void Start() {
        // Set up the compute buffer to hold point data
        pointBuffer = new ComputeBuffer(pointCount, sizeof(float) * 3);

        // Generate point positions and set them in the compute buffer
        Vector3[] pointPositions = GeneratePointPositions(pointCount);
        pointBuffer.SetData(pointPositions);

        // Create the result texture
        resultTexture = new RenderTexture(textureWidth, textureHeight, 0);
        resultTexture.enableRandomWrite = true;
        resultTexture.Create();

        // Find the kernel index of the compute shader
        kernelIndex = pointShader.FindKernel("PointRendering");

        // Set the compute buffer as a parameter in the compute shader
        pointShader.SetBuffer(kernelIndex, "pointBuffer", pointBuffer);

        // Set the result texture as a parameter in the compute shader
        pointShader.SetTexture(kernelIndex, "resultTexture", resultTexture);

        // Dispatch the compute shader
        pointShader.Dispatch(kernelIndex, pointCount / 256, 1, 1);

        // Assign the result texture to the camera's target texture
        //camera.targetTexture = resultTexture;
    }

    private void OnDestroy() {
        // Release the compute buffer when no longer needed
        pointBuffer.Release();

        // Release the render texture
        resultTexture.Release();
    }

    private Vector3[] GeneratePointPositions(int count) {
        // Generate point positions as needed
        // Replace this with your own point generation logic

        Vector3[] positions = new Vector3[count];
        for (int i = 0; i < count; i++) {
            positions[i] = Random.insideUnitSphere * 10f; // Random position within a sphere
        }

        return positions;
    }

    public ComputeShader otherShader;

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        /*
        int kernel = otherShader.FindKernel("Main");
        otherShader.SetTexture(kernel, "_color", source);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        otherShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);
        Graphics.Blit(_target, destination); //_target is the render texture the shader writes to
        */
        Graphics.Blit(resultTexture, destination);
    }
}
