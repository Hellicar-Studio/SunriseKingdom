using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public bool StartRecordingButton; // Press me to Start Recording
    static public bool currentlyRecording; // Press me to Stop Recording
    public bool StopRecordingButton;
    public string CamIP = "192.168.1.201"; // Change me to external IP address

    private string StartRecordingURL;
    private string StopRecordingURL;
    private string GetRecordingDetailsURL;
    private WWW webResponse;

    public bool ExportButton;   // Press me to export the Video  

    // Use this for initialization
    void Start () {
        StartRecordingURL = "http://" + CamIP + "/axis-cgi/io/virtualinput.cgi?action=6:/";
        StopRecordingURL = "http://" + CamIP + "/axis-cgi/io/virtualinput.cgi?action=6:%5C";
        GetRecordingDetailsURL = "http://" + CamIP + "/axis-cgi/record/list.cgi?recordingid=all";
    }
	
	// Update is called once per frame
	void Update () {
        if (StartRecordingButton && !currentlyRecording)
        {
            StartRecordingButton = false;
            StartRecording();
        }

        if (StopRecordingButton && currentlyRecording)
        {
            StopRecordingButton = false;
            StopRecording();
        }

        if(ExportButton)
        {
            Debug.Log("Test 0");
            ExportRecording(GetRecordingDetails());
        }
    }

    WWW StartRecording()
    {
        Debug.Log("Test Start Rec");
        currentlyRecording = true;
        WWW www = new WWW(StartRecordingURL);
        return www;
    }

    WWW StopRecording()
    {
        Debug.Log("Test Stop Rec");
        currentlyRecording = false;
        WWW www = new WWW(StopRecordingURL);
        return www;
    }

    string GetRecordingDetails()
    {
        Debug.Log("Test 1");
        ExportButton = false;
        WWW www = new WWW(GetRecordingDetailsURL);
        while(!www.isDone) { }
        Debug.Log(www.text);
        System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader( new System.IO.StringReader(www.text));
        string newestID = "";
        while(reader.Read())
        {
            Debug.Log("Reading!");
            Debug.Log("Start Time: " + reader.GetAttribute("starttime"));
            Debug.Log("Stop Time: " + reader.GetAttribute("stoptime"));
            newestID = reader.GetAttribute("recordingid");
            Debug.Log(newestID);
        }
        return newestID;
    }

    void ExportRecording(string newestRecordingID)
    {
        string ExportRecordingURL = "http://" + CamIP + "/axis-cgi/record/export/exportrecording.cgi?schemaversion=1&recordingid=" + newestRecordingID + "&diskid=SD_DISK&exportformat=matroska";
        WWW www = new WWW(ExportRecordingURL);
        while (!www.isDone)
        {
            Debug.Log("Waiting!");
        }
    }
}
