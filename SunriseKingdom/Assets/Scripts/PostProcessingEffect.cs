using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessingEffect : MonoBehaviour {

    public Material mat;
	// Use this for initialization
	void Start () {

	}

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }
}
