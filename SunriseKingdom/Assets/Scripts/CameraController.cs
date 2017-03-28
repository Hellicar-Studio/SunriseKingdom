using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    // Struct with the recording details
    struct RecordingDetails
    {
        public bool collected;
        public string recordingID;
        public string startTime;
        public string stopTime;
        public string date;
        public int startTimeInSeconds;
        public int stopTimeInSeconds;
    }

    public bool StartRecordingButton; // Press me to Start Recording
    static public bool currentlyRecording; // Press me to Stop Recording
    public bool StopRecordingButton;
    public bool GetDetailsButton;
    public string CamIP = "192.168.1.201"; // Change me to external IP address

    private string StartRecordingURL;
    private string StopRecordingURL;
    private string GetRecordingDetailsURL;
    private WWW webResponse = null;
    private bool needToDownload;

    public int index;

    public int recordingInterval;

    private float lastDownloadTime;
    private float startRecTime;

    public bool ExportButton;   // Press me to export the video

    RecordingDetails details;

    public string startTime;

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

        if(GetDetailsButton)
        {
            details = GetRecordingDetails();
            Debug.Log("Recording ID: " + details.recordingID);
            Debug.Log("Recording Start Time: " + details.startTime);
            Debug.Log("Recording End Time: " + details.stopTime);
            Debug.Log("Recording Start Time Int: " + details.startTimeInSeconds);
            Debug.Log("Recording End Time Int: " + details.stopTimeInSeconds);
            Debug.Log("Start Time String From Conversion: " + convertTimeToString(details.startTimeInSeconds));
            Debug.Log("Stop Time String From Conversion: " + convertTimeToString(details.stopTimeInSeconds));

            details.collected = true;
            GetDetailsButton = false;
        }

        if (ExportButton)
        {
            if (details.collected)
            {
                ExportButton = false;
                Debug.Log("Old Start Time: " + details.startTime);
                string exportURL = generateExportURLAfterInterval(recordingInterval);
                Debug.Log("New Start Time: " + details.startTime);
                Debug.Log(exportURL);
                webResponse = new WWW(exportURL);
                float timeStart = Time.time;
                Debug.Log("Time Download Started: " + timeStart);

                needToDownload = true;
            }
        }

        if (webResponse != null)
        {
            if (needToDownload && webResponse.isDone)
            {
                //Debug.Log(webResponse.text);
                float newTime = Time.time;
                Debug.Log("Time Download Ended: " + newTime);
                Debug.Log("Num Bytes: " + webResponse.bytesDownloaded);
                Debug.Log("System Start Write: " + Time.time);
                System.IO.File.WriteAllBytes("C:\\Users\\Flowers\\Documents\\RandD\\SunriseKingdom\\Assets\\StreamingAssets\\Videos\\Test" + index.ToString() + ".mkv", webResponse.bytes);
                Debug.Log("System Ended Write: " + Time.time);
                index++;
                needToDownload = false;
                if(index <= 60 / recordingInterval)
                {
                    ExportButton = true;
                }
            }
        }

        startTime = details.startTime;
    }

    WWW StartRecording()
    {
        Debug.Log("Test Start Rec");
        currentlyRecording = true;
        WWW www = new WWW(StartRecordingURL);
        lastDownloadTime = Time.time;
        startRecTime = Time.time;
        return www;
    }

    WWW StopRecording()
    {
        Debug.Log("Test Stop Rec");
        currentlyRecording = false;
        WWW www = new WWW(StopRecordingURL);
        return www;
    }

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

        details.collected = true;
        return output;
    }

    void ExportRecording(string newestRecordingID, string startTime, string stopTime)
    {
        Debug.Log(newestRecordingID);
        string ExportRecordingURL = "http://" + CamIP + "/axis-cgi/record/export/exportrecording.cgi?schemaversion=1&recordingid=" + newestRecordingID + "&diskid=SD_DISK&exportformat=matroska&starttime=" + startTime + "&stoptime=" + stopTime;
        Debug.Log(ExportRecordingURL);
        webResponse = new WWW(ExportRecordingURL);
        float timeStart = Time.time;
        Debug.Log("Time Download Started: " + timeStart);
        needToDownload = true;
    }

    string generateExportURLAfterInterval(int interval)
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

        int newEnd = start + interval * 60;

        if (newEnd > end)
        {
            newEnd = end;
        }

        string newEndString = startDate + "T" + convertTimeToString(newEnd) + ".0000Z";

        string ExportRecordingURL = "http://" + CamIP + "/axis-cgi/record/export/exportrecording.cgi?schemaversion=1&recordingid=" + details.recordingID + "&diskid=SD_DISK&exportformat=matroska&starttime=" + details.startTime + "&stoptime=" + newEndString;

        details.startTime = newEndString;

        return ExportRecordingURL;
    }

    string getDate(string timeString)
    {
        string[] dateAndTime = timeString.Split('T');
        if (dateAndTime.Length > 0)
        {
            // The date is the first valye and the time is the second, but of course we first checked if the second existed
            string date = dateAndTime[0];
            return date;
        } else
        {
            Debug.Log("CameraController::getDate : 'T' was not a good seperator for the date and time, you'll need to find another solution, returning empty string");
            return "";
        }
    }

    int convertTimeToInt(string timeString)
    {
        // First split it along date and time
        string[] dateAndTime = timeString.Split('T');
        if(dateAndTime.Length > 1)
        {
            // The date is the first valye and the time is the second, but of course we first checked if the second existed
            string time = dateAndTime[1];
            string[] timeParts = time.Split(':');
            if(timeParts.Length > 2)
            {
                string hours = timeParts[0];
                string minutes = timeParts[1];
                string secondsAndMillis = timeParts[2];
                string[] secondsAndMillisParts = secondsAndMillis.Split('.');
                if(secondsAndMillisParts.Length > 0)
                {
                    string seconds = secondsAndMillisParts[0];
                    int hoursInSeconds;
                    int minutesInSeconds;
                    int secondsInSeconds;

                    bool parsed = int.TryParse(hours, out hoursInSeconds);
                    if(parsed)
                    {
                        hoursInSeconds *= 60 * 60;
                    } else
                    {
                        Debug.Log("CameraController::convertTimeToInt : the hours field was mal-formed! returning -1");
                        return -1;
                    }
                    parsed = int.TryParse(minutes, out minutesInSeconds);
                    if(parsed)
                    {
                        minutesInSeconds *= 60;
                    } else
                    {
                        Debug.Log("CameraController::convertTimeToInt : the minutes field was mal-formed! returning -1");
                        return -1;
                    }
                    parsed = int.TryParse(seconds, out secondsInSeconds);
                    if(parsed)
                    {
                        int totalSeconds = hoursInSeconds + minutesInSeconds + secondsInSeconds;
                        return totalSeconds;
                    } else
                    {
                        Debug.Log("CameraController::convertTimeToInt : the seconds field was mal-formed! returning -1");
                        return -1;
                    }
                } else
                {
                    Debug.Log("CameraController::convertTimeToInt : '.' was not a good seperator for the seconds and milliseconds, you'll need to find another solution, returning -1");
                    return -1;
                }
            } else
            {
                Debug.Log("CameraController::convertTimeToInt : ':' was not a good seperator for the time, you'll need to find another solution, returning -1");
                return -1;
            }

        } else
        {
            Debug.Log("CameraController::convertTimeToInt : 'T' was not a good seperator between the time and the date, you'll need to find another solution, returning -1");
            return -1;
        }
        return 0;
    }

    string convertTimeToString(int timeInt)
    {
        float hoursFloat = timeInt / 60 / 60;
        int hoursInt = (int)hoursFloat;
        int secondsInHours = hoursInt * 60 * 60;
        timeInt -= secondsInHours;

        float minutesFloat = timeInt / 60;
        int minutesInt = (int)minutesFloat;
        int secondsInMinutes = minutesInt * 60;

        timeInt -= secondsInMinutes;

        int secondsInt = timeInt;

        string hoursString = padWithZeros(hoursInt.ToString());
        string minutesString = padWithZeros(minutesInt.ToString());
        string secondsString = padWithZeros(secondsInt.ToString());

        string output = hoursString + ":" + minutesString + ":" + secondsString;

        return output;
    }

    string padWithZeros(string num)
    {
        if(num.Length == 1)
        {
            num = "0" + num;
        }
        return num;
    }
}
