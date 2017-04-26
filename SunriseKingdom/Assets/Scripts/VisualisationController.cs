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

<<<<<<< HEAD
    public GameObject player;
	// Use this for initialization
	void Start () {
=======
    public float pressed;

    public float map(float value, float low1, float high1, float low2, float high2)
    {
        return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
    }

	// Use this for initialization
	void Start () {
        Application.targetFrameRate = 60;
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
        colors = new Color[5][];
        for(int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color[365];
<<<<<<< HEAD
            float bonus = 0.1f;//(float)i *0.3f;
            //if (i == 0)
            //    bonus = -0.2f;
            //else
            //    bonus = 0.2f;
            //
=======
            float bonus = 0.1f;

>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
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
<<<<<<< HEAD
=======

        pressed = 0.0f;
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
    }

    // Update is called once per frame
    void Update () {
<<<<<<< HEAD
        Debug.Log("Step");
=======
        //Debug.Log("Step");
        if (Input.GetMouseButtonDown(0))
        {
            pressed = 1.0f;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            pressed = 0.0f;
        }
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            mat.SetColorArray(uniformArrayName + (i + 1).ToString(), colors[i]);
        }
<<<<<<< HEAD
        mat.SetInt("days", days);
        mat.SetInt("shotsPerDay", colors.Length - 1);
=======
        Debug.Log(colors[0][0].r);
        mat.SetInt("days", days);
        mat.SetFloat("size", map(days, 1, 365, 1, 0));
        mat.SetInt("shotsPerDay", colors.Length - 1);

        Vector3 mouseStatus = new Vector3(Input.mousePosition.x, Input.mousePosition.y, pressed);
        mat.SetVector("_Mouse", mouseStatus);

>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
        Graphics.Blit(source, destination, mat);
    }
}
