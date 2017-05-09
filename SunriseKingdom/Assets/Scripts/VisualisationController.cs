using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualisationController : MonoBehaviour {

    public Material mat;
    ColorSampler sampler;
    public bool drawing;
    float time;

    Texture2D tex;

    public float map(float value, float low1, float high1, float low2, float high2)
    {
        return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
    }

	// Use this for initialization
	void Start () {
        Application.targetFrameRate = 60;

        if (!sampler)
            sampler = FindObjectOfType<ColorSampler>();

        drawing = false;
        time = 0;
        //colors = new Color[365];
        //float bonus = 0.1f;

        //float RMin = 0.7f + bonus;
        //float RMax = 0.9f + bonus;
        //float GMin = 0.32f + bonus;
        //float GMax = 0.6f + bonus;
        //float BMin = 0.0f + bonus;
        //float BMax = 0.39f + bonus;

        //for (int j = 0; j < colors.Length; j++)
        //{
        //    colors[j] = new Color(Random.Range(RMin, RMax), Random.Range(GMin, GMax), Random.Range(BMin, BMax), 1);
        //}

        //StartCoroutine(appendNewColors());
    }

    // Update is called once per frame
    void Update () {
        if(drawing)
        {
            time += Time.deltaTime;
        }
    }

    public void startDrawing()
    {
        drawing = true;
        time = 0;
    }

    public void stopDrawing()
    {
        drawing = false;
        time = 0;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(drawing)
        {
            mat.SetColorArray("Colors", sampler.colors);
            mat.SetFloatArray("Times", sampler.times);
            mat.SetFloat("Time", time);

            Graphics.Blit(source, destination, mat);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
