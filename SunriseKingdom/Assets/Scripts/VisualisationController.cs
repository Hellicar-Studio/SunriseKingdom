using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualisationController : MonoBehaviour {

    public Material mat;
    public string uniformArrayName;
    [HideInInspector]
    public Color[] colors;
    [Range(0, 364)]
    public int days;
    public Camera cam;

    public int step;

    public Vector2 samplePos;
    public Vector2 sampleSize;

    Texture2D tex;

    public float map(float value, float low1, float high1, float low2, float high2)
    {
        return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
    }

	// Use this for initialization
	void Start () {
        Application.targetFrameRate = 60;

        colors = new Color[365];
        float bonus = 0.1f;

        float RMin = 0.7f + bonus;
        float RMax = 0.9f + bonus;
        float GMin = 0.32f + bonus;
        float GMax = 0.6f + bonus;
        float BMin = 0.0f + bonus;
        float BMax = 0.39f + bonus;

        if(!cam)
        {
            cam = GetComponent<Camera>();
        }

        for (int j = 0; j < colors.Length; j++)
        {
            colors[j] = Color.black;//new Color(Random.Range(RMin, RMax), Random.Range(GMin, GMax), Random.Range(BMin, BMax), 1);
        }

        StartCoroutine(appendNewColors());
    }

    public IEnumerator appendNewColors()
    {
        string filePath = "file:///" + Application.streamingAssetsPath + "/Images0" + ".png";
        Debug.Log(filePath);
        WWW localFile = new WWW(filePath);

        yield return localFile;

        tex = localFile.texture;

        Color[] pix = tex.GetPixels(tex.width / 2 + (int)samplePos.x, tex.height / 2 + (int)samplePos.y, (int)sampleSize.x, (int)sampleSize.y);

        Color avg = Color.black;
        for(int j = 0; j < pix.Length; j += step)
        {
            avg += pix[j] * step/pix.Length;
        }

        for(int i = 0; i < colors.Length; i++)
        {
            colors[i] = avg;
        }
    }

    // Update is called once per frame
    void Update () {
        drawDebugRect(samplePos.x, samplePos.y, sampleSize.x, sampleSize.y);
    }

    private void drawDebugRect(float x, float y, float width, float height)
    {
        x *= cam.orthographicSize / (Screen.height/2);
        y *= cam.orthographicSize / (Screen.height / 2);
        width *= cam.orthographicSize / (Screen.height / 2);
        height *= cam.orthographicSize / (Screen.height / 2);

        Vector3 topLeft = new Vector3(x, y, 0);
        Vector3 topRight = new Vector3(x + width, y, 0);
        Vector3 bottomLeft = new Vector3(x, y + height, 0);
        Vector3 bottomRight = new Vector3(x+width, y+height, 0);

        Debug.DrawLine(topLeft, topRight, Color.red);
        Debug.DrawLine(topLeft, bottomLeft, colors[0]);
        Debug.DrawLine(topRight, bottomRight, colors[0]);
        Debug.DrawLine(bottomLeft, bottomRight, colors[0]);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            mat.SetColorArray(uniformArrayName, colors);
        }
        mat.SetInt("days", days);
        mat.SetFloat("size", map(days, 1, 365, 1, 0));
        mat.SetTexture("Texture", tex);

        Graphics.Blit(source, destination, mat);
    }
}
