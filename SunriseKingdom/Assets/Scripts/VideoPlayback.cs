//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
//using System.IO;

public class VideoPlayback : MonoBehaviour {

    public MediaPlayer[] media;
    public Renderer[] rend;
    [HideInInspector]
    public int maxVideos = 4;
    [HideInInspector]
    public float loadAtSeconds = 30f;
    //[HideInInspector]
    //public string fileExtension = ".mkv";
    [HideInInspector]
    public string videoFolder = "D:\\SunriseData/Images/";
    [HideInInspector]
    public string imagesFolder = "D:\\SunriseData/Images/";
    [HideInInspector]
    public bool beginPlayback = false;
    [HideInInspector]
    public bool firstPlayback = true;
    [HideInInspector]
    public bool debugActive = false;

    //controls
    public bool play = false;
    public bool stop = false;
    public bool pause = false;

    private float elapsedTime;
    private int item = 1;
    private bool[] isLoaded;
    private float timeExtension = 0f;
    private int extCount = 0;
    private bool filesExist = false;
    private bool screenshotTaken = false;

    // disables all object renderers
    public void RenderMaterial(bool _active)
    {
        // if renderer is active
        if (!_active)
        {
            // disable renderer if enabled
            if (rend[0].enabled) rend[0].enabled = false;
            if (rend[1].enabled) rend[1].enabled = false;
        }
    }

    private void CheckForVideoFiles(string[] _dataFolder)
    {
        if (debugActive) Debug.Log("Checking for video files...");


        if (_dataFolder == null)
        {
            if (debugActive) Debug.Log("File paths were empty!  Will check again in 60seconds.");
            filesExist = false;
            extCount++;

            if (extCount > 1)
            {
                timeExtension = 60f;
            }
            else
            {
                timeExtension = 0f;
            }
        }
        else
        {
            if (debugActive) Debug.Log("File paths are ready!");
            filesExist = true;
        }

        //DirectoryInfo di = new DirectoryInfo(_dataFolder);
        //int fileCount = di.GetFiles("*" + fileExtension).Length;

        //if (fileCount != maxVideos)
        //{
        //    filesExist = false;
        //    extCount++;

        //    if (extCount > 1)
        //    {
        //        timeExtension = 60f;
        //    }
        //    else
        //    {
        //        timeExtension = 0f;
        //    }

        //    if (debugActive) Debug.Log("Incorrect amount or no videos were found!");
        //}
        //else
        //{
        //    filesExist = true;
        //}

        //if (debugActive) Debug.Log("Videos files found " + fileCount + ".");
    }

    public void BeginPlayback()
    {
        if (filesExist)
        {
            // default variables
            item = 1;
            elapsedTime = 0f;
            isLoaded = new bool[media.Length];
            extCount = 0;

            // toggle renderers
            rend[0].enabled = true;
            rend[1].enabled = false;

            // load and playback media
            LoadVideo(0, 0, VideoRecord.mostRecentRecording);
            PlayVideo(0);

            // toggle playback flag
            beginPlayback = true;
        }
        else
        {
            // check every 60 seconds
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= timeExtension)
            {
                CheckForVideoFiles(VideoRecord.mostRecentRecording); // Application.streamingAssetsPath);
                elapsedTime = 0f;
            }
        }
    }

    public void UpdatePlayer()
    {
        MediaControls();

        elapsedTime += Time.deltaTime;

        // gets current player and checks if it's finished
        int player = (item + 1) % 2;
        if (media[player].Control.IsFinished())
        {
            if (debugActive)
                Debug.Log("Player " + player + " has ended playback!");

            // advance to the next video
            // reset loop if we reach the end
            item++;
            if (item > maxVideos - 1)
            {
                // disable screen capture
                firstPlayback = false;
                // reset items for looping
                item = 0;
            }

            if (item % 2 == 1)
            {
                rend[0].enabled = true;
                PlayVideo(0);

                rend[1].enabled = false;
                isLoaded[1] = false;
            }
            else if (item % 2 == 0)
            {
                rend[1].enabled = true;
                PlayVideo(1);

                rend[0].enabled = false;
                isLoaded[0] = false;
            }

            elapsedTime = 0f;
        }

        // loads the correct video for the right player
        if (!media[0].Control.IsFinished() && (int)elapsedTime == loadAtSeconds && !isLoaded[1])
        {
            LoadVideo(1, item, VideoRecord.mostRecentRecording);
        }
        else if (!media[1].Control.IsFinished() && (int)elapsedTime == loadAtSeconds && !isLoaded[0])
        {
            LoadVideo(0, item, VideoRecord.mostRecentRecording);
        }

        // on first playback capture 1 screenshot for each video at a specific time
        if (firstPlayback && !screenshotTaken && (int)elapsedTime == loadAtSeconds)
        {
            CaptureScreenshot();
        }
        else if ((int)elapsedTime != loadAtSeconds)
        {
            if (screenshotTaken) screenshotTaken = false;
        }

        // on first playback send emails of the screenshots 10 seconds at the capture is taken
        if (firstPlayback && !EmailThread.emailSent && (int)elapsedTime == loadAtSeconds + 10)
        {
            SendEmail();
        }
    }

    public void MediaControls()
    {
        int player = (item + 1) % 2;
        // play media
        if (!media[player].Control.IsPlaying() && play)
        {
            media[player].Control.Play();

            // reset toggles
            stop = false;
            pause = false;
        }

        // stop the system and reset
        if (media[player].Control.IsPlaying() && stop)
        {
            // reset system variables
            BeginPlayback();

            // restart paused
            pause = true;

            // reset toggles
            play = false;
            stop = false;
        }

        // pause the system
        if (media[player].Control.IsPlaying() && pause)
        {
            media[player].Control.Pause();

            // reset toggles
            stop = false;
            play = false;
        }
    }

    private void CaptureScreenshot()
    {
        Application.CaptureScreenshot(imagesFolder + item + ".png");

        if (debugActive)
            Debug.Log("Screenshot " + item + ".png has been saved!");

        screenshotTaken = true;
    }

    private void SendEmail()
    {
        int itemCorrected = item - 1;
        if (itemCorrected == -1)
        {
            itemCorrected = maxVideos - 1;
        }
        EmailThread.item = itemCorrected;
        if (!EmailThread.emailSent) EmailThread.emailSent = true;
    }

    private void LoadVideo(int player, int _item, string[] _filePath)
    {
        //int itemCorrected = _item + 1;
        //if (itemCorrected > maxVideos-1)
        //{
        //    itemCorrected = 0;
        //}

        string fileName = _filePath[(maxVideos-1) - _item]; //videoFolder + _item.ToString() + fileExtension;
        media[player].OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, fileName);
        media[player].m_AutoStart = false;
        isLoaded[player] = true;

        if (debugActive)
        {
            Debug.Log("Loading MediaPlayer " + player);
            //Debug.Log("Loading Video " + itemCorrected);
        }
    }

    private void PlayVideo(int _item)
    {
        int player = _item % 2;
        media[player].Play();

        if (debugActive)
        {
            Debug.Log("Playing MediaPlayer " + player);
        }
    }
}
