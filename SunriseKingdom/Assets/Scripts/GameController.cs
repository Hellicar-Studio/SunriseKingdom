using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlaybackMode
{
    image,
    video
}

public class GameController : MonoBehaviour {
	
    public SunriseController sunrise;
    public VideoController video;
    public ImagePlayback imagePlayback;
    public VideoPlayback videoPlayback;
    public MediaPlayerCtrl liveStream;
    
    [Header("Sunrise Data")]
    public string APIKey = "7f09e7d718a5c1dd8d39f1635ac7f006";
    public string city = "London";
    public string checkSunriseTimeAt = "03:00";
    [Header("Live Stream")]
    public string videoURL = "rtsp://88.97.57.25:10001/axis-media/media.amp?videocodec=h264";
    public string dataFolder = "Images";
    public float framesPerSecond = 25f;
    public float recordingMaxSeconds = 3600f;
    [Header("Playback")]
    public PlaybackMode playbackMode = PlaybackMode.video;
    [Header("Playback - Video")]
    public int maxVideos = 4;
    public float cutAtSeconds = 60f;
    public float loadAtSeconds = 30f;
    public string fileExtension = ".mkv";
    [Header("Support")]
    public bool debugActive = true;
    public bool simulationMode = false;
    public bool manualRecord = false;

    private bool isSunriseActive = false;
    private float elapsedTime = 0f;

    // Use this for initialization
    void Start () 
    {
        //Time.captureFramerate = (int)framesPerSecond;

        // sunrise startup
        sunrise.APIKey = APIKey;
        sunrise.city = city;
        sunrise.GetSunriseTime();

        // setup debug info
        sunrise.debugActive = debugActive;
        imagePlayback.debugActive = debugActive;
        videoPlayback.debugActive = debugActive;

        // setup video playback variables
        videoPlayback.maxVideos = maxVideos;
        videoPlayback.cutAtSeconds = cutAtSeconds;
        videoPlayback.loadAtSeconds = loadAtSeconds;
        videoPlayback.fileExtension = fileExtension;

        // setup video stream url
        liveStream.m_strFileName = videoURL;

        // sync seconds
        video.recordingMaxSeconds = recordingMaxSeconds;
    }

	// Update is called once per frame
	void Update () 
    {
        SunSystem();
        CaptureStream();

        if (playbackMode == PlaybackMode.image)
        {
            PlaybackImage();
            // disables all video playback renderers
            videoPlayback.RenderMaterial(false);
        }
        else
        {
            PlaybackVideo();
            // disables image playback renderer
            imagePlayback.RenderMaterial(false);
        }

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
        if (sunrise.GetLocalTime() == checkSunriseTimeAt)
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

    private void CaptureStream()
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

    private void PlaybackVideo()
    {
        if (!simulationMode)
        {
            if (isSunriseActive)
            {
                if (videoPlayback.beginPlayback)
                    videoPlayback.beginPlayback = false;
            }
            else
            {
                if (!videoPlayback.beginPlayback)
                {
                    videoPlayback.BeginPlayback();
                }
                else
                {
                    videoPlayback.UpdatePlayer();
                }
            }
        }
        else
        {
            if (manualRecord)
            {
                if (videoPlayback.beginPlayback)
                    videoPlayback.beginPlayback = false;
            }
            else
            {
                if (!videoPlayback.beginPlayback)
                {
                    videoPlayback.BeginPlayback();
                }
                else
                {
                    videoPlayback.UpdatePlayer();
                }
            }
        }
    }

    private void PlaybackImage()
    {
        if (!simulationMode)
        {
            if (isSunriseActive)
            {
                if (imagePlayback.isLoaded)
                    imagePlayback.isLoaded = false;

                imagePlayback.RenderMaterial(false);
            }
            else
            {
                if (!imagePlayback.isLoaded)
                {
                    imagePlayback.LoadImages(dataFolder);
                }
                else
                {
                    imagePlayback.PlayImages(framesPerSecond);
                }

                imagePlayback.RenderMaterial(true);
            }
        }
        else
        {
            if (manualRecord)
            {
                if (imagePlayback.isLoaded)
                    imagePlayback.isLoaded = false;

                imagePlayback.RenderMaterial(false);
            }
            else
            {
                if (!imagePlayback.isLoaded)
                {
                    imagePlayback.LoadImages(dataFolder);
                }
                else
                {
                    imagePlayback.PlayImages(framesPerSecond);
                }

                imagePlayback.RenderMaterial(true);
            }
        }
    }
}