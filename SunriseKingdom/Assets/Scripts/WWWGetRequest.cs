using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System;

public class WWWGetRequest : MonoBehaviour {

    public static bool sunriseActive;

    public string URL = "http://api.openweathermap.org/data/2.5/weather?q=Berlin&appid=7f09e7d718a5c1dd8d39f1635ac7f006";

    private string unixTime;
    private string data;
    private int utcTime;

    void Start()
    {
        StartCoroutine(GetText());
    }

    void Update()
    {
        if (DateTime.UtcNow.ToLocalTime().ToShortTimeString() == GetSunriseTime(utcTime))
        {
            if (!sunriseActive) sunriseActive = true;
        }
    }

    IEnumerator GetText() 
    {
        using(UnityWebRequest www = UnityWebRequest.Get(URL))
        {
            yield return www.Send();

            if(www.isError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // results as text
                data = www.downloadHandler.text;
                // parses text based on http://openweathermap.org/api for JSON
                JObject j = JObject.Parse(data);
                utcTime = (int)j.GetValue("sys").SelectToken("sunrise");
                // display results in the console
                Debug.Log(GetSunriseTime(utcTime));
            }
        }
    }

    // gets sunrise time and converts UNIX/UTC to local time && short time (removes the date)
    public string GetSunriseTime(int _time)
    {
        string t;
        t = string.Format("{0:d/M/yyyy HH:mm:ss}", new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(_time).ToLocalTime().ToShortTimeString());

        return t;
    }
}