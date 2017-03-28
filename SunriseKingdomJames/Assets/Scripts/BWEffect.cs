using UnityEngine;
using System.Collections;
//using RenderHeads.Media.AVProVideo



[ExecuteInEditMode]
public class BWEffect : MonoBehaviour
{

    public float height;
    private Material material;

    // Creates a private material used to the effect
    void Awake()
    {
        material = new Material(Shader.Find("Hidden/cropToTop"));
    }

    // Postprocess the image
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
        if (height == 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetFloat("_YOffset", height);
        Graphics.Blit(source, destination, material);
    }
}