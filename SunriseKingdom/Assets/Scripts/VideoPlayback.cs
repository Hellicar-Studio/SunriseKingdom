using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using UnityEngine.UI;

public class VideoPlayback : MonoBehaviour {

    public EmailThread emailSender;
    public MediaPlayer[] media;
    public Renderer[] rend;

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
<<<<<<< HEAD
    [HideInInspector]
    public bool screenshotEmailed = true;

    //controls
    public Slider playhead;
=======

    public Slider playhead;

    //controls
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
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
<<<<<<< HEAD
=======
    private bool screenshotEmailed = false;
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed

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
<<<<<<< HEAD

            screenshotEmailed = true;
=======
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
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

        // if active, a cycles of saving screenshots and emails occur after recording stops
        if (emailActive)
        {
            // on first playback capture 1 screenshot for each video at a specific time
            if (!screenshotTaken && (int)elapsedTime == videoLoadTime + 10)
            {
                CaptureScreenshot();
            }
            else if ((int)elapsedTime != videoLoadTime + 10)
            {
                screenshotTaken = false;
            }

<<<<<<< HEAD
            // on first playback, will send an email of all the screenshots
            // 20 seconds after the last capture is taken
=======
            // on first playback send emails of the screenshots 10 seconds at the capture is taken
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
            if (!screenshotEmailed && !emailSender.emailSent && (int)elapsedTime == videoLoadTime + 20)
            {
                SendEmail();
            }
<<<<<<< HEAD
=======
            else if (screenshotEmailed && (int)elapsedTime != videoLoadTime + 20)
            {
                screenshotEmailed = false;
            }
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
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

    private void CaptureScreenshot()
    {
        Application.CaptureScreenshot(imageFolder + itemAdjust(item) + ".png");

        if (debugActive)
            Debug.Log("Screenshot " + itemAdjust(item) + ".png has been saved!");

<<<<<<< HEAD
        // when the last email in the list is sent, disable toggle
        if (itemAdjust(item) == VideoRecord.mostRecentRecording.Length - 1)
        {
            screenshotEmailed = false;
        }

=======
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
        screenshotTaken = true;
    }

    private void SendEmail()
    {
<<<<<<< HEAD
        // send the video file amount to the email sender's attachment array
        emailSender.videosLength = VideoRecord.mostRecentRecording.Length;

        //emailSender.item = itemAdjust(item);
=======
        emailSender.item = itemAdjust(item);
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
        if (emailSender.useThreading) emailSender.emailSent = true;
        else emailSender.SendEmail();

        if (debugActive)
<<<<<<< HEAD
            Debug.Log(emailSender.videosLength + " screenshots have been emailed!");

        emailActive = false;

        if (debugActive)
            Debug.Log("The email cycle is complete until next recording.");
=======
            Debug.Log("Screenshot " + emailSender.item + ".png has been emailed!");

        // when the last email in the list is sent, disable toggle
        if (itemAdjust(item) == VideoRecord.mostRecentRecording.Length - 1)
        {
            emailActive = false;

            if (debugActive)
                Debug.Log("The email cycle is complete until next recording.");
        }
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed

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
