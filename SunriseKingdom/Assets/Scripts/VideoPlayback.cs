using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using UnityEngine.UI;

public class VideoPlayback : MonoBehaviour {

    public float captureTime = 30f;
    public float captureTimeMax = 60f;
    public EmailThread emailSender;
    public MediaPlayer[] media;
    public Renderer[] rend;
    public ColorSampler sampler;
    
    [HideInInspector]
    public float videoLoadTime = 150f;
    [HideInInspector]
    public string videoFolder = "D:\\SunriseData/Videos/";
    [HideInInspector]
    public string imageFolder = "D:\\SunriseData/Images/";
    [HideInInspector]
    public bool beginPlayback = false;
    [HideInInspector]
    public bool emailActive = false;
    [HideInInspector]
    public bool debugActive = false;
    [HideInInspector]
    public bool screenshotEmailed = true;

    //controls
    public Slider playhead;
    [HideInInspector]
    public bool play = true;
    [HideInInspector]
    public bool stop = false;
    [HideInInspector]
    public bool pause = false;

    private float elapsedTime;
    private float elapsedTimeCapture;
    private int captureItem = 0;
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
            if (debugActive) Debug.Log("File paths were empty!  Will check again in 60 sec...");
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

            play = true;
            pause = false;
            stop = false;

            // toggle playback flag
            beginPlayback = true;

            screenshotEmailed = true;
        }
        else
        {
            // check every 60 seconds
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= timeExtension)
            {
                CheckForVideoFiles(VideoRecord.mostRecentRecording);
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
            if (item > VideoRecord.mostRecentRecording.Length-1) //  edited by JB
            {
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
        if (!media[0].Control.IsFinished() && (int)elapsedTime == videoLoadTime && !isLoaded[1])
        {
            LoadVideo(1, item, VideoRecord.mostRecentRecording);
        }
        else if (!media[1].Control.IsFinished() && (int)elapsedTime == videoLoadTime && !isLoaded[0])
        {
            LoadVideo(0, item, VideoRecord.mostRecentRecording);
        }

        //  capture/email sequence
        if (emailActive)
        {
            // on first playback, will send an email of all the screenshots
            // when the playhead of the last media in the list is 2.5 seconds before max duration
            // playhead is in milliseconds
            if (!screenshotEmailed && !emailSender.emailSent && playhead.value >= playhead.maxValue - 2500)
            {
                SendEmail();
            }
            else
            {
                elapsedTimeCapture += Time.deltaTime;

                // on first playback capture 1 screenshot for each video at a specific time
                if (!screenshotTaken && (int)elapsedTimeCapture == captureTime)
                {
                    // when the last email in the list is sent, disable toggle
                    if (itemAdjust(item) == VideoRecord.mostRecentRecording.Length - 1)
                    {
                        screenshotEmailed = false;
                    }

                    StartCoroutine(CaptureScreenshot(captureItem));
                    captureItem++;
                    screenshotTaken = true;
                }
                else if ((int)elapsedTimeCapture == captureTime + 1)
                {
                    screenshotTaken = false;
                }

                // reset timer
                if (elapsedTimeCapture >= captureTimeMax)
                    elapsedTimeCapture = 0f;
            }
        }
        else
        {
            elapsedTimeCapture = 0f;
            captureItem = 0;
        }
    }

    private void MediaControls()
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
        if (stop)
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

        // ui slider playhead controls
        bool selected = playhead.GetComponent<UIMouseDown>().selected;
        if (selected)
        {
            // seek to playhead location
            media[player].Control.Seek(playhead.value);
        }
        else
        {
            // value is media current time in ms
            playhead.value = media[player].Control.GetCurrentTimeMs();
        }

        // set max value of slider to media duration in ms
        playhead.maxValue = media[player].Info.GetDurationMs();
    }

    private int itemAdjust(int _item)
    {
        int itemCorrected = _item - 1;
        if (itemCorrected == -1)
        {
            itemCorrected = VideoRecord.mostRecentRecording.Length - 1;
        }
        return itemCorrected;
    }

    IEnumerator CaptureScreenshot(int _item)
    {
        screenshotTaken = false;

        yield return new WaitForEndOfFrame();

        string path = imageFolder + _item + ".jpg";

        Texture2D img = new Texture2D(Screen.width, Screen.height);

        // reads the screen pixels and applies to texture
        img.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        img.Apply();

        if(_item == (VideoRecord.mostRecentRecording.Length * 300) / captureTimeMax / 2)
        {
            Color col = sampler.saveNewColor(img);
            Debug.Log("Color saved at Item: " + _item + " was: " + col);
            int r = (int)(col.r * 255);
            int g = (int)(col.g * 255);
            int b = (int)(col.b * 255);
            string hex = r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
            Debug.Log("That's Hex String: " + hex);
            sampler.newestColorHex = hex;
        }
        // converts texture to JPG and writes to folder path
        byte[] bytes = img.EncodeToJPG();
        System.IO.File.WriteAllBytes(path, bytes);

        if (debugActive)
            Debug.Log("Screenshot " + _item + ".jpg has been saved!");
    }

    private void SendEmail()
    {
        //screenshotEmailed = true;
        //emailActive = false;

        //yield return new WaitForEndOfFrame();

        emailSender.videosLength = captureItem;
        
        if (emailSender.useThreading) emailSender.emailSent = true;
        else emailSender.SendEmail();

        if (debugActive)
            Debug.Log(emailSender.videosLength + " screenshots have been emailed!");

        emailActive = false;

        if (debugActive)
            Debug.Log("The email cycle is complete until next recording.");

        screenshotEmailed = true;
    }

    private void LoadVideo(int player, int _item, string[] _filePath)
    {
        string fileName = _filePath[_item];
        media[player].OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, fileName);
        media[player].m_AutoStart = false;
        isLoaded[player] = true;

        if (debugActive)
        {
            Debug.Log("Loading MediaPlayer " + player);
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
