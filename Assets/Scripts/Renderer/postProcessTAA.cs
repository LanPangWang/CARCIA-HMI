// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class postProcessTAA : MonoBehaviour
{
    [Range(-1f, 1f)]
    public float _AvmDevideOffset;
    public float TAAJitterScale;
    public float TAAJitterClamp;
	public Camera mainCamera;
    public Camera AVMCamera;
    public Camera freeSpaceCamera;
    public Material _blitMat;
    public Shader _getDepthShader;
    public Shader _getFreeSpaceShader;
    public Material _blurMat;
    public RenderTexture _RTPre;
    public RenderTexture _RTDepth;
    public RenderTexture _RTDevide;
    public RenderTexture _RTAVM;
    public RenderTexture _RTAVMBlend;
    public RenderTexture _RTFreeSpace;
    public RenderTexture _RTFreeSpaceBlur;
    public Texture2D _AVMRaw;

    public int HaltonCount;
    public float HaltonFactor1;
    public float HaltonFactor2;
    private List<Vector2> HaltonList = new List<Vector2>();
    private int haltonFrames = 0;

    private Matrix4x4 _MainCameraProjection;
    private Matrix4x4 preProj;
    private Matrix4x4 lastWorldToLocalMatrix;
    private bool isFirstFrame;
    // private bool renderingDepth;
    // private bool renderedDepth;
    // Start is called before the first frame update
    void Start()
    {
        ResizeAllRT();
        // _MainCameraProjection = mainCamera.projectionMatrix;
		Shader.SetGlobalTexture("_MainCameraRGBAPre", _RTPre);
		Shader.SetGlobalTexture("_MainCameraDepthTexture", _RTDepth);
		Shader.SetGlobalTexture("_AvmCameraFreeSpaceTexture", _RTFreeSpaceBlur);
        // Debug.Log(_MainCameraProjection);
        isFirstFrame = true;
        HaltonGenerate();
        Debug.Log(mainCamera.renderingPath);
        // mainCamera.enabled = false;
        // mainCamera.depthTextureMode = DepthTextureMode.Depth;
        // beforeGbuffer.SetViewProjectionMatrices(mainCamera.transform.worldToLocalMatrix, preProj);
        // _TAAJitterArray.Add(new Vector2(TAAJitterScale * , TAAJitterScale));
    }
    void OnDestroy()
    {
    }
    float ItoHalton(float input, ref float haltonFactor)
    {
        float haltonCache = input;
        List<float> factorArray1 = new List<float>();
        while(haltonCache > 0)
        {
            factorArray1.Add(haltonCache % haltonFactor);
            haltonCache = (haltonCache - factorArray1[factorArray1.Count - 1]) / haltonFactor;
        }
        float Numerator = 0f;
        float Denominator = Mathf.Pow(haltonFactor, factorArray1.Count);
        for (int i = factorArray1.Count; i > 0 ; i -= 1)
        {
            Numerator += factorArray1[factorArray1.Count - i] * Mathf.Pow(haltonFactor, i - 1);
        }
        // Debug.Log(Numerator.ToString() + "/" + Denominator.ToString());
        return Numerator / Denominator;
    }

    void HaltonGenerate()
    {
        HaltonList.Clear();
        for (int i = 1; i < HaltonCount + 1; i += 1)
        {
            HaltonList.Add(new Vector2(
                ItoHalton(i, ref HaltonFactor1) * 2f - 1f,
                ItoHalton(i, ref HaltonFactor2) * 2f - 1f
            ));
        }
    }

    void UpdateProjMatrix()
    {
        // mainCamera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, beforeGbuffer);
        preProj = mainCamera.projectionMatrix;
        preProj.m02 += TAAJitterScale * Mathf.Clamp(HaltonList[haltonFrames].x, -TAAJitterClamp, TAAJitterClamp) / mainCamera.pixelWidth;
        preProj.m12 += TAAJitterScale * Mathf.Clamp(HaltonList[haltonFrames].y, -TAAJitterClamp, TAAJitterClamp) / mainCamera.pixelHeight;
        // preProj.m02 += TAAJitterScale * HaltonList[haltonFrames].x / mainCamera.pixelWidth;
        // preProj.m12 += TAAJitterScale * HaltonList[haltonFrames].y / mainCamera.pixelHeight;
        // mainCamera.AddCommandBuffer (CameraEvent.BeforeGBuffer, beforeGbuffer);
        // context.ExecuteCommandBuffer(beforeGbuffer);
        mainCamera.projectionMatrix = preProj;

        haltonFrames += 1;
        haltonFrames = haltonFrames >= HaltonCount ? 0 : haltonFrames;
    }

    void ResizeAllRT()
    {

        ResizeRT(ref _RTPre, new Vector2(Screen.width, Screen.height));
        ResizeRT(ref _RTDepth, new Vector2(Screen.width, Screen.height));
        ResizeRT(ref _RTDevide, new Vector2(Screen.width, Screen.height));
        // float AVMSize = Mathf.Min(Screen.height, Screen.width);
        // ResizeRT(ref _RTAVM, new Vector2(AVMSize, AVMSize));
        ResizeRT(ref _RTAVM, new Vector2(Screen.width, Screen.height));
        ResizeRT(ref _RTAVMBlend, new Vector2(Screen.width, Screen.height));
        // ResizeRT(ref _RTFreeSpace, new Vector2(Screen.width, Screen.height));
        AVMCamera.enabled = false;
        AVMCamera.enabled = true;
    }
	void ResizeRT(ref RenderTexture rtIN, Vector2 Size)
	{
		rtIN.Release ();
		rtIN.height = (int)(Size.y);
		rtIN.width = (int)(Size.x);
		rtIN.Create ();
	}    

    void Update()
    {
		if (Screen.height != _RTPre.height || Screen.width != _RTPre.width) {
            ResizeAllRT();
            isFirstFrame = true;
            haltonFrames = 0;
		}
		_blitMat.SetMatrix ("_MainCameraToWorld", mainCamera.transform.localToWorldMatrix);
		// _blitMat.SetMatrix ("_MainWorldToCamera", lastWorldToLocalMatrix);
        // Debug.Log(mainCamera.transform.localToWorldMatrix);
        // Debug.Log(lastWorldToLocalMatrix);
        // Debug.Log("-=-=-====");

		_blitMat.SetMatrix ("_MainCameraProjection", mainCamera.projectionMatrix);
        _blitMat.SetMatrix ("_MainCameraInvProjection", mainCamera.projectionMatrix.inverse);
        _blitMat.SetFloat("_MainCameraFarClip", mainCamera.farClipPlane);
        UpdateProjMatrix();
        // renderingDepth = false;
        // mainCamera.renderingPath = RenderingPath.Forward;
        // mainCamera.clearFlags = CameraClearFlags.SolidColor;
        // // mainCamera.RenderWithShader(_getDepthShader, "RenderType");
        // mainCamera.renderingPath = RenderingPath.DeferredShading;
        // mainCamera.clearFlags = CameraClearFlags.Skybox;
        // mainCamera.ResetReplacementShader();
    }

	void OnPreRender() {
        // _MainCameraProjection = mainCamera.projectionMatrix;
        freeSpaceCamera.RenderWithShader(_getFreeSpaceShader, "RenderType");
        _blurMat.SetTexture("_MainTex", _RTFreeSpace);
        Graphics.Blit(_RTFreeSpace, _RTFreeSpaceBlur, _blurMat, 0);
        
		Shader.SetGlobalVector("_AVMCameraPos", new Vector4(freeSpaceCamera.transform.position.x, 
                                                            freeSpaceCamera.transform.position.y, 
                                                            freeSpaceCamera.transform.position.z, 
                                                            freeSpaceCamera.orthographicSize));
        // mainCamera.projectionMatrix = preProj;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Graphics.Blit(source, destination);
        // if (!renderingDepth)
        {
            _blitMat.SetTexture("_MainTex", source);
            if (!isFirstFrame)
            {
                Graphics.Blit(source, _RTPre, _blitMat, 2);
            }
            else
            {
                Graphics.Blit(source, _RTPre);
                isFirstFrame = false;
                Debug.Log("PreRGBA Inited");
            }
            // _blitMat.SetTexture("_MainTex", destination);

            _blitMat.SetTexture("_AVMTex", _RTAVM);
            _blitMat.SetTexture("_AVMRaw", _AVMRaw);
            Graphics.Blit(_RTAVM, _RTAVMBlend, _blitMat, 5);

            _blitMat.SetTexture("_MainTex", _RTPre);
            _blitMat.SetFloat("_AvmDevideOffset", _AvmDevideOffset);
            _blitMat.SetTexture("_AVMTex", _RTAVMBlend);
            Graphics.Blit(_RTPre, _RTDevide, _blitMat, 4);

            lastWorldToLocalMatrix = mainCamera.transform.worldToLocalMatrix;
            _blitMat.SetMatrix ("_MainWorldToCamera", lastWorldToLocalMatrix);
            mainCamera.projectionMatrix = Matrix4x4.Perspective(mainCamera.fieldOfView, mainCamera.aspect, mainCamera.nearClipPlane, mainCamera.farClipPlane);

            Graphics.Blit(_RTDevide, destination);
        }
        // else
        // {
        //     Graphics.Blit(source, _RTDepth);
        //     renderingDepth = false;
        // }
    }
	void OnPostRender()
	{
        // mainCamera.projectionMatrix = _MainCameraProjection;
	}
}
