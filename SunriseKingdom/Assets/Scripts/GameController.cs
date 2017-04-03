using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	
    public SunriseController sunrise;
    public VideoController video;
    public ImageController image;
    public MediaPlayerCtrl liveStream;

    public string videoURL = "rtsp://88.97.57.25:10001/axis-media/media.amp?videocodec=h264";
    public string APIKey = "7f09e7d718a5c1dd8d39f1635ac7f006";
    public string city = "London";
    public string checkSunriseTime = "03:00";
    public string dataFolder = "Images";
    public float framesPerSecond = 25f;
    public float recordingMaxSeconds = 3600f;
    public bool debugActive = true;
    public bool simulationMode = false;
    public bool manualRecord = false;

    private bool isSunriseActive = false;
    private float elapsedTime = 0f;

    // Use this for initialization
    void Start () 
    {
        Time.captureFramerate = (int)framesPerSecond;

        // sunrise startup
        sunrise.APIKey = APIKey;
        sunrise.city = city;
        sunrise.GetSunriseTime();

        // setup debug info
        sunrise.debugActive = debugActive;
        image.debugActive = debugActive;

        // setup video stream url
        liveStream.m_strFileName = videoURL;

        // sync seconds
        video.recordingMaxSeconds = recordingMaxSeconds;
    }

	// Update is called once per frame
	void Update () 
    {
        SunSystem();
        Capture();
        Playback();

        // on escape key up, unload media and quit app
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            liveStream.UnLoad();
        }

        if (manualRecord || isSunriseActive)
        {
            // count up seconds
            elapsedTime += Time.deltaTime;

            // if elapsed time is greater than 1hr
            if (elapsedTime >= recordingMaxSeconds)
            {
                if (debugActive)
                {
                    Debug.Log("Recording complete!");
                    Debug.Log("Elapsed time in seconds " + elapsedTime);
                    Debug.Log("Manual Recording Active " + manualRecord);
                }

                // disable
                elapsedTime = 0f;
                manualRecord = false;
                sunrise.isActive = false;
            }
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

        sunrise.GetSunriseStatus();

        if (!simulationMode)
        {
            isSunriseActive = sunrise.isActive;
        }
        else
        {
            isSunriseActive = manualRecord;
        }
    }

    private void Capture()
    {
        if (!simulationMode)
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
        else
        {
            if (manualRecord)
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
    }

    private void Playback()
    {
        if (!simulationMode)
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
        else
        {
            if (manualRecord)
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
}