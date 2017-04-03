using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;

public class PlaybackController : MonoBehaviour {

    public MediaPlayer[] media;
    public Renderer[] rend;
    //public Text timerUI;
    public float maxSeconds = 60f;
    public int maxVideos = 4;
    public float loadAtSeconds = 30f;
    public string fileExtension = ".mkv";
    public bool debugActive = false;

    private float elapsedTime;
    private int item = 1;
    private bool[] isLoaded;

    public void BeginPlayback()
    {
        // default variables
        item = 1;
        elapsedTime = 0f;
        isLoaded = new bool[media.Length];


        LoadVideo(0, 0);
        PlayVideo(0);
    }

    public void UpdatePlayer()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= maxSeconds)
        {
            item++;
            if (item > maxVideos - 1)
            {
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

        if (!media[0].m_Control.IsFinished() && (int)elapsedTime == loadAtSeconds && !isLoaded[1])
        {
            LoadVideo(1, item);
        }
        else if (!media[1].m_Control.IsFinished() && (int)elapsedTime == loadAtSeconds && !isLoaded[0])
        {
            LoadVideo(0, item);
        }

        //timerUI.text = (int)elapsedTime + "";
    }

    private void LoadVideo(int player, int _item)
    {
        int itemCorrected = _item + 1;
        if (itemCorrected > maxVideos-1)
        {
            itemCorrected = 0;
        }

        string fileName = _item.ToString() + fileExtension;
        media[player].OpenVideoFromFile(media[player].m_VideoLocation, fileName);
        media[player].m_AutoStart = false;
        isLoaded[player] = true;

        if (debugActive)
        {
            Debug.Log("Loading MediaPlayer " + player);
            Debug.Log("Loading Video " + itemCorrected);
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
