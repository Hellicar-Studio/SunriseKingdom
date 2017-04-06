﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using System.IO;

public class VideoPlayback : MonoBehaviour {

    public MediaPlayer[] media;
    public Renderer[] rend;
    //[HideInInspector]
    //public int maxVideos = 4;
    [HideInInspector]
    public float loadAtSeconds = 30f;
    //[HideInInspector]
    //public string fileExtension = ".mkv";
    [HideInInspector]
    public string videoFolder = "D:\\SunriseData/Images/";
    [HideInInspector]
    public string imagesFolder = "D:\\SunriseData/Images/";
    [HideInInspector]
    public bool beginPlayback = false;
    [HideInInspector]
    public bool firstPlayback = true;
    [HideInInspector]
    public bool debugActive = false;

    private float elapsedTime;
    private int item = 1;
    private bool[] isLoaded;
    private float timeExtension = 0f;
    private int extCount = 0;
    private bool filesExist = false;
    private bool screenshotTaken = false;

    // disables all object renderers
    public void RenderMaterial(bool _active)
    {
        // if renderer is active
        if (!_active)
        {
            // disable renderer if enabled
            if (rend[0].enabled) rend[0].enabled = false;
            if (rend[1].enabled) rend[1].enabled = false;
        }
    }

    private void CheckForVideoFiles(string[] _dataFolder)
    {
        if (debugActive) Debug.Log("Checking for video files...");


        if (_dataFolder == null)
        {
            if (debugActive) Debug.Log("File paths were empty!  Will check again in 60seconds.");
            filesExist = false;
            extCount++;

            if (extCount > 1)
            {
                timeExtension = 60f;
            }
            else
            {
                timeExtension = 0f;
            }
        }
        else
        {
            if (debugActive) Debug.Log("File paths are ready!");
            filesExist = true;
        }

        //DirectoryInfo di = new DirectoryInfo(_dataFolder);
        //int fileCount = di.GetFiles("*" + fileExtension).Length;

        //if (fileCount != maxVideos)
        //{
        //    filesExist = false;
        //    extCount++;

        //    if (extCount > 1)
        //    {
        //        timeExtension = 60f;
        //    }
        //    else
        //    {
        //        timeExtension = 0f;
        //    }

        //    if (debugActive) Debug.Log("Incorrect amount or no videos were found!");
        //}
        //else
        //{
        //    filesExist = true;
        //}

        //if (debugActive) Debug.Log("Videos files found " + fileCount + ".");
    }

    public void BeginPlayback()
    {
        if (filesExist)
        {
            // default variables
            item = 1;
            elapsedTime = 0f;
            isLoaded = new bool[media.Length];
            extCount = 0;

            // toggle renderers
            rend[0].enabled = true;
            rend[1].enabled = false;

            // load and playback media
            LoadVideo(0, 0, VideoRecord.mostRecentRecording);
            PlayVideo(0);

            // reset first playback flag
            firstPlayback = true;

            // toggle playback flag
            beginPlayback = true;
        }
        else
        {
            // check every 60 seconds
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= timeExtension)
            {
                CheckForVideoFiles(VideoRecord.mostRecentRecording); // Application.streamingAssetsPath);
                elapsedTime = 0f;
            }
        }
    }

    public void UpdatePlayer()
    {
        elapsedTime += Time.deltaTime;

        // gets current player and checks if it's finished
        int player = (item + 1) % 2;
        if (media[player].Control.IsFinished())
        {
            if (debugActive)
                Debug.Log("Player " + player + " has ended playback!");

            // advance to the next video
            // reset loop if we reach the end
            item++;
            if (item > VideoRecord.mostRecentRecording.Length-1) //  edited by JB
            {
                // disable screen capture
                firstPlayback = false;
                // reset items for looping
                item = 0;
            }

            if (item % 2 == 1)
            {
                rend[0].enabled = true;
                PlayVideo(0);

                rend[1].enabled = false;
                isLoaded[1] = false;
            }
            else if (item % 2 == 0)
            {
                rend[1].enabled = true;
                PlayVideo(1);

                rend[0].enabled = false;
                isLoaded[0] = false;
            }

            elapsedTime = 0f;
        }

        if (!media[0].Control.IsFinished() && (int)elapsedTime == loadAtSeconds && !isLoaded[1])
        {
            LoadVideo(1, item, VideoRecord.mostRecentRecording);
        }
        else if (!media[1].Control.IsFinished() && (int)elapsedTime == loadAtSeconds && !isLoaded[0])
        {
            LoadVideo(0, item, VideoRecord.mostRecentRecording);
        }

        if (firstPlayback && !screenshotTaken && (int)elapsedTime == 150)
        {
            CaptureScreenshot();
        }
        else if ((int)elapsedTime != 150)
        {
            if (screenshotTaken) screenshotTaken = false;
        }
    }

    private void CaptureScreenshot()
    {
        Application.CaptureScreenshot(imagesFolder + item + ".png");

        if (debugActive)
            Debug.Log("Screenshot " + item + ".png has been saved!");

        screenshotTaken = true;
    }

    private void LoadVideo(int player, int _item, string[] _filePath)
    {
        //int itemCorrected = _item + 1;
        //if (itemCorrected > maxVideos-1)
        //{
        //    itemCorrected = 0;
        //}

        string fileName = _filePath[_item]; //videoFolder + _item.ToString() + fileExtension;
        media[player].OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, fileName);
        media[player].m_AutoStart = false;
        isLoaded[player] = true;

        if (debugActive)
        {
            Debug.Log("Loading MediaPlayer " + player);
            //Debug.Log("Loading Video " + itemCorrected);
        }
    }

    private void PlayVideo(int _item)
    {
        int player = _item % 2;
        media[player].Play();

        if (debugActive)
        {
            Debug.Log("Playing MediaPlayer " + player);
        }
    }
}
