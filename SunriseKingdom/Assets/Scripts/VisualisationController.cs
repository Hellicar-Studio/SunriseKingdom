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

    public float pressed;

    public float map(float value, float low1, float high1, float low2, float high2)
    {
        return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
    }

	// Use this for initialization
	void Start () {
        Application.targetFrameRate = 60;
        colors = new Color[5][];
        for(int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color[365];
            float bonus = 0.1f;

            float RMin = 0.7f + bonus;
            float RMax = 0.9f + bonus;
            float GMin = 0.32f + bonus;
            float GMax = 0.6f + bonus;
            float BMin = 0.0f + bonus;
            float BMax = 0.39f + bonus;

            for (int j = 0; j < colors[i].Length; j++)
            {
                colors[i][j] = Color.black;//new Color(Random.Range(RMin, RMax), Random.Range(GMin, GMax), Random.Range(BMin, BMax), 1);
            }
        }

        StartCoroutine(appendNewColors());

        pressed = 0.0f;
    }

    public IEnumerator appendNewColors()
    {
        // for each of the images in the images folder...
        for (int i = 0; i < 1; i++)
        {
            string filePath = "file:///" + Application.streamingAssetsPath + "/Images3" + ".png";
            WWW localFile = new WWW(filePath);

            yield return localFile;

            Texture2D tex = localFile.texture;

            Color[] pix = tex.GetPixels(tex.width / 2 - 50, tex.height / 2 - 50, 100, 100);

            Color avg = Color.black;
            int step = 10;
            for(int j = 0; j < pix.Length; j += step)
            {
                avg += pix[j] * step/pix.Length;
            }

            colors[0][0] = avg;
        }

    }

    // Update is called once per frame
    void Update () {
        //Debug.Log("Step");
        if (Input.GetMouseButtonDown(0))
        {
            pressed = 1.0f;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            pressed = 0.0f;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            mat.SetColorArray(uniformArrayName + (i + 1).ToString(), colors[i]);
        }
        mat.SetInt("days", days);
        mat.SetFloat("size", map(days, 1, 365, 1, 0));
        mat.SetInt("shotsPerDay", colors.Length - 1);

        Vector3 mouseStatus = new Vector3(Input.mousePosition.x, Input.mousePosition.y, pressed);
        mat.SetVector("_Mouse", mouseStatus);

        Graphics.Blit(source, destination, mat);
    }
}
