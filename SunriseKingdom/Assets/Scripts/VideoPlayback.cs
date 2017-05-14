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
    public GameController controller;
    
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
            // on first playback we'll take a screenshot every "Capture Time Max" seconds.
            // To do this we need to identify that we are on a first playback and how much time has elasped since that playback started.
            // To identify first playback, we'l use the "email active" field.
            // We'll add the delta time to the elapsed time capture time.
            elapsedTimeCapture += Time.deltaTime;

            // If our elapsed capture time equals out captureTimeMax then we shoudl take a screenshot! the problem with this is that it will take too many screenshots so we need to set
            // Another bool so we only take one.
            if((int)elapsedTimeCapture % captureTimeMax == 0 && !screenshotTaken)
            {
                Debug.Log("Taking Screenshot! Number: " + captureItem);
                StartCoroutine(CaptureScreenshot(captureItem));
                captureItem++;
                screenshotTaken = true;
            }
            // once we've taken out screenshot, we want to set our screenshot taken bool back to false;
            if((int)elapsedTimeCapture % captureTimeMax != 0)
            {
                screenshotTaken = false;
            }

            // If we've made it to the end of all the recordings (assuming 5 minutes for each recording) then we should send the email and turn off the emailing and screenshotting system.
            if ((int)elapsedTimeCapture > VideoRecord.mostRecentRecording.Length * 300)
            {
                Debug.Log("Sending Email!");
                controller.setEmailBody();
                SendEmail();
                elapsedTimeCapture = 0;
                captureItem = 0;
                screenshotTaken = false;
                emailActive = false;
            }

            //// on first playback, will send an email of all the screenshots
            //// when the playhead of the last media in the list is 2.5 seconds before max duration
            //// playhead is in milliseconds
            //Debug.Log("Checking if email should be sent:");
            //Debug.Log(" screenshotEmailed: " + screenshotEmailed);
            //Debug.Log(" emailSender.emailSent: " + emailSender.emailSent);
            //Debug.Log(" screenshotEmailed: " + screenshotEmailed);
            //Debug.Log(" Media Player is Finished: " + media[player].Control.IsFinished());
            //Debug.Log(" Playhead value is near the end: " + (playhead.value >= playhead.maxValue - 2500));

            //if (!screenshotEmailed && !emailSender.emailSent && media[player].Control.IsFinished()/*playhead.value >= playhead.maxValue - 2500*/)
            //{
            //    Debug.Log("Email is being sent!");
            //    SendEmail();
            //}
            //else
            //{
            //    Debug.Log("Email is not being sent");
            //    elapsedTimeCapture += Time.deltaTime;

            //    Debug.Log("Checking isf we should take a screenshot: ");
            //    Debug.Log(" Screenshot Take: " + screenshotTaken);
            //    Debug.Log(" elapsedTimeCapture: " + (int)elapsedTimeCapture);
            //    Debug.Log(" captureTime: " + captureTime);
            //    Debug.Log(" elapsedTimeCapture == captureTime: " + ((int)elapsedTimeCapture == captureTime));

            //    // on first playback capture 1 screenshot for each video at a specific time
            //    if (!screenshotTaken && (int)elapsedTimeCapture == captureTime)
            //    {
            //        Debug.Log("We're taking a screenshot!");
            //        Debug.Log("Checking if the last email in the list has been sent:");
            //        Debug.Log("item: " + item);
            //        Debug.Log("itemAdjust: " + itemAdjust(item));
            //        Debug.Log("itemAdjust == mostRecentRecording.Length - 1: " + (itemAdjust(item) == VideoRecord.mostRecentRecording.Length - 1));

            //        // when the last email in the list is sent, disable toggle
            //        if (itemAdjust(item) == VideoRecord.mostRecentRecording.Length - 1)
            //        {
            //            Debug.Log("The last screenshot has been taken!");
            //            screenshotEmailed = false;
            //        }

            //        Debug.Log("Start captureing the screenshot for capture item: " + captureItem);
            //        StartCoroutine(CaptureScreenshot(captureItem));
            //        captureItem++;
            //        screenshotTaken = true;
            //    }
            //    else if ((int)elapsedTimeCapture == captureTime + 1)
            //    {
            //        screenshotTaken = false;
            //    }

            //    // reset timer
            //    if (elapsedTimeCapture >= captureTimeMax)
            //        elapsedTimeCapture = 0f;
            //}
        }
        //else
        //{
        //    elapsedTimeCapture = 0f;
        //    captureItem = 0;
        //}
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

        if(_item == (int)(VideoRecord.mostRecentRecording.Length * 300) / captureTimeMax / 2)
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

        Destroy(img);

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
