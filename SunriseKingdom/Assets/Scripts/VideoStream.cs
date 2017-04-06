//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class VideoStream : MonoBehaviour {

    [HideInInspector] 
    public bool isFolderClear = false;
    [HideInInspector]
    public float recordingMaxSeconds;

    private Renderer rend;
    private int index = 0;

    // based on the frame number, string is padded with zeros
    // this is to keep frames in correct sequential order
    private string FileNamePadding(int _frame)
    {
        string s;

        if (_frame <= 9)
        {
            s = "000000" + _frame;
        }
        else if (_frame >= 10 && _frame <= 99)
        {
            s = "00000" + _frame;
        }
        else if (_frame >= 100 && _frame <= 999)
        {
            s = "0000" + _frame;
        }
        else if (_frame >= 1000 && _frame <= 9999)
        {
            s = "000" + _frame;
        }
        else if (_frame >= 10000 && _frame <= 99999)
        {
            s = "00" + _frame;
        }
        else if (_frame >= 100000 && _frame <= 999999)
        {
            s = "0" + _frame;
        }
        else
        {
            s = "" + _frame;
        }

        return s;
    }

    // toggles object renderer
    public void RenderMaterial(bool _active)
    {
        // checks if rend is null and if yes, reference
        if (rend == null)
            rend = GetComponent<Renderer>();
        else
        {
            // if renderer is active
            if (_active)
            {
                // enable renderer if disabled
                if (!rend.enabled)
                    rend.enabled = true;
            }
            else
            {
                // disable renderer if enabled
                if (rend.enabled)
                    rend.enabled = false;
            }
        }
    }

    // clears save folder and resets file index
    public void ClearFolder(string _folderName)
    {        
        // reset index
        if (index != 0) index = 0;

        // access save folder directory
        DirectoryInfo dir = new DirectoryInfo(_folderName);
        // delete all files that exist
        foreach(FileInfo fi in dir.GetFiles())
        {
            fi.Delete();
        }

        // folder has been cleared!
        if (!isFolderClear) isFolderClear = true;
    }

    // much faster super awesome yes yes!
    public void VideoRecord(string _folderName, float _fps)
    {
        //Time.captureFramerate = (int)_fps;

        // frame index at the rate of frames per second
//        index = (int)(Time.time * _fps);
//        index = index % (int)(_fps * recordingMaxSeconds);
            
        Application.CaptureScreenshot(_folderName + "/image_" + FileNamePadding(index) + ".png", 1);
        index++;
    }
}