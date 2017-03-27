using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	
    public VideoController video;
    public ImageController image;
    public SunriseController sunrise;
    public MediaPlayerCtrl scrMedia;

    public string videoURL = "rtsp://88.97.57.25:10001/axis-media/media.amp?videocodec=h264";
    public string APIKey = "7f09e7d718a5c1dd8d39f1635ac7f006";
    public string city = "London";
    public string checkSunriseTime = "03:00";
    public string dataFolder = "Images";
    public float framesPerSecond = 25f;
    public bool debugActive = true;

    private bool isSunriseActive = false;

    // Use this for initialization
    void Start () 
    {
        // setup debug info
        sunrise.debugActive = debugActive;
        image.debugActive = debugActive;

        // setup video stream url
        scrMedia.m_strFileName = videoURL;

        // sunrise startup
        sunrise.APIKey = APIKey;
        sunrise.city = city;
        sunrise.GetSunriseTime();
    }

	// Update is called once per frame
	void Update () 
    {
        SunSystem();
        VideoSystem();
        ImageSystem();

        // on escape key up, unload media and quit app
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            scrMedia.UnLoad();
        }
	}

    private void SunSystem()
    {
        if (sunrise.GetLocalTime() == checkSunriseTime)
        {
            sunrise.GetSunriseTime();
        }
        else
        {
            // reset switch
            if (sunrise.isUpdateTime)
                sunrise.isUpdateTime = false;
        }

        isSunriseActive = sunrise.GetSunriseStatus();
    }

    private void VideoSystem()
    {
        if (isSunriseActive)
        {
            if (!video.isFolderClear)
            {
                video.ClearFolder(dataFolder);
            }
            else
            {
                video.VideoRecord(dataFolder, framesPerSecond);
                video.RenderMaterial(true);
            }
        }
        else
        {
            if (video.isFolderClear) 
                video.isFolderClear = false;

            video.RenderMaterial(false);
        }
    }

    private void ImageSystem()
    {
        if (isSunriseActive)
        {
            if (image.isLoaded)
                image.isLoaded = false;

            image.RenderMaterial(false);
        }
        else
        {
            if (!image.isLoaded)
            {
                image.LoadImages(dataFolder);
            }
            else
            {
                image.PlayImages(framesPerSecond);
            }

            image.RenderMaterial(true);
        }
    }
}