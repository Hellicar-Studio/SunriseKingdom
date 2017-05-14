// Written By James Bentley 28/4/17
// Samples a new colros from an input texture and saves the color value into a player preferences array.
// You can adjust the position the color is sampled from using the samplePos and sampleSize fields. A debug 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ColorSampler : MonoBehaviour {

    public int currentDay;

    public Vector2 samplePos = new Vector2(-50, -50);
    public Vector2 sampleSize = new Vector2(100, 100);
    public int step;
    public Camera cam;

    public bool ResetColorsButton;

    public string newestColorHex;

    public string colorsKey = "SunriseColors";
    public string currentDayKey = "CurrentDay";
    public string timeKey = "SunriseTimes";

    //[HideInInspector]
    public Color[] colors;
    //[HideInInspector]
    public float[] times;

    // Use this for initialization
    void Start () {
        ResetColorsButton = false;
        colors = PlayerPrefsX.GetColorArray(colorsKey);
        times = PlayerPrefsX.GetFloatArray(timeKey);
        currentDay = PlayerPrefs.GetInt(currentDayKey);

        if (!cam)
            cam = FindObjectOfType<Camera>();

        if (colors.Length < 1 || times.Length < 1)
        {
            Debug.Log("Initializing Colors!");
            colors = new Color[365];
            times = new float[365];
            for (int i = 0; i < 365; i++)
            {
                colors[i] = new Color(1, 1, 1);
                times[i] = 0;
            }
            currentDay = 0;
            PlayerPrefsX.SetColorArray(colorsKey, colors);
            PlayerPrefsX.SetFloatArray(timeKey, times);
            PlayerPrefs.SetInt(currentDayKey, 0);
        }

        //printColors();

        //visController.enabled = false;

        //printColors();

        //float bonus = 0.1f;

        //float RMin = 0.7f + bonus;
        //float RMax = 0.9f + bonus;
        //float GMin = 0.32f + bonus;
        //float GMax = 0.6f + bonus;
        //float BMin = 0.0f + bonus;
        //float BMax = 0.39f + bonus;

        //for (int j = 0; j < colors.Length; j++)
        //{
        //    if (j % 2 == 0)
        //        colors[j] = new Color(Random.Range(RMin, RMax), Random.Range(GMin, GMax), Random.Range(BMin, BMax), 1);
        //    else
        //        colors[j] = new Color(Random.Range(1 - RMin, 1 - RMax), Random.Range(1 - GMin, 1 - GMax), Random.Range(1 - BMin, 1 - BMax), 1);
        //}

        //for (int i = 0; i < times.Length; i++)
        //{
        //    times[i] = Random.Range(240, 375);
        //}
    }

    // Print all the filled colors so far
    public void printColors()
    {
        for(int i = 0; i < colors.Length; i++)
        {
            Debug.Log("Day: " + i + " " + colors[i]);
        }
        for(int i = 0; i < times.Length; i++)
        {
            Debug.Log("Time: " + i + " " + times[i]);
        }
    }

    // Reset all colors to white and set the current day back to 0
    public void resetAll()
    {
        for(int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(1, 1, 1);
        }
        for (int i = 0; i < times.Length; i++)
        {
            times[i] = 0;
        }
        currentDay = 0;
        saveSettings();
        loadSettings();
    }

    // Wrapper method for saving a new color
    public Color saveNewColor(Texture2D tex)
    {
        if (currentDay == 365)
            resetAll();
        Debug.Log("Saved Starting!");
        Color col = getNewColor(tex);
        colors[currentDay] = col;
        currentDay++;
        currentDay %= colors.Length;
        bool saved = saveSettings();
        if (!saved)
            Debug.Log("Error Saving the Color Array!");
        return col;
    }

    public void saveSunriseTime(string sunriseTime)
    {
        string[] splitTime = sunriseTime.Split(':');
        int hours = 0;
        if(splitTime.Length > 1)
        {
            bool parsed = int.TryParse(splitTime[0], out hours);
            if (!parsed)
            {
                Debug.Log("Failed to Parse hours! setting to default value: 0");
                hours = 0;
            } else
            {
                hours *= 60;
            }
            int minutes = 0;
            parsed = int.TryParse(splitTime[1], out minutes);
            if (!parsed)
            {
                Debug.Log("Failed to Parse minutes! setting to default value: 0");
                minutes = 0;
            }
            else
            {
                times[currentDay] = hours + minutes;
                saveSettings();
            }
        } else
        {
            Debug.Log("save Sunrise Time did not get a good time!");
        }
    }


    // Save the color array and the currentDay value to the Player Prefs
    public bool saveSettings()
    {
        PlayerPrefs.SetInt(currentDayKey, currentDay);
        bool savedTimes = PlayerPrefsX.SetFloatArray(timeKey, times);
        bool savedColors = PlayerPrefsX.SetColorArray(colorsKey, colors);
        return (savedTimes && savedColors);
    }

    // Load the color array and the currentDay value to the Player Prefs
    public void loadSettings()
    {
        currentDay = PlayerPrefs.GetInt(currentDayKey, 0);
        times = PlayerPrefsX.GetFloatArray(timeKey);
        colors = PlayerPrefsX.GetColorArray(colorsKey);
    }

    // Find the Next Color
    public Color getNewColor(Texture2D tex)
    {
        Color[] pix = tex.GetPixels(tex.width / 2 + (int)samplePos.x, tex.height / 2 + (int)samplePos.y, (int)sampleSize.x, (int)sampleSize.y);

        Color avg = Color.black;
        for (int j = 0; j < pix.Length; j += step)
        {
            avg += pix[j] * step / pix.Length;
        }

        return avg;
    }

    // Update is called once per frame
    void Update()
    {
        drawDebugRect(samplePos.x, samplePos.y, sampleSize.x, sampleSize.y);

        if(ResetColorsButton)
        {
            printColors();
            Debug.Log("Resetting These Colors!");
            resetAll();
            printColors();
            Debug.Log("Colors now Reset!");
            ResetColorsButton = false;
        }
    }

    // Draw a rectangle outlining the area which we are sampling the color from.
    private void drawDebugRect(float x, float y, float width, float height)
    {
        x *= cam.orthographicSize / (Screen.height / 2);
        y *= cam.orthographicSize / (Screen.height / 2);
        width *= cam.orthographicSize / (Screen.height / 2);
        height *= cam.orthographicSize / (Screen.height / 2);

        Vector3 topLeft = new Vector3(x, y, 0);
        Vector3 topRight = new Vector3(x + width, y, 0);
        Vector3 bottomLeft = new Vector3(x, y + height, 0);
        Vector3 bottomRight = new Vector3(x + width, y + height, 0);

        Debug.DrawLine(topLeft, topRight, Color.red);
        Debug.DrawLine(topLeft, bottomLeft, Color.red);
        Debug.DrawLine(topRight, bottomRight, Color.red);
        Debug.DrawLine(bottomLeft, bottomRight, Color.red);
    }
}
