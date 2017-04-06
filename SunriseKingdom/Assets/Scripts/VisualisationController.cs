using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualisationController : MonoBehaviour {

    public Material mat;
    public string uniformArrayName;
    public Color[] colors;
    public int numDays;
	// Use this for initialization
	void Start () {
        //for(int i = 0; i < colors.Length; i++)
        //{
        //    colors[i] = new Color(0, 0, 0, 0);
        //}
	}
	
	// Update is called once per frame
	void Update () {
        if (mat != null)
        {
            mat.SetColorArray(uniformArrayName, colors);
            mat.SetInt("days", numDays);
        }
    }
}
