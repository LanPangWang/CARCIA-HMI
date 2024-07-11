// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class postProcess : MonoBehaviour
{
    public float TAARandOffset;
	public Camera mainCamera;
    public Material _blitMat;
    public RenderTexture _RTPre;

    private Vector2[] Halton8 = {new Vector2(1f / 2f - 0.5f, 1f / 3f - 0.5f),
                                new Vector2(1f / 4f - 0.5f, 2f / 3f - 0.5f),
                                new Vector2(3f / 4f - 0.5f, 1f / 9f - 0.5f),
                                new Vector2(1f / 8f - 0.5f, 4f / 9f - 0.5f),
                                new Vector2(5f / 8f - 0.5f, 7f / 9f - 0.5f),
                                new Vector2(3f / 8f - 0.5f, 2f / 9f - 0.5f),
                                new Vector2(7f / 8f - 0.5f, 5f / 9f - 0.5f),
                                new Vector2(1f / 16f - 0.5f, 8f / 9f - 0.5f)};
    private int frames = 0;
    private Matrix4x4 _MainCameraProjection;
    private Matrix4x4 preProj;
    private bool isFirstFrame;
    // Start is called before the first frame update
    void Start()
    {
        _MainCameraProjection = mainCamera.projectionMatrix;
		Shader.SetGlobalTexture("_MainCameraRGBAPre", _RTPre);
        // Debug.Log(_MainCameraProjection);
        isFirstFrame = true;
        // _TAAJitterArray.Add(new Vector2(TAARandOffset * , TAARandOffset));
    }

	void ResizeRT(ref RenderTexture rtIN, Vector2 Size)
	{
		rtIN.Release ();
		rtIN.height = (int)(Size.y);
		rtIN.width = (int)(Size.x);
		//rtIN.height = Screen.height;
		//rtIN.width = Screen.width;
		rtIN.Create ();
	}    
    void Update()
    {
        
		if (Screen.height != _RTPre.height || Screen.width != _RTPre.width) {
			ResizeRT(ref _RTPre, new Vector2(Screen.width, Screen.height));
		}
    }
	void OnPreRender() {
        preProj = _MainCameraProjection;
        // Debug.Log(Random.Range(-TAARandOffset, TAARandOffset));
        // preProj[0, 2] += Random.Range(-TAARandOffset, TAARandOffset) / Screen.width;
        // preProj[1, 2] += Random.Range(-TAARandOffset, TAARandOffset) / Screen.height;
        preProj[0, 2] += TAARandOffset * Halton8[frames].x / Screen.width;
        preProj[1, 2] += TAARandOffset * Halton8[frames].y / Screen.height;

        frames += 1;
        frames = frames == 8 ? 0 : frames;

        mainCamera.projectionMatrix = preProj;
		_blitMat.SetMatrix ("_MainCameraToWorld", mainCamera.transform.localToWorldMatrix);
		_blitMat.SetMatrix ("_MainCameraInvProjection", mainCamera.projectionMatrix.inverse);
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Graphics.Blit(_renderTextures[1], _velocityOut);
        // Graphics.Blit(_renderTextures[0], _velocityOut, _blitMat, 0);
        //Graphics.Blit(_renderTextures[1], _velocityOut);
        // Graphics.Blit(_renderTextures[0], destination, _blitMat, 2);
        _blitMat.SetTexture("_MainTex", source);
        if (!isFirstFrame)
        {
            Graphics.Blit(source, destination, _blitMat, 2);
        }
        else
        {
            Graphics.Blit(source, destination);
            isFirstFrame = false;
        }
        _blitMat.SetTexture("_MainTex", destination);
        Graphics.Blit(destination, _RTPre, _blitMat, 3);
        // Graphics.Blit(source, destination, _blitMat, 0);
        // Graphics.Blit(_RTPre, destination, _blitMat, 1);
    }
	void OnPostRender()
	{
		_blitMat.SetMatrix ("_MainWorldToCamera", mainCamera.transform.worldToLocalMatrix);
		_blitMat.SetMatrix ("_MainCameraProjection", mainCamera.projectionMatrix);
	}
}
