using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class postProcess : MonoBehaviour
{
    public Material _blitMat;
    public RenderTexture _RT;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
        
		// if (Screen.height != _RT.height || Screen.width != _RT.width) {
		// 	ResizeRT(ref _RT, new Vector2(2 * Screen.width, 2 * Screen.height));
		// }
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Graphics.Blit(_renderTextures[1], _velocityOut);
        // Graphics.Blit(_renderTextures[0], _velocityOut, _blitMat, 0);
        //Graphics.Blit(_renderTextures[1], _velocityOut);
        // Graphics.Blit(_renderTextures[0], destination, _blitMat, 2);
        // Graphics.Blit(source, _RT);
        Graphics.Blit(source, destination, _blitMat, 0);
    }
}
