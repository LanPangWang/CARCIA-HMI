using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class postProcessMSAA : MonoBehaviour
{
    public float TAAJitterScale;
    public float TAAJitterClamp;
	public Camera mainCamera;
    public Material _blitMat;
    public RenderTexture _RTPre;

    public int HaltonCount;
    public int MSAATimes;
    public float HaltonFactor1;
    public float HaltonFactor2;
    private List<Vector2> HaltonList = new List<Vector2>();

    private Matrix4x4 preProj;
    private bool isFirstFrame;
    private int haltonFrames = 0;
    // Start is called before the first frame update
    void Start()
    {
		Shader.SetGlobalTexture("_MainCameraRGBAPre", _RTPre);
        HaltonGenerate();
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
    }

	void ResizeRT(ref RenderTexture rtIN, Vector2 Size)
	{
		rtIN.Release ();
		rtIN.height = (int)(Size.y);
		rtIN.width = (int)(Size.x);
		rtIN.Create ();
	}    
    // Update is called once per frame
    void Update()
    {
		if (Screen.height != _RTPre.height || Screen.width != _RTPre.width) {
			ResizeRT(ref _RTPre, new Vector2(Screen.width, Screen.height));
		}
        haltonFrames = 0;
        
        Matrix4x4 thisMainProj = Matrix4x4.Perspective(mainCamera.fieldOfView, mainCamera.aspect, mainCamera.nearClipPlane, mainCamera.farClipPlane);
        for (haltonFrames = 0; haltonFrames < MSAATimes; haltonFrames += 1)
        {
            mainCamera.projectionMatrix = thisMainProj;
            UpdateProjMatrix();
            mainCamera.Render();
        }
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _blitMat.SetTexture("_MainTex", source);
        if (haltonFrames > 0)
        {
            Graphics.Blit(source, _RTPre, _blitMat, 3);
        }
        else
        {
            Graphics.Blit(source, _RTPre);
        }
        if (haltonFrames >= MSAATimes)
        {
            Graphics.Blit(_RTPre, destination);
        }
    }
}
