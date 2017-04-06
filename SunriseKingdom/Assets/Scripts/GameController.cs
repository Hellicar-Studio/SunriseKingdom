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
    public VideoStream videoStream;
    public ImagePlayback imagePlayback;
    public VideoPlayback videoPlayback;
    public VideoRecord videoRecord;
    public MediaPlayerCtrl liveStream;
    
    [Header("Sunrise Data")]
    public string APIKey = "7f09e7d718a5c1dd8d39f1635ac7f006";
    public string city = "London";
    public string checkSunriseTimeAt = "03:00";
    [Header("Live Stream")]
    public string videoURL = "rtsp://88.97.57.25:10001/axis-media/media.amp?videocodec=h264";
    //public float framesPerSecond = 25f;
    [Header("Data Folders")]
    public string videoFolder = "D:\\SunriseData/Images/";
    public string imagesFolder = "D:\\SunriseData/Images/";
    [Header("Recorder")]
    public string cameraIP = "192.168.1.201";
    public float recordingMaxSeconds = 3600f;
    //[Header("Playback")]
    //public PlaybackMode playbackMode = PlaybackMode.video;
    //[Header("Playback - Video")]
    //public int maxVideos = 12;
    public float loadAtSeconds = 150f;
    //public string fileExtension = ".mkv";
    [Header("Support")]
    public bool debugActive = true;
    public bool simulationMode = false;
    public bool manualRecord = false;

    private bool isSunriseActive = false;
    private float elapsedTime = 0f;

    // Use this for initialization
    void Start () 
    {
        // sunrise startup
        sunrise.APIKey = APIKey;
        sunrise.city = city;
        sunrise.GetSunriseTime();

        // setup debug info
        sunrise.debugActive = debugActive;
        videoRecord.debugActive = debugActive;
        //imagePlayback.debugActive = debugActive;
        videoPlayback.debugActive = debugActive;

        // setup video playback variables
        //videoPlayback.maxVideos = maxVideos;
        videoPlayback.loadAtSeconds = loadAtSeconds;
        //videoPlayback.fileExtension = fileExtension;
        videoPlayback.videoFolder = videoFolder;
        videoPlayback.imagesFolder = imagesFolder;

        // setup video recorder variables
        videoRecord.CamIP = cameraIP;
        videoRecord.recordingsRoot = videoFolder;
        videoRecord.maxVideos = (int)Mathf.Max(2, recordingMaxSeconds / 300.0f);

        // setup video stream url
        liveStream.m_strFileName = videoURL;

        // sync seconds
        videoStream.recordingMaxSeconds = recordingMaxSeconds;
    }

	// Update is called once per frame
	void Update () 
    {
        // main systems
        SunSystem();
        VideoStream();
        RecordVideo();
        PlaybackVideo();

        //if (playbackMode == PlaybackMode.image)
        //{
        //    PlaybackImage();
        //    // disables all video playback renderers
        //    videoPlayback.RenderMaterial(false);
        //}
        //else
        //{
        //    PlaybackVideo();
        //    // disables image playback renderer
        //    imagePlayback.RenderMaterial(false);
        //}

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
                    Debug.Log("Sunrise complete!");
                    Debug.Log("Elapsed time in seconds " + elapsedTime);
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

    private void RecordVideo()
    {
        if (!simulationMode)
        {
            if (isSunriseActive)
            {
                if (!videoRecord.isRecording)
                    videoRecord.StartRecording();
            }
            else
            {
                if (videoRecord.isRecording)
                    videoRecord.StopRecording();
            }
        }
        else
        {
            if (manualRecord)
            {
                if (!videoRecord.isRecording)
                    videoRecord.StartRecording();
            }
            else
            {
                if (videoRecord.isRecording)
                    videoRecord.StopRecording();
            }
        }
    }

    private void VideoStream()
    {
        if (!simulationMode)
        {
            if (isSunriseActive)
            {
                videoStream.RenderMaterial(true);

                //if (!videoStream.isFolderClear)
                //{
                //    videoStream.ClearFolder(videoFolder);
                //}
                //else
                //{
                //    videoStream.VideoRecord(videoFolder, framesPerSecond);
                //    videoStream.RenderMaterial(true);
                //}
            }
            else
            {
                //if (videoStream.isFolderClear)
                //    videoStream.isFolderClear = false;

                videoStream.RenderMaterial(false);
            }
        }
        else
        {
            if (manualRecord)
            {
                videoStream.RenderMaterial(true);

                //if (!videoStream.isFolderClear)
                //{
                //    videoStream.ClearFolder(videoFolder);
                //}
                //else
                //{
                //    videoStream.VideoRecord(videoFolder, framesPerSecond);
                //    videoStream.RenderMaterial(true);
                //}
            }
            else
            {
                //if (videoStream.isFolderClear)
                //    videoStream.isFolderClear = false;

                videoStream.RenderMaterial(false);
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

    //private void PlaybackImage()
    //{
    //    if (!simulationMode)
    //    {
    //        if (isSunriseActive)
    //        {
    //            if (imagePlayback.isLoaded)
    //                imagePlayback.isLoaded = false;

    //            imagePlayback.RenderMaterial(false);
    //        }
    //        else
    //        {
    //            if (!imagePlayback.isLoaded)
    //            {
    //                imagePlayback.LoadImages(imagesFolder);
    //            }
    //            else
    //            {
    //                imagePlayback.PlayImages(framesPerSecond);
    //            }

    //            imagePlayback.RenderMaterial(true);
    //        }
    //    }
    //    else
    //    {
    //        if (manualRecord)
    //        {
    //            if (imagePlayback.isLoaded)
    //                imagePlayback.isLoaded = false;

    //            imagePlayback.RenderMaterial(false);
    //        }
    //        else
    //        {
    //            if (!imagePlayback.isLoaded)
    //            {
    //                imagePlayback.LoadImages(imagesFolder);
    //            }
    //            else
    //            {
    //                imagePlayback.PlayImages(framesPerSecond);
    //            }

    //            imagePlayback.RenderMaterial(true);
    //        }
    //    }
    //}
}