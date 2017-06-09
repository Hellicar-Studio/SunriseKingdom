using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    public GameObject uiCanvas;
    public UIController uiSettings;
    public SunriseController sunrise;
    public VideoPlayback videoPlayback;
    public VideoRecord videoRecord;
    public EmailThread emailSender;
    public Transform playbackTransform;
    public ColorSampler sampler;
    public VisualisationController visController;
    [Header("Support")]
    public bool manualRecord = false;
    public bool runFirstRun = false;
    [Header("Email Debug")]
    public bool emailActive = false;
    public bool useThreading = false;

    private bool isSunriseActive = false;
    private bool isUIActive = true;
    private float elapsedTime = 0f;
    private float fpsTime = 0f;

    // Use this for initialization
    void Start () 
    {
        Screen.SetResolution(3840, 600, true);

        // toggles
        emailSender.useThreading = useThreading;
        videoPlayback.emailActive = emailActive;

        // enables ui canvas on launch
        uiCanvas.SetActive(isUIActive);

        // if this is the first time the app is run
        if (PlayerPrefs.GetInt("isFirstRun") == 0 || runFirstRun)
        {
            if (uiSettings._debugActive)
            Debug.Log("FirstRun sequence is currently running...");

            // resets the ui settings to default
            uiSettings.DefaultSettings();

            // save the ui settings
            uiSettings.SaveSettings();

            // switch toggle
            PlayerPrefs.SetInt("isFirstRun", 1);
        }
        else
        {
            // load in saved settings
            uiSettings.LoadSettings();
        }

        // Find the color sampler if there isn't one.
        if (!sampler)
            sampler = FindObjectOfType<ColorSampler>();

        // updates system variables
        UpdateSystemSettings();
    }

    void UpdateSystemSettings()
    {
        // sunrise startup
        sunrise.apiKey = uiSettings._apiKey.text;
        sunrise.city = uiSettings._city.text;
        double result;
        bool parsed = double.TryParse(uiSettings._minuteOffset.text, out result);
        if (parsed)
            sunrise.minuteOffset = result;
        else
            sunrise.minuteOffset = -30;
        sunrise.GetSunriseTime();

        // setup debug info
        sunrise.debugActive = uiSettings._debugActive.isOn;
        videoRecord.debugActive = uiSettings._debugActive.isOn;
        videoPlayback.debugActive = uiSettings._debugActive.isOn;
        emailSender.debugActive = uiSettings._debugActive.isOn;

        // folder setup
        videoPlayback.videoFolder = uiSettings._videoFolder.text;
        videoPlayback.imageFolder = uiSettings._imageFolder.text;
        emailSender.imagesFolder = uiSettings._imageFolder.text;

        // setup video playback variables
        float videoLoadTime;
        float.TryParse(uiSettings._videoLoadTime.text, out videoLoadTime);
        if (parsed)
            videoPlayback.videoLoadTime = videoLoadTime;
        else
            videoPlayback.videoLoadTime = 150; // default value

        // capture settings
        float captureTime;
        float.TryParse(uiSettings._captureTime.text, out captureTime);
        if (parsed)
            videoPlayback.captureTime = captureTime;
        else
            videoPlayback.captureTime = 30f; // default value

        float captureTimeMax;
        float.TryParse(uiSettings._captureTimeMax.text, out captureTimeMax);
        if (parsed)
            videoPlayback.captureTimeMax = captureTimeMax;
        else
            videoPlayback.captureTimeMax = 60f; // default value

        // setup video recorder variables
        videoRecord.CamIP = uiSettings._cameraIP.text;
        videoRecord.recordingsRoot = uiSettings._videoFolder.text;
        float duration;
        parsed = float.TryParse(uiSettings._recordingDuration.text, out duration);
        if (!parsed)
            duration = 3600;
        videoRecord.maxVideos = (int)Mathf.Max(2, duration / 300.0f);

        // setup email settings
        emailSender.emailAccount = uiSettings._emailAccount.text;
        emailSender.emailPassword = uiSettings._emailPassword.text;
        emailSender.serverSMTP = uiSettings._serverSMTP.text;
        int port;
        parsed = int.TryParse(uiSettings._portSMTP.text, out port);
        if (!parsed)
            port = 587;
        emailSender.portSMTP = 587;// port;
        emailSender.emailRecipient = uiSettings._emailRecipient.text;
    }

    void UpdateUIText()
    {
        if (uiSettings._debugActive.isOn)
        {
            Debug.Log("Updating UI textfields...");
        }

        // sunrise data
        uiSettings.apiKey.text = "API Key: " + sunrise.apiKey;
        uiSettings.city.text = "City: " + sunrise.city;
        uiSettings.country.text = "Country: " + sunrise.country;
        uiSettings.latlon.text = "Lat/Lon: " + sunrise.lat + " / " + sunrise.lon;
        uiSettings.minuteOffset.text = "Minute Offset: " + sunrise.minuteOffset;
        uiSettings.sunriseTimeCur.text = "Sunrise Time Current: " + sunrise.sunriseTime;
        uiSettings.sunriseTimeCheck.text = "Sunrise Time Check: " + uiSettings._sunriseTimeCheck.text;

        // recording settings
        uiSettings.cameraIP.text = "Camera IP: " + videoRecord.CamIP;
        uiSettings.recordingDuration.text = "Recording Duration: " + uiSettings._recordingDuration.text;

        // playback settings
        uiSettings.imageFolder.text = "Image Folder: " + videoPlayback.imageFolder;
        uiSettings.videoFolder.text = "Video Folder: " + videoPlayback.videoFolder;
        uiSettings.videoLoadTime.text = "Video Load Time: " + videoPlayback.videoLoadTime.ToString();
        uiSettings.captureTime.text = "Capture Time: " + videoPlayback.captureTime.ToString();
        uiSettings.captureTimeMax.text = "Capture Time Max: " + videoPlayback.captureTimeMax.ToString();

        // email sender
        uiSettings.emailAccount.text = "Account: " + emailSender.emailAccount;
        uiSettings.emailPassword.text = "Password: " + emailSender.emailPassword;
        uiSettings.serverSMTP.text = "Server: " + emailSender.serverSMTP;
        uiSettings.portSMTP.text = "Port: " + emailSender.portSMTP.ToString();
        uiSettings.emailRecipient.text = "Recipient: " + emailSender.emailRecipient;
    }

    void UpdateUI()
    {
        // system data
        uiSettings.currentDate.text = "Current Date: " + sunrise.GetCurrentDate();
        uiSettings.currentTime.text = "Current Time: " + sunrise.GetLocalTime();

        // calculates the fps
        fpsTime += (Time.deltaTime - fpsTime) * 0.1f;
        float fps = 1.0f / fpsTime;
        uiSettings.fps.text = "FPS: " + fps.ToString();

        // video playback window position
        float x = uiSettings._xPosition.value;
        float y = uiSettings._yPosition.value;
        playbackTransform.position = new Vector3(x, y, 0f);

        // video playback window scale
        float scale = uiSettings._scale.value;
        playbackTransform.localScale = new Vector3(scale, scale, 1f);

        // capture active media player button states
        bool play = uiSettings._mediaPlay.GetComponent<UIMouseDown>().selected;
        bool pause = uiSettings._mediaPause.GetComponent<UIMouseDown>().selected;
        bool stop = uiSettings._mediaStop.GetComponent<UIMouseDown>().selected;

        if (play) videoPlayback.play = true;
        if (pause) videoPlayback.pause = true;
        if (stop) videoPlayback.stop = true;

        uiSettings._mediaPlay.isOn = videoPlayback.play;
        uiSettings._mediaPause.isOn = videoPlayback.pause;
        uiSettings._mediaStop.isOn = videoPlayback.stop;

        // handles the manual record button
        bool mRecord = uiSettings._manualRecord.GetComponent<UIMouseDown>().selected;
        if (mRecord)
        {
            manualRecord = true;
        }
        else
        {
            uiSettings._manualRecord.isOn = manualRecord;
        }

        // resets the settings to default
        bool resetSelected = uiSettings._resetSystem.GetComponent<UIMouseDown>().selected;
        if (resetSelected)
        {
            // resets the ui settings to default
            uiSettings.DefaultSettings();

            // save the ui settings
            uiSettings.SaveSettings();

            UpdateSystemSettings();
        }
        else
        {
            uiSettings._resetSystem.isOn = false;
        }
    }

    // Update is called once per frame
    void Update () 
    {
        // main systems
        KeyCommands();
        SunSystem();
        RecordVideo();
        PlaybackVideo();
        UpdateUI();

        // when ui is active onscreen
        if (isUIActive)
        {
            UpdateUI();
        }
    }

    private void KeyCommands()
    {
        // on escape key up, quit app
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }

        // toggles ui view
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isUIActive = !isUIActive;

            if (isUIActive)
            {
                uiSettings.LoadSettings();
            }
            else
            {
                uiSettings.SaveSettings();
            }

            // toggle canvas
            uiCanvas.SetActive(isUIActive);
        }

        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            UpdateSystemSettings();
        }
    }

    // sunrise data system
    private void SunSystem()
    {
        // check for latest sunrise at the specified time
        //Debug.Log("Local Time: " + sunrise.GetLocalTime());
        //Debug.Log("Time To check: " + uiSettings._sunriseTimeCheck.text);
        if (sunrise.GetLocalTime() == uiSettings._sunriseTimeCheck.text)
        {
            sunrise.GetSunriseTime();
            sampler.saveSunriseTime(sunrise.sunriseTime);
        }
        else
        {
            // reset switch
            if (sunrise.isUpdateTime)
            {
                // updates all of the UI text fields
                // runs after sunrise data has been pulled
                UpdateUIText();

                sunrise.isUpdateTime = false;
            }
        }

        // gets sunrise status
        sunrise.GetSunriseStatus();

        // if manual record is toggled, simulate sunrise
        if (manualRecord)
        {
            isSunriseActive = manualRecord;
        }
        else
        {
            isSunriseActive = sunrise.isActive;
        }

        // when the sunrise is active or simulated
        if (isSunriseActive)
        {
            // count up seconds
            elapsedTime += Time.deltaTime;

            // if elapsed time is greater than 1hr
            float duration;
            bool parsed = float.TryParse(uiSettings._recordingDuration.text, out duration);
            if (!parsed)
                duration = 3600;
            if (elapsedTime >= duration)
            {
                if (uiSettings._debugActive)
                {
                    Debug.Log("Sunrise cycle is complete! " + sunrise.GetLocalTime());
                    Debug.Log("Total time was " + elapsedTime + " seconds.");
                }

                // disable
                elapsedTime = 0f;
                manualRecord = false;
                sunrise.isActive = false;
            }
        }
    }

    // video recording system
    private void RecordVideo()
    {
        if (isSunriseActive)
        {
            if (!videoRecord.isRecording)
            {
                uiSettings.recordingStartTime.text = "Recording Start: " + sunrise.GetLocalTime();
                videoRecord.StartRecording();
                visController.startDrawing();
            }
        }
        else
        {
            if (videoRecord.isRecording)
            {
                uiSettings.recordingStopTime.text = "Recording Stop: " + sunrise.GetLocalTime();
                videoPlayback.emailActive = true;
                videoRecord.StopRecording();
                visController.stopDrawing();
            }
        }
    }

    // video playback system
    private void PlaybackVideo()
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
        
        //// updates email message body
        //if (!videoPlayback.screenshotEmailed)
        //{

        //}
    }

    public void setEmailBody()
    {
        uiSettings.LoadSettings();
        // gather system data to send along as well as the color for today
        string msg = "System Date: " + uiSettings.currentDate.text + " \n" +
            "Sunrise Time: " + uiSettings.sunriseTimeCur.text + " \n" +
            "Recording Start Time: " + uiSettings.recordingStartTime.text + " \n" +
            "Recording Stop Time: " + uiSettings.recordingStopTime.text + " \n" +
            "Color For the Day: #" + sampler.newestColorHex + " \n";

        emailSender.messageBody = msg;
    }
}