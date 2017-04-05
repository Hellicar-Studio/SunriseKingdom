using UnityEngine;
using System.Collections;
using System.IO;


public class VideoRecord : MonoBehaviour {

    // Struct with the recording details
    struct RecordingDetails
    {
        public bool collected;
        public string recordingID;
        public string startTime;
        public string stopTime;
    }

    struct PathDate
    {
        public string path;
        public System.DateTime date;

        public int CompareTo(PathDate _pathDate)
        {
            return date.CompareTo(_pathDate.date);
        }
    }

    [HideInInspector]
    public bool debugActive;
    [HideInInspector]
    //public bool StartRecordingButton; // Press me to Start Recording
    //public bool StopRecordingButton; // Press me to Stop Recording
    public bool getFilePathButton; // Press me to get the file path of the most recent recording
    //public bool recordForDurationButton; //Press me to kick of a recording for a certain number of minutes specificed by the recordingDuration field
    [HideInInspector]
    public string CamIP = "192.168.1.201"; // Change me to external IP address

    //private bool currentlyRecording;
    private string StartRecordingURL;
    private string StopRecordingURL;
    private string GetRecordingDetailsURL;

    public string newestPath;

    private ArrayList allVideoFiles;

    RecordingDetails details;

    private float timeRecordingStarted;
    private bool recording;
    public int recordingDuration;
    //[HideInInspector]
    public static string[] mostRecentRecording;
    [HideInInspector]
    public string recordingsRoot = "D:\\SunriseNAS";
    [HideInInspector]
    public bool isRecording = false;

    // Use this for initialization
    public void Start()
    {
        StartRecordingURL = "http://" + CamIP + "/axis-cgi/io/virtualinput.cgi?action=6:/";
        StopRecordingURL = "http://" + CamIP + "/axis-cgi/io/virtualinput.cgi?action=6:%5C";
        GetRecordingDetailsURL = "http://" + CamIP + "/axis-cgi/record/list.cgi?recordingid=all";

        details.collected = false;

        allVideoFiles = new ArrayList();

        mostRecentRecording = null;
        mostRecentRecording = new string[12];

        getRecordingPath(recordingsRoot);
    }

    /*
    // Update is called once per frame
    public void Update () {
        if (StartRecordingButton && !isRecording)
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

        if (getFilePathButton)
        {
            getRecordingPath(recordingsRoot);
            getFilePathButton = false;
        }

        if (recordForDurationButton)
        {
            StartCoroutine(recordForDuration(recordingDuration));
            recordForDurationButton = false;
        }
    }
    */

    // Start a new Recording (on the camera)
    public WWW StartRecording()
    {
        mostRecentRecording = null;
        mostRecentRecording = new string[12];

        WWW www = new WWW(StartRecordingURL);
        while (!www.isDone) { }
        if (debugActive)
            Debug.Log(www.text);
        isRecording = true;
        return www;
    }

    // Stop an ongoing Recording (on the camera)
    public WWW StopRecording()
    {
        WWW www = new WWW(StopRecordingURL);
        while (!www.isDone) { }
        if (debugActive)
            Debug.Log(www.text);
        getRecordingPath(recordingsRoot);
        isRecording = false;
        return www;
    }

    // Get the details of the most recent recording and save them in to the "details" object
    void getDetails()
    {
        GetRecordingDetails();
        if (debugActive)
        {
            Debug.Log("Recording ID: " + details.recordingID);
            Debug.Log("Recording Start Time: " + details.startTime);
            Debug.Log("Recording End Time: " + details.stopTime);
        }

        details.collected = true;
    }

    // Get the recording details from the XML file returned by the web request
    RecordingDetails GetRecordingDetails()
    {
        WWW www = new WWW(GetRecordingDetailsURL);
        while(!www.isDone) { }
        if (debugActive)
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

    public void getRecordingPath(string _root)
    {
        allVideoFiles.Clear();
        getMostRecentPath(_root);
        if(allVideoFiles.Count == 0)
        {
            if (debugActive)
                Debug.Log("No .mkv files found in root!");
            return;
        }
        PathDate[] pathDates = new PathDate[allVideoFiles.Count];
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
            pathDates[i].date = dateTime;
            pathDates[i].path = (string)allVideoFiles[i];
        }

        for(int j = 0; j < pathDates.Length - 1; j++)
        {
            // Find the smallest
            int iMin = j;
            // test against elements after j to find the smallest
            for(int i = j+1; i < pathDates.Length; i++)
            {
                if(pathDates[i].CompareTo(pathDates[iMin]) > 0)
                {
                    iMin = i;
                }
            }

            if(iMin != j)
            {
                PathDate temp = pathDates[iMin];
                pathDates[iMin] = pathDates[j];
                pathDates[j] = temp;
            }
        }

        for(int i = 0; i < pathDates.Length; i++)
        {
            if(i < mostRecentRecording.Length)
            {
                mostRecentRecording[i] = pathDates[i].path;
            } else
            {
                break;
            }
        }
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
        if (debugActive)
            Debug.Log("Done Recording! Duration: " + _durationInSeconds.ToString());
        StopRecording();
        getRecordingPath(recordingsRoot);
    }
}
