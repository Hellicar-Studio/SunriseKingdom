using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualisationController : MonoBehaviour {

    public Material mat;
    public string uniformArrayName;
    public Color[][] colors;
    [Range(0, 364)]
    public int days;
    [Range(0, 12)]
    public int shotsPerDay;

    public GameObject player;
	// Use this for initialization
	void Start () {
        colors = new Color[5][];
        for(int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color[365];
            float bonus = 0.1f;//(float)i *0.3f;
            //if (i == 0)
            //    bonus = -0.2f;
            //else
            //    bonus = 0.2f;
            //
            float RMin = 0.7f + bonus;
            float RMax = 0.9f + bonus;
            float GMin = 0.32f + bonus;
            float GMax = 0.6f + bonus;
            float BMin = 0.0f + bonus;
            float BMax = 0.39f + bonus;

            for (int j = 0; j < colors[i].Length; j++)
            {
                colors[i][j] = new Color(Random.Range(RMin, RMax), Random.Range(GMin, GMax), Random.Range(BMin, BMax), 1);
            }
        }
    }

    // Update is called once per frame
    void Update () {
        Debug.Log("Step");
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            mat.SetColorArray(uniformArrayName + (i + 1).ToString(), colors[i]);
        }
        mat.SetInt("days", days);
        mat.SetInt("shotsPerDay", colors.Length - 1);
        Graphics.Blit(source, destination, mat);
    }
}
