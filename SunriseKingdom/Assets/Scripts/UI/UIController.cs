using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    [Header("Sliders")]
    public Slider _xPosition;
    public Slider _yPosition;
    public Slider _scale;

    [Header("Text")]
    public Text apiKey;
    public Text sunriseTimeCur;
    public Text sunriseTimeCheck;
    public Text country;
    public Text city;
    public Text latlon;
    public Text currentDate;
    public Text currentTime;
    public Text cameraIP;
    public Text recordingDuration;
    public Text imageFolder;
    public Text videoFolder;
    public Text videoLoadTime;
    public Text emailAccount;
    public Text emailPassword;
    public Text serverSMTP;
    public Text portSMTP;
    public Text emailRecipient;
    public Text fps;

    [Header("Input Fields")]
    public InputField _apiKey;
    public InputField _city;
    public InputField _sunriseTimeCheck;
    public InputField _imageFolder;
    public InputField _videoFolder;
    public InputField _videoLoadTime;
    public InputField _cameraIP;
    public InputField _recordingDuration;
    public InputField _emailAccount;
    public InputField _emailPassword;
    public InputField _serverSMTP;
    public InputField _portSMTP;
    public InputField _emailRecipient;

    [Header("Toggles")]
    public Toggle _debugActive;
    public Toggle _simulation;
    public Toggle _manualRecord;
    public Toggle _resetSystem;
    public Toggle _mediaPlay;
    public Toggle _mediaPause;
    public Toggle _mediaStop;

    void OnEnable()
    {
        LoadSettings();
    }

    void OnDisable()
    {
        SaveSettings();
    }

    public void DefaultSettings()
    {
        // video position
        _xPosition.value = 0f;
        _yPosition.value = 0f;
        _scale.value = 1f;

        // sunrise data settings
        _apiKey.text = "7f09e7d718a5c1dd8d39f1635ac7f006";
        _city.text = "London";
        _sunriseTimeCheck.text = "03:00";

        // player settings
        _imageFolder.text = "D:\\SunriseData/Images/";
        _videoFolder.text = "D:\\SunriseData/Videos/";
        _videoLoadTime.text = "150";

        // recording settings
        _cameraIP.text = "192.168.1.201";
        _recordingDuration.text = "3600";

        // email settings
        _emailAccount.text = "jason@glitchbeam.com";
        _emailPassword.text = "";
        _serverSMTP.text = "smtp.gmail.com";
        _portSMTP.text = "587";
        _emailRecipient.text = "jason@glitchbeam.com";

        // toggle settings
        _debugActive.isOn = true;
        _simulation.isOn = false;
        _manualRecord.isOn = false;
        _resetSystem.isOn = false;
        _mediaPlay.isOn = false;
        _mediaPause.isOn = false;
        _mediaStop.isOn = false;

        SaveSettings();
    }

    public void LoadSettings()
    {
        // video position
        _xPosition.value = PlayerPrefs.GetFloat("playerPositionX");
        _yPosition.value = PlayerPrefs.GetFloat("playerPositionY");
        _scale.value = PlayerPrefs.GetFloat("playerScale");

        // sunrise data settings
        _apiKey.text = PlayerPrefs.GetString("apiKey");
        _city.text = PlayerPrefs.GetString("city");
        _sunriseTimeCheck.text = PlayerPrefs.GetString("sunriseTimeCheck");

        // player settings
        _imageFolder.text = PlayerPrefs.GetString("imageFolder");
        _videoFolder.text = PlayerPrefs.GetString("videoFolder");
        _videoLoadTime.text = PlayerPrefs.GetString("videoLoadTime");

        // recording settings
        _cameraIP.text = PlayerPrefs.GetString("cameraIP");
        _recordingDuration.text = PlayerPrefs.GetString("recordingDuration");

        // email settings
        _emailAccount.text = PlayerPrefs.GetString("emailAccount");
        _emailPassword.text = PlayerPrefs.GetString("emailPassword");
        _serverSMTP.text = PlayerPrefs.GetString("smtpServer");
        _portSMTP.text = PlayerPrefs.GetString("smtpPort");
        _emailRecipient.text = PlayerPrefs.GetString("emailRecipient");

        // toggle settings
        _debugActive.isOn = bool.Parse(PlayerPrefs.GetString("debugActive"));
        _simulation.isOn = bool.Parse(PlayerPrefs.GetString("simulation"));
        _manualRecord.isOn = bool.Parse(PlayerPrefs.GetString("manualRecord"));
        _resetSystem.isOn = bool.Parse(PlayerPrefs.GetString("resetSystem"));
        _mediaPlay.isOn = bool.Parse(PlayerPrefs.GetString("mediaPlay"));
        _mediaPause.isOn = bool.Parse(PlayerPrefs.GetString("mediaPause"));
        _mediaStop.isOn = bool.Parse(PlayerPrefs.GetString("mediaStop"));
    }

    public void SaveSettings()
    {
        // video position
        PlayerPrefs.SetFloat("playerPositionX", _xPosition.value);
        PlayerPrefs.SetFloat("playerPositionY", _yPosition.value);
        PlayerPrefs.SetFloat("playerScale", _scale.value);

        // sunrise data settings
        PlayerPrefs.SetString("apiKey", _apiKey.text);
        PlayerPrefs.SetString("city", _city.text);
        PlayerPrefs.SetString("sunriseTimeCheck", _sunriseTimeCheck.text);

        // player settings
        PlayerPrefs.SetString("imageFolder", _imageFolder.text);
        PlayerPrefs.SetString("videoFolder", _videoFolder.text);
        PlayerPrefs.SetString("videoLoadTime", _videoLoadTime.text);

        // recording settings
        PlayerPrefs.SetString("cameraIP", _cameraIP.text);
        PlayerPrefs.SetString("recordingDuration", _recordingDuration.text);

        // email settings
        PlayerPrefs.SetString("emailAccount", _emailAccount.text);
        PlayerPrefs.SetString("emailPassword", _emailPassword.text);
        PlayerPrefs.SetString("smtpServer", _serverSMTP.text);
        PlayerPrefs.SetString("smtpPort", _portSMTP.text);
        PlayerPrefs.SetString("emailRecipient", _emailRecipient.text);

        // toggles
        PlayerPrefs.SetString("debugActive", boolToString(_debugActive.isOn));
        PlayerPrefs.SetString("simulation", boolToString(_simulation.isOn));
        PlayerPrefs.SetString("manualRecord", boolToString(_manualRecord.isOn));
        PlayerPrefs.SetString("resetSystem", boolToString(_resetSystem.isOn));
        PlayerPrefs.SetString("mediaPlay", boolToString(_mediaPlay.isOn));
        PlayerPrefs.SetString("mediaPause", boolToString(_mediaPause.isOn));
        PlayerPrefs.SetString("mediaStop", boolToString(_mediaStop.isOn));

        // commits to disk
        PlayerPrefs.Save();
    }

    private string boolToString(bool _b)
    {
        string s = null;
        if (_b)
            s = "true";
        else
            s = "false";
        return s;
    }
}