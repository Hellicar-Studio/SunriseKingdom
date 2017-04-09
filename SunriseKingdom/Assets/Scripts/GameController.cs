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
    public VideoStream videoStream;
    public VideoPlayback videoPlayback;
    public VideoRecord videoRecord;
    public EmailThread emailSender;
    public Transform playbackTransform;
    [Header("Support")]
    public bool simulationMode = false;
    public bool manualRecord = false;
    public bool runFirstRun = false;

    private bool isSunriseActive = false;
    private bool isUIActive = true;
    private float elapsedTime = 0f;
    private float fpsTime = 0f;

    // Use this for initialization
    void Start () 
    {
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

        // updates system variables
        UpdateSystemSettings();
    }

    void UpdateSystemSettings()
    {
        // sunrise startup
        sunrise.apiKey = uiSettings._apiKey.text;
        sunrise.city = uiSettings._city.text;
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
        videoPlayback.videoLoadTime = float.Parse(uiSettings._videoLoadTime.text);

        // setup video recorder variables
        videoRecord.CamIP = uiSettings._cameraIP.text;
        videoRecord.recordingsRoot = uiSettings._videoFolder.text;
        videoRecord.maxVideos = (int)Mathf.Max(2, float.Parse(uiSettings._recordingDuration.text) / 300.0f);

        // setup email settings
        emailSender.emailAccount = uiSettings._emailAccount.text;
        emailSender.emailPassword = uiSettings._emailPassword.text;
        emailSender.serverSMTP = uiSettings._serverSMTP.text;
        emailSender.portSMTP = int.Parse(uiSettings._portSMTP.text);
        emailSender.emailRecipient = uiSettings._emailRecipient.text;
    }

    void UpdateUIText()
    {
        if (uiSettings._debugActive)
        {
            Debug.Log("Updating UI textfields...");
        }

        // sunrise data
        uiSettings.apiKey.text = "API Key: " + sunrise.apiKey;
        uiSettings.city.text = "City: " + sunrise.city;
        uiSettings.country.text = "Country: " + sunrise.country;
        uiSettings.latlon.text = "Lat/Lon: " + sunrise.lat + " / " + sunrise.lon;
        uiSettings.currentDate.text = "Current Date: " + sunrise.GetCurrentDate();
        uiSettings.currentTime.text = "Current Time: " + sunrise.GetLocalTime();

        // recording settings
        uiSettings.cameraIP.text = "Camera IP: " + videoRecord.CamIP;
        uiSettings.recordingDuration.text = "Recording Duration: " + uiSettings._recordingDuration.text;

        // playback settings
        uiSettings.imageFolder.text = "Image Folder: " + videoPlayback.imageFolder;
        uiSettings.videoFolder.text = "Video Folder: " + videoPlayback.videoFolder;
        uiSettings.videoLoadTime.text = "Video Load Time: " + videoPlayback.videoLoadTime.ToString();

        // email sender
        uiSettings.emailAccount.text = "Account: " + emailSender.emailAccount;
        uiSettings.emailPassword.text = "Password: " + emailSender.emailPassword;
        uiSettings.serverSMTP.text = "Server: " + emailSender.serverSMTP;
        uiSettings.portSMTP.text = "Port: " + emailSender.portSMTP.ToString();
        uiSettings.emailRecipient.text = "Recipient: " + emailSender.emailRecipient;
    }

    // Update is called once per frame
    void Update () 
    {
        // main systems
        SunSystem();
        RecordVideo();
        PlaybackVideo();

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

        // when ui is active onscreen
        if (isUIActive)
        {
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

            simulationMode = uiSettings._simulation.isOn;
            manualRecord = uiSettings._manualRecord.isOn;

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

        if (Input.GetKeyUp(KeyCode.Return))
        {
            UpdateSystemSettings();
        }

        // when the sunrise is active or simulated
        if (isSunriseActive || manualRecord)
        {
            // count up seconds
            elapsedTime += Time.deltaTime;

            // if elapsed time is greater than 1hr
            float duration = float.Parse(uiSettings.recordingDuration.text);
            if (elapsedTime >= duration)
            {
                if (uiSettings._debugActive)
                {
                    Debug.Log("Sunrise cycle is complete!");
                    Debug.Log("Total time was " + elapsedTime + " seconds.");
                }

                // disable
                elapsedTime = 0f;
                manualRecord = false;
                sunrise.isActive = false;
            }
        }
	}

    // sunrise data system
    private void SunSystem()
    {
        if (sunrise.GetLocalTime() == uiSettings.sunriseTimeCheck.text)
        {
            sunrise.GetSunriseTime();
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

    // video recording system
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
                {
                    videoPlayback.emailActive = true;
                    videoRecord.StopRecording();
                }
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
                {
                    videoPlayback.emailActive = true;
                    videoRecord.StopRecording();
                }
            }
        }
    }

    // video playback system
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
}