using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ImageController : MonoBehaviour {

    [HideInInspector] 
    public bool isLoaded = false;
    [HideInInspector]
    public bool debugActive = false;

    private Renderer rend;
    private Texture2D texTmp;
    private string[] files;
    private float index = 0f;
    private float elapsedTime = 0f;
    private bool filesExist = false;

    // toggles object renderer
    public void RenderMaterial(bool _active)
    {
        // checks if rend is null and if yes, reference
        if (rend == null)
            rend = GetComponent<Renderer>();
        else
        {
            // if renderer is active
            if (_active)
            {
                // enable renderer if disabled
                if (!rend.enabled)
                    rend.enabled = true;
            }
            else
            {
                // disable renderer if enabled
                if (rend.enabled)
                    rend.enabled = false;
            }
        }
    }

    private void CheckForFiles(string _folderName)
    {
        DirectoryInfo di = new DirectoryInfo(_folderName);
        int fileCount = di.GetFiles().Length - 1;

        if (fileCount == 0)
        {
            filesExist = false;
        }
        else
        {
            filesExist = true;
        }

        if (debugActive) Debug.Log("Found " + fileCount + " files.");
    }

    public void LoadImages(string _folderName)
    {
        if (filesExist)
        {
            // get file paths
            files = Directory.GetFiles(_folderName, "*.png");

            texTmp = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texTmp.filterMode = FilterMode.Point;

            // reset index
            index = 0f;

            isLoaded = true;
        }
        else
        {
            // check every 60 seconds
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= 60)
            {
                CheckForFiles(_folderName);
                elapsedTime = 0f;
            }
        }

    }

    // loads images on the fly
    public void PlayImages(float _fps)
    {
        // frame index at the rate of frames per second
        index = Time.time * _fps;
        index = index % files.Length;

        // load latest 
        string pathTemp = "file://" + Application.dataPath + "/../" + files[(int)index];
        WWW www = new WWW(pathTemp);
        www.LoadImageIntoTexture(texTmp);

        // update renderer with latest frame
        rend.material.mainTexture = texTmp;

        // resets index to loop through
        if (index >= files.Length)
            index = 0;
    }
}