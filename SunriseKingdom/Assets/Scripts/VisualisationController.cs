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
	// Use this for initialization
	void Start () {
        colors = new Color[3][];
        for(int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color[365];
            for(int j = 0; j < colors[i].Length; j++)
            {
                colors[i][j] = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1);
            }
        }
    }

    // Update is called once per frame
    void Update () {
        if (mat != null)
        {
            for(int i = 0; i < colors.Length; i++)
            {
                mat.SetColorArray(uniformArrayName + (i+1).ToString(), colors[i]);
            }
            mat.SetInt("days", days);
            mat.SetInt("shotsPerDay", shotsPerDay);
        }
    }
}
