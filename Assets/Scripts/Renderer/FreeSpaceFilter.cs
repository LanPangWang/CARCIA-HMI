// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FreeSpaceFilter : MonoBehaviour
{
    public Camera freeSpaceCamera;
    // public Shader _getFreeSpaceShader;
    public RenderTexture _RTFreeSpace;
    public RenderTexture _RTFreeSpaceDivide;
    public RenderTexture _RTFreeSpaceFilt;
    public RenderTexture _RTFreeSpaceBlur;
    public ComputeShader _CSFreeSpaceFilter;
	private int CSFreeSpaceId;
    // Start is called before the first frame update
    void Start()
    {
        
		// Shader.SetGlobalTexture("_AvmCameraFreeSpaceTexture", _RTFreeSpaceBlur);
		// _CSFreeSpaceFilter.SetFloat("_ResultResolution", _RTFreeSpaceDivide.width);
		// CSFreeSpaceId = _CSFreeSpaceFilter.FindKernel("CSMain");
		// _CSFreeSpaceFilter.SetTexture(CSFreeSpaceId, "FreeSpaceRaw", _RTFreeSpace);
		// _CSFreeSpaceFilter.SetTexture(CSFreeSpaceId, "FreeSpaceDivide", _RTFreeSpaceDivide);
    }

    // Update is called once per frame
    void Update()
    {
        
    }	
    
    void OnPreRender() {
        // freeSpaceCamera.RenderWithShader(_getFreeSpaceShader, "RenderType");
        // _blurMat.SetTexture("_MainTex", _RTFreeSpace);
        // Graphics.Blit(_RTFreeSpace, _RTFreeSpaceBlur, _blurMat, 0);
        
        Shader.SetGlobalVector("_AVMCameraPos", new Vector4(freeSpaceCamera.transform.position.x, 
                                                            freeSpaceCamera.transform.position.y, 
                                                            freeSpaceCamera.transform.position.z, 
                                                            freeSpaceCamera.orthographicSize));
        // float cameraOffsetZ = freeSpaceCamera.transform.position.z / (2 * freeSpaceCamera.orthographicSize);
		// _CSFreeSpaceFilter.SetFloat("_CameraOffsetZ", _RTFreeSpaceDivide.width * (0.5f - cameraOffsetZ));
    }
	void OnPostRender()
	{
        // mainCamera.projectionMatrix = _MainCameraProjection;
        // _CSFreeSpaceFilter.Dispatch(CSFreeSpaceId, 1, 1, 1);
        // Graphics.Blit(_RTFreeSpace, _RTFreeSpaceDivide);
	}
}
