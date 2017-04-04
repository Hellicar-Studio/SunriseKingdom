using UnityEngine;
using System.Collections;

using System.IO;


public class CameraController : MonoBehaviour {

    // Struct with the recording details
    struct RecordingDetails
    {
        public bool collected;
        public string recordingID;
        public string startTime;
        public string stopTime;
    }

    public bool StartRecordingButton; // Press me to Start Recording
    public bool StopRecordingButton; // Press me to Stop Recording
    public bool getFilePathButton; // Press me to get the file path of the most recent recording
    public bool recordForDurationButton; //Press me to kick of a recording for a certain number of minutes specificed by the recordingDuration field

    public string CamIP = "192.168.1.201"; // Change me to external IP address

    private bool currentlyRecording;
    private string StartRecordingURL;
    private string StopRecordingURL;
    private string GetRecordingDetailsURL;

    public string newestPath;

    private ArrayList allVideoFiles;

    RecordingDetails details;

    private float timeRecordingStarted;
    private bool recording;
    public int recordingDuration;

    public string recordingsRoot = "D:\\SunriseNAS";

    // Use this for initialization
    void Start () {
        StartRecordingURL = "http://" + CamIP + "/axis-cgi/io/virtualinput.cgi?action=6:/";
        StopRecordingURL = "http://" + CamIP + "/axis-cgi/io/virtualinput.cgi?action=6:%5C";
        GetRecordingDetailsURL = "http://" + CamIP + "/axis-cgi/record/list.cgi?recordingid=all";

        details.collected = false;

        allVideoFiles = new ArrayList();

        newestPath = getRecordingPath(recordingsRoot);
    }

    // Update is called once per frame
    void Update () {
        if (StartRecordingButton && !currentlyRecording)
        {
            StartRecordingButton = false;
            //StartRecordingURL = "http://root:pass@" + CamIP + "/axis-cgi/record/record.cgi?diskid=NetworkShare";
            StartRecording();
        }

        if (StopRecordingButton)
        {
            getDetails();
            StopRecordingButton = false;
            //StopRecordingURL = "http://root:pass@" + CamIP + "/axis-cgi/record/stop.cgi?recordingid=" + details.recordingID;
            StopRecording();
        }

        if(getFilePathButton)
        {
            newestPath = getRecordingPath(recordingsRoot);
            getFilePathButton = false;
        }

        if(recordForDurationButton)
        {
            StartCoroutine(recordForDuration(recordingDuration));
            recordForDurationButton = false;
        }
    }

    // Start a new Recording (on the camera)
    public WWW StartRecording()
    {
        currentlyRecording = true;
        WWW www = new WWW(StartRecordingURL);
        while (!www.isDone) { }
        Debug.Log(www.text);
        return www;
    }

    // Stop an ongoing Recording (on the camera)
    public WWW StopRecording()
    {
        currentlyRecording = false;
        WWW www = new WWW(StopRecordingURL);
        while (!www.isDone) { }
        Debug.Log(www.text);
        return www;
    }

    // Get the details of the most recent recording and save them in to the "details" object
    void getDetails()
    {
        GetRecordingDetails();
        Debug.Log("Recording ID: " + details.recordingID);
        Debug.Log("Recording Start Time: " + details.startTime);
        Debug.Log("Recording End Time: " + details.stopTime);

        details.collected = true;
    }

    // Get the recording details from the XML file returned by the web request
    RecordingDetails GetRecordingDetails()
    {
        WWW www = new WWW(GetRecordingDetailsURL);
        while(!www.isDone) { }
        Debug.Log(www.text);
        System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader( new System.IO.StringReader(www.text));
        string newestID = "";
        string newestStartTime = "";
        string newestStopTime = "";
        while(reader.Read())
        {
            string newID = reader.GetAttribute("recordingid");
            string newStartTime = reader.GetAttribute("starttime");
            string newStopTime = reader.GetAttribute("stoptime");

            if (newID != null)
            {
                newestStartTime = newStartTime;
                newestStopTime = newStopTime;
                newestID = newID;
            }
        }

        RecordingDetails output = new RecordingDetails();
        output.recordingID = newestID;
        output.startTime = newestStartTime;
        output.stopTime = newestStopTime;

        details.collected = true;
        return output;
    }

    void getMostRecentPath(string _root)
    {
        DirectoryInfo di = new DirectoryInfo(_root);
        FileInfo[] fi = di.GetFiles("*.mkv");
        DirectoryInfo[] dis = di.GetDirectories();
        for(int i = 0; i < fi.Length; i++)
        {
            allVideoFiles.Add(fi[i].FullName);
        }
        for(int i = 0; i < dis.Length; i++)
        {
            getMostRecentPath(dis[i].FullName);
        }
    }

    public string getRecordingPath(string _root)
    {
        allVideoFiles.Clear();
        getMostRecentPath(_root);
        if(allVideoFiles.Count == 0)
        {
            Debug.Log("No .mkv files found in root!");
            return "";
        }
        int newestIndex = 0;
        System.DateTime newestTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc); ;
        for(int i = 0; i < allVideoFiles.Count; i++)
        {
            //Debug.Log("Path " + i.ToString() + ": " + allVideoFiles[i]);
            string path = (string)allVideoFiles[i];
            string date = getDateFromFilePath(path);
            string time = getTimeFromFilePath(path);
            int year = int.Parse(date.Remove(4, 4));
            int month = int.Parse(date.Remove(0, 4).Remove(2, 2));
            int day = int.Parse(date.Remove(0, 6));
            int hour = int.Parse(time.Remove(2, 4));
            int minute = int.Parse(time.Remove(0, 2).Remove(2, 2));
            int second = int.Parse(time.Remove(0, 4));
            //Debug.Log("Year " + i.ToString() + ": " + year);
            //Debug.Log("Month " + i.ToString() + ": " + month);
            //Debug.Log("Day " + i.ToString() + ": " + day);
            //Debug.Log("Hour " + i.ToString() + ": " + hour);
            //Debug.Log("Minute " + i.ToString() + ": " + minute);
            //Debug.Log("Second " + i.ToString() + ": " + second);
            System.DateTime dateTime = new System.DateTime(year, month, day, hour, minute, second, 0, System.DateTimeKind.Utc);
            int compare = dateTime.CompareTo(newestTime);
            if(compare > 0)
            {
                newestTime = dateTime;
                newestIndex = i;
            }
        }
        return (string)allVideoFiles[newestIndex];
    }

    string getDateFromFilePath(string _path)
    {
        string[] extensionRemoved = _path.Split('.');
        string[] pathRemoved = extensionRemoved[0].Split('\\');
        string[] splitDateAndTime = pathRemoved[pathRemoved.Length - 1].Split('_');
        string date = splitDateAndTime[0];
        //Debug.Log("Date: " + date);
        return date;
    }

    string getTimeFromFilePath(string _path)
    {
        string[] extensionRemoved = _path.Split('.');
        string[] pathRemoved = extensionRemoved[0].Split('\\');
        string[] splitDateAndTime = pathRemoved[pathRemoved.Length - 1].Split('_');
        string time = splitDateAndTime[1];
        //Debug.Log("Time: " + time);
        return time;
    }

    public IEnumerator recordForDuration(int _durationInSeconds)
    {
        timeRecordingStarted = Time.time;
        StartRecording();
        while(Time.time - timeRecordingStarted < _durationInSeconds)
        {
            //Debug.Log(Time.time - timeRecordingStarted);
            yield return null;
        }
        Debug.Log("Done Recording! Duration: " + _durationInSeconds.ToString());
        StopRecording();
        newestPath = getRecordingPath(recordingsRoot);
    }
}
