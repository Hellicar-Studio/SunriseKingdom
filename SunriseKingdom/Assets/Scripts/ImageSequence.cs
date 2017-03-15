using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ImageSequence : MonoBehaviour {

    public string loadFromFolder = "Images";
    public float framesPerSecond = 10.0f;

    private Renderer rend;
    private Texture2D[] frames;
    private string[] files;
    private int frame = 0;

	// Use this for initialization
	void Start ()
    {
        // reference components
        rend = GetComponent<Renderer>();
        // get file paths
        files = Directory.GetFiles(loadFromFolder, "*.png");
        // process images into a sequence
        StartCoroutine(LoadImages());
	}

    private IEnumerator LoadImages()
    {
        //load all images in folder as textures
        frames = new Texture2D[files.Length];

        frame = 0;
        Texture2D texTmp = new Texture2D(1424, 220, TextureFormat.RGB24, false);
        texTmp.filterMode = FilterMode.Point;

        foreach (string tstring in files)
        {
            string pathTemp = "file://" + Application.dataPath + "/../" + tstring;
            WWW www = new WWW(pathTemp);
            yield return www;
            www.LoadImageIntoTexture(texTmp);

            frames[frame] = texTmp;
            frames[frame].filterMode = FilterMode.Point;

            rend.material.SetTexture("_MainTex", texTmp);
            frame++;
        }
    }
}