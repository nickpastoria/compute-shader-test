// Code Copied from Sebastian Lague Video https://www.youtube.com/watch?v=9RHGLZLUuwc&t=13s
// As well as this paper featured in the video http://blog.three-eyed-games.com/2018/05/03/gpu-ray-tracing-in-unity-part-1/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingMasster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    private RenderTexture _target;
    public Texture SkyboxTexture;
    private uint _currentSample = 0;
    private Material _addMaterial;
    public float scalias = 1f;
    
    private struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();
        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        // Blit the result texture to the screen
        // Blit the result texture to the screen
        if (_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        _addMaterial.SetFloat("_Sample", _currentSample);
        Graphics.Blit(_target, destination, _addMaterial);
        _currentSample++;
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
                _target.Release();
            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }

    private void Update() 
    {
        if (transform.hasChanged) 
        {
            _currentSample = 0;
            transform.hasChanged = false;
        }
    }

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void SetShaderParameters()
    {
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value * scalias, Random.value * scalias));
    }
}
