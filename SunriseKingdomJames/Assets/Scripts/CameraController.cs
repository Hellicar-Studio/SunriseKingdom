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
        public int startTimeInSeconds;
        public int stopTimeInSeconds;
        public int duration;
    }

    public bool StartRecordingButton; // Press me to Start Recording
    public bool StopRecordingButton; // Press me to Stop Recording
    public bool ExportButton;   // Press me to export the video
    public string CamIP = "192.168.1.201"; // Change me to external IP address
    public string RecordingPath = "StreamingAssets\\Videos"; //Relative to the Assets Folder, this is where the recordings get saved and cleared

    private bool currentlyRecording;
    private string StartRecordingURL;
    private string StopRecordingURL;
    private string GetRecordingDetailsURL;
    private WWW webResponse = null;
    private bool needToDownload;

    public int index;

    public int recordingInterval;


    RecordingDetails details;

    // Use this for initialization
    void Start () {
        StartRecordingURL = "http://" + CamIP + "/axis-cgi/io/virtualinput.cgi?action=6:/";
        StopRecordingURL = "http://" + CamIP + "/axis-cgi/io/virtualinput.cgi?action=6:%5C";
        GetRecordingDetailsURL = "http://" + CamIP + "/axis-cgi/record/list.cgi?recordingid=all";
        webResponse = null;
        needToDownload = false;

        index = 0;

        details.collected = false;
    }

    // Update is called once per frame
    void Update () {
        if (StartRecordingButton && !currentlyRecording)
        {
            StartRecordingButton = false;
            StartRecording();
        }

        if (StopRecordingButton)
        {
            StopRecordingButton = false;
            StopRecording();
        }

        if (ExportButton)
        {
            DownloadRecording();
        }

        if (webResponse != null)
        {
            if (needToDownload && webResponse.isDone)
            {
                //Debug.Log(webResponse.text);
                float newTime = Time.time;
                Debug.Log(webResponse.bytes);
                File.WriteAllBytes(Application.dataPath + "\\" + RecordingPath + "\\" + index.ToString() + ".mkv", webResponse.bytes);
                index++;
                needToDownload = false;
                if(index <= details.duration / recordingInterval)
                {
                    Debug.Log("Starting Another Download!");
                    getPropertiesAndExportRecording();
                }
            }
        }
    }

    // Start a new Recording (on the camera)
    public WWW StartRecording()
    {
        currentlyRecording = true;
        WWW www = new WWW(StartRecordingURL);
        return www;
    }

    // Stop an ongoing Recording (on the camera)
    public WWW StopRecording()
    {
        currentlyRecording = false;
        WWW www = new WWW(StopRecordingURL);
        return www;
    }

    // Kick off the recursive download recording process
    public void DownloadRecording()
    {
        getDetails();
        getPropertiesAndExportRecording();
    }

    // get the start and stop time (known a properties) and export the next recording in that interval
    void getPropertiesAndExportRecording()
    {
        if (details.collected)
        {
            ExportButton = false;
            string propertiesURL = generateGetPropertiesURLAfterInterval(recordingInterval);
            WWW www = new WWW(propertiesURL);
            while (!www.isDone) { }
            Debug.Log("Properties: " + www.text);
            //string startTime = getStartTimeFromPropertiesXML(www.text);
            string stopTime = getStopTimeFromPropertiesXML(www.text);
            Debug.Log("StartTime: " + details.startTime);
            Debug.Log("StopTime: " + stopTime);
            ExportRecording(details.recordingID, details.startTime, stopTime);
            details.startTime = stopTime;
            float timeStart = Time.time;

            needToDownload = true;
        }
    }

    // Get the details of the most recent recording and save them in to the "details" object
    void getDetails()
    {
        ClearFolder(Application.dataPath + "\\" +  RecordingPath);
        details = GetRecordingDetails();
        Debug.Log("Recording ID: " + details.recordingID);
        Debug.Log("Recording Start Time: " + details.startTime);
        Debug.Log("Recording End Time: " + details.stopTime);
        Debug.Log("Recording Start Time Int: " + details.startTimeInSeconds);
        Debug.Log("Recording End Time Int: " + details.stopTimeInSeconds);
        Debug.Log("Start Time String From Conversion: " + convertTimeToString(details.startTimeInSeconds));
        Debug.Log("Stop Time String From Conversion: " + convertTimeToString(details.stopTimeInSeconds));

        details.collected = true;
    }

    // Get the recording details from the XML file returned by the web request
    RecordingDetails GetRecordingDetails()
    {
        ExportButton = false;
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
        output.startTimeInSeconds = convertTimeToInt(newestStartTime);
        output.stopTimeInSeconds = convertTimeToInt(newestStopTime);
        output.duration = output.stopTimeInSeconds - output.startTimeInSeconds;

        details.collected = true;
        return output;
    }

    // Get the keyframe of the start time
    string getStartTimeFromPropertiesXML(string xml)
    {
        return findAttributeInXML(xml, "Starttime");
    }

    // Get the keyframe of the stop time
    string getStopTimeFromPropertiesXML(string xml)
    {
        return findAttributeInXML(xml, "Stoptime");
    }

    // find a particular attribute in an XML file
    string findAttributeInXML(string xml, string attribute)
    {
        System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(new System.IO.StringReader(xml));
        string value = "";
        while (reader.Read())
        {
            string newValue = reader.GetAttribute(attribute);

            if (newValue != null)
            {
                value = newValue;
            }
        }
        return value;
    }

    // Send the command to the camera to export a particular recording with a start and stop time specified.
    void ExportRecording(string newestRecordingID, string startTime, string stopTime)
    {
        string ExportRecordingURL = "http://" + CamIP + "/axis-cgi/record/export/exportrecording.cgi?schemaversion=1&recordingid=" + newestRecordingID + "&diskid=SD_DISK&exportformat=matroska&starttime=" + startTime + "&stoptime=" + stopTime;
        webResponse = new WWW(ExportRecordingURL);
        needToDownload = true;
    }

    // Generate a new get properties URL for the next recording interval
    string generateGetPropertiesURLAfterInterval(int interval)
    {
        string startDate = getDate(details.startTime);
        string endDate = getDate(details.startTime);

        if (startDate != endDate)
        {
            Debug.Log("CameraController::generateStopTimeAtPercentage : You're start and end date don't match! that's weird and shouldn't happen, returning empty string!");
            return "";
        }

        int start = convertTimeToInt(details.startTime);
        int end = convertTimeToInt(details.stopTime);

        int newEnd = start + interval;

        if (newEnd > end)
        {
            newEnd = end;
        }

        string newEndString = startDate + "T" + convertTimeToString(newEnd) + "Z";

        string getExportPropertiesURL = "http://" + CamIP + "/axis-cgi/record/export/properties.cgi?schemaversion=1&recordingid=" + details.recordingID + "&diskid=SD_DISK&exportformat=matroska&starttime=" + details.startTime + "&stoptime=" + newEndString;

        return getExportPropertiesURL;
    }

    // Get the date from the web responce
    string getDate(string timeString)
    {
        string[] dateAndTime = timeString.Split('T');
        if (dateAndTime.Length > 0)
        {
            // The date is the first value and the time is the second, but of course we first checked if the second existed
            string date = dateAndTime[0];
            return date;
        } else
        {
            Debug.Log("CameraController::getDate : 'T' was not a good seperator for the date and time, you'll need to find another solution, returning empty string");
            return "";
        }
    }

    // Convert an Axis formatted tim string to an integer amoutn of seconds
    int convertTimeToInt(string timeString)
    {
        // First split it along date and time
        string[] dateAndTime = timeString.Split('T');
        if (dateAndTime.Length > 1)
        {
            // The date is the first valye and the time is the second, but of course we first checked if the second existed
            string time = dateAndTime[1];
            string[] timeParts = time.Split(':');
            if (timeParts.Length > 2)
            {
                string hours = timeParts[0];
                string minutes = timeParts[1];
                string secondsAndMillis = timeParts[2];
                string[] secondsAndMillisParts = secondsAndMillis.Split('.');
                if (secondsAndMillisParts.Length > 0)
                {
                    string seconds = secondsAndMillisParts[0];
                    seconds = seconds.TrimEnd('Z');

                    int hoursInSeconds;
                    int minutesInSeconds;
                    int secondsInSeconds;

                    bool parsed = int.TryParse(hours, out hoursInSeconds);
                    if (parsed)
                    {
                        hoursInSeconds *= 60 * 60;
                    }
                    else
                    {
                        Debug.Log("CameraController::convertTimeToInt : the hours field was mal-formed! returning -1");
                        return -1;
                    }
                    parsed = int.TryParse(minutes, out minutesInSeconds);
                    if (parsed)
                    {
                        minutesInSeconds *= 60;
                    }
                    else
                    {
                        Debug.Log("CameraController::convertTimeToInt : the minutes field was mal-formed! returning -1");
                        return -1;
                    }
                    parsed = int.TryParse(seconds, out secondsInSeconds);
                    if (parsed)
                    {
                        int totalTime = hoursInSeconds + minutesInSeconds + secondsInSeconds;
                        return totalTime;
                    }
                    else
                    {
                        Debug.Log("CameraController::convertTimeToInt : the seconds field was mal-formed! it contained: " + seconds + " returning -1");
                        return -1;
                    }

                }
                else
                {
                    Debug.Log("CameraController::convertTimeToInt : '.' was not a good seperator for the seconds and milliseconds, you'll need to find another solution, returning -1");
                    return -1;
                }
            }
            else
            {
                Debug.Log("CameraController::convertTimeToInt : ':' was not a good seperator for the time, you'll need to find another solution, returning -1");
                return -1;
            }

        }
        else
        {
            Debug.Log("CameraController::convertTimeToInt : 'T' was not a good seperator between the time and the date, you'll need to find another solution, returning -1");
            return -1;
        }
    }

    // Convert an integer amount of seconds to an axis useable time string
    string convertTimeToString(int timeDouble)
    {
        double hoursDouble = timeDouble / 60 / 60;
        int hoursInt = (int)hoursDouble;
        int secondsInHours = hoursInt * 60 * 60;
        timeDouble -= secondsInHours;

        double minutesDouble = timeDouble / 60;
        int minutesInt = (int)minutesDouble;
        int secondsInMinutes = minutesInt * 60;

        timeDouble -= secondsInMinutes;

        double secondsDouble = timeDouble;

        string hoursString = padWithZeros(hoursInt.ToString());
        string minutesString = padWithZeros(minutesInt.ToString());
        string secondsString = padWithZeros(secondsDouble.ToString());

        string output = hoursString + ":" + minutesString + ":" + secondsString;

        return output;
    }

    // clears save folder and resets file index
    void ClearFolder(string _folderName)
    {
        // reset index
        if (index != 0) index = 0;

        // access save folder directory
        DirectoryInfo dir = new DirectoryInfo(_folderName);
        // delete all files that exist
        foreach (FileInfo fi in dir.GetFiles())
        {
            fi.Delete();
        }
    }

    // Pad a string number with 0s if it has only one digit
    string padWithZeros(string num)
    {
        if(num.Length == 1)
        {
            num = "0" + num;
        }
        return num;
    }
}
