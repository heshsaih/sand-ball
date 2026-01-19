using System;
using UnityEditor.Rendering;
using UnityEngine;

public class SandController : MonoBehaviour
{
    [SerializeField]
    public ComputeShader sandShader;
    [SerializeField]
    public RenderTexture sandRT;
    [SerializeField]
    private float colorValueToAdd;
    private MeshRenderer meshRenderer;
    [SerializeField]
    private int resolution = 512;
    [SerializeField]
    private int spotSize = 10;
    private Vector2Int position = new Vector2Int(256, 256);

    void Awake()
    {
        CreateRenderTexture();
        SetRTWhite();
        SetMaterialTexture();
        InvokeRepeating(nameof(AddSandLayer), .1f, .3f);
        ExtendBoudOfMesh();
    }

    private void CreateRenderTexture()
    {
        sandRT = new RenderTexture(resolution, resolution, 24);
        sandRT.enableRandomWrite = true;
        sandRT.Create();
    }

    private void SetRTWhite()
    {
        int kernelHandle = sandShader.FindKernel(ShaderProperties.fillWhiteKernel);
        sandShader.SetTexture(kernelHandle, ShaderProperties.sandImage, sandRT);
        sandShader.SetFloat(ShaderProperties.colorValue, colorValueToAdd);
        sandShader.SetFloat(ShaderProperties.resolution, resolution);
        sandShader.SetFloat(ShaderProperties.posX, 0);
        sandShader.SetFloat(ShaderProperties.posY, 0);
        sandShader.SetFloat(ShaderProperties.spotSize, 0);

        sandShader.Dispatch(kernelHandle, sandRT.width / 8, sandRT.height / 8, 1);
    }

    private void SetMaterialTexture()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.SetTexture("_PathTexture", sandRT);
    }

    private void AddSandLayer()
    {
        int kernelHandle = sandShader.FindKernel(ShaderProperties.csMainKernel);
        sandShader.SetTexture(kernelHandle, ShaderProperties.sandImage, sandRT);
        sandShader.SetFloat(ShaderProperties.colorValue, colorValueToAdd);
        sandShader.SetFloat(ShaderProperties.resolution, resolution);
        sandShader.SetFloat(ShaderProperties.posX, 0);
        sandShader.SetFloat(ShaderProperties.posY, 0);
        sandShader.SetFloat(ShaderProperties.spotSize, 0);

        sandShader.Dispatch(kernelHandle, sandRT.width / 8, sandRT.height / 8, 1);
    }

    private void ExtendBoudOfMesh()
    {
        Bounds bounds = GetComponent<MeshFilter>().mesh.bounds;
        bounds.extents = new Vector3(2, 0, 2);
        GetComponent<MeshFilter>().mesh.bounds = bounds;
    }
}
