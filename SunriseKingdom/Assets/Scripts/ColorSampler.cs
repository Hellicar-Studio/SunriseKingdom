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

    [HideInInspector]
    public Color[] colors;

	// Use this for initialization
	void Start () {
        ResetColorsButton = false;
        colors = new Color[365];
        colors = PlayerPrefsX.GetColorArray(colorsKey);
        currentDay = PlayerPrefs.GetInt(currentDayKey);
        Debug.Log("Colors After Load" + colors[0]);

        if (!cam)
            cam = FindObjectOfType<Camera>();

        printColors();
    }

    // Print all the filled colors so far
     public void printColors()
    {
        for(int i = 0; i < currentDay; i++)
        {
            Debug.Log("Day: " + i + " " + colors[i]);
        }
    }

    // Reset all colors to white and set the current day back to 0
    public void resetAll()
    {
        for(int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(0, 0, 0);
        }
        currentDay = 0;
        saveSettings();
    }

    // Wrapper method for saving a new color
    public Color saveNewColor(Texture2D tex)
    {
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

    // Save the color array and the currentDay value to the Player Prefs
    public bool saveSettings()
    {
        PlayerPrefs.SetInt(currentDayKey, currentDay);
        return PlayerPrefsX.SetColorArray(colorsKey, colors);
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
