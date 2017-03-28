using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System;

public class SunriseController : MonoBehaviour {

    [HideInInspector] 
    public string APIKey;
    [HideInInspector] 
    public string city;
    [HideInInspector] 
    public bool isUpdateTime = false;
    [HideInInspector]
    public bool isActive = false;
    [HideInInspector]
    public bool debugActive = false;

//    private float elapsedTime = 0f;
    private int utcTime;
    private string URL1 = "http://api.openweathermap.org/data/2.5/weather?q=";
    private string URL2 = "&appid=";
    private string unixTime;
    private string data;

    public string GetLocalTime()
    {
        return DateTime.UtcNow.ToLocalTime().ToShortTimeString();
    }

    public void GetSunriseStatus()
    {
        // if sunrise time, sets sunrise active trigger to true
        if (GetLocalTime() == ConvertTime(utcTime))
        {
            // enable sunrise active timer
            if (!isActive)
            {
                if (debugActive)
                    Debug.Log("Sunrise isActive " + isActive);

                isActive = true;
            }
        }
    }

    // gets sunrise time and converts UNIX/UTC to local time && short time (removes the date)
    private string ConvertTime(int _time)
    {
        string t;
        t = string.Format("{0:d/M/yyyy HH:mm:ss}", new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(_time).ToLocalTime().ToShortTimeString());

        return t;
    }

    // gets the latest sunrise time
    public void GetSunriseTime()
    {
        if (!isUpdateTime)
        {
            StartCoroutine(GetUpdatedTime());
            isUpdateTime = true;
        }
    }

    IEnumerator GetUpdatedTime() 
    {
        // example of full URL "http://api.openweathermap.org/data/2.5/weather?q=Berlin&appid=7f09e7d718a5c1dd8d39f1635ac7f006"
        using(UnityWebRequest www = UnityWebRequest.Get(URL1 + city + URL2 + APIKey))
        {
            yield return www.Send();

            if(www.isError)
            {
                if (debugActive) Debug.Log(www.error);
            }
            else
            {
                // results as text
                data = www.downloadHandler.text;
                // parses text based on http://openweathermap.org/api for JSON
                JObject j = JObject.Parse(data);
                utcTime = (int)j.GetValue("sys").SelectToken("sunrise");
                
                // display results in the console
                if (debugActive)
                {
                    string _country = j.GetValue("sys").SelectToken("country").ToString();
                    string _cityid = j.SelectToken("id").ToString();
                    string _city = j.SelectToken("name").ToString();
                    string _lon = j.GetValue("coord").SelectToken("lon").ToString();
                    string _lat = j.GetValue("coord").SelectToken("lat").ToString();

                    Debug.Log("Sunrise city id is " + _cityid);
                    Debug.Log("Sunrise city is " + _city);
                    Debug.Log("Sunrise country is " + _country);
                    Debug.Log("Sunrise lon is " + _lon + ", lat is " + _lat);
                    Debug.Log("Sunrise time is " + ConvertTime(utcTime));
                }
            }
        }
    }
}