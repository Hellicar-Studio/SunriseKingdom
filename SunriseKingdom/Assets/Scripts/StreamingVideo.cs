using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StreamingVideo : MonoBehaviour {

    public string videoURL = "http://clips.vorwaerts-gmbh.de/big_buck_bunny.ogv";
    public string saveToFolder = "Images";
    public bool saveAsScreenshot = false;
    public bool saveAsBytes = false;

    private Renderer rend;
    private MovieTexture tex;
    private int frame = 0;
    private string fileId;

    void Start() 
    {
        // reference components
        rend = GetComponent<Renderer>();
        // grab video stream and send to texture
        //StartCoroutine(PlayVideo());
    }

    void Update() 
    {
        //if (tex.isPlaying)
        //{
        //    if (saveAsScreenshot) Screenshot();
        //    if (saveAsBytes) SaveToPNG();
        //}
        //else
        //{
        //    if (frame != 0) frame = 0;
        //}

        if (saveAsScreenshot) Screenshot();
        else
        {
            if (frame != 0) frame = 0;
        }
    }

    IEnumerator PlayVideo()
    {
        // fetches video stream and converts to a MovieTexture
        var www = new WWW (videoURL);
        tex = www.movie;

        while (!tex.isReadyToPlay)
            yield return tex;

        // plays MovieTexture
        tex.Play();
        // sharpen the pixels
        //tex.filterMode = FilterMode.Point;

        // update renderer texture with MovieTexture
        rend.material.mainTexture = tex;
    }  

    // very slow loses frames boo...
    void SaveToPNG()
    {
        // creates a new RenderTexture that will be used for data conversion
        RenderTexture rText = new RenderTexture(Screen.width, Screen.height, 24);

        // must convert the MovieTexture to a RenderTexture
        // so it can be read by Texture2D ...
        Graphics.Blit(tex, rText);

        // creates a Texture2D of the size of the RenderTexture, RGB24 format
        int width = rText.width;
        int height = rText.height;
        Texture2D text = new Texture2D(width, height, TextureFormat.RGB24, false);

        // captures the RenderTexture pixels and applies to Texture2D
        text.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        text.Apply();

        // encodes Texture2D into PNG
        byte[] bytes = text.EncodeToPNG();
        //Object.Destroy(text);

        // writes a file for each frame a sub folder of the project folder
        File.WriteAllBytes(Application.dataPath + "/../" + saveToFolder + "/image_" + FileNamePadding(frame) + ".png", bytes);
        // advance to the next file
        frame++;
    }

    // much faster super awesome yes yes!
    void Screenshot()
    {
        Application.CaptureScreenshot(saveToFolder + "/image_" + FileNamePadding(frame) + ".png");
        frame++;
    }

    // based on the frame number, string is padded with zeros
    // this is to keep frames in correct sequential order
    private string FileNamePadding(int _frame)
    {
        string s;

        if (_frame <= 9)
        {
            s = "000" + _frame;
        }
        else if (_frame >= 10 && _frame <= 99)
        {
            s = "00" + _frame;
        }
        else if (_frame >= 100 && _frame <= 999)
        {
            s = "0" + _frame;
        }
        else
        {
            s = "" + _frame;
        }

        return s;
    }
}