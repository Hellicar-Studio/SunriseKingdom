using UnityEngine;
using RenderHeads.Media.AVProVideo;
using System.Collections;

public class PlaybackController : MonoBehaviour {

    public MediaPlayer currentPlayer;
    public MediaPlayer nextPlayer;

    public ApplyToMesh videoCanvas;

    public string vid1;
    public string vid2;

    public bool swapButton;
    public bool loadButton;
    public bool initialLoadButton;
    public bool playButton;

    public float nextVidIndex;
    public float totalVideos;

	// Use this for initialization
	void Start () {
        if (currentPlayer == null)
            currentPlayer = FindObjectOfType<MediaPlayer>();

        if (nextPlayer == null)
            nextPlayer = FindObjectOfType<MediaPlayer>();

        if (videoCanvas == null)
            videoCanvas = FindObjectOfType<ApplyToMesh>();

        loadInitialVideos();

        nextVidIndex = 2;
    }
	
	// Update is called once per frame
	void Update () {
        videoCanvas._media = currentPlayer;
        if(swapButton)
        {
            swapButton = false;
            swapVideo();
        }
        if(loadButton)
        {
            loadButton = false;
            loadVideoInNextPlayer();
        }
        if(playButton)
        {
            playButton = false;
            if(!currentPlayer.Control.IsPlaying()) 
                currentPlayer.Play();
        }
        if (initialLoadButton)
        {
            initialLoadButton = false;
            loadInitialVideos();
        }
        if (currentPlayer.Control.IsFinished())
        {
            swapVideo();
            //loadVideoInNextPlayer();
        }
    }

    void swapVideo()
    {
        MediaPlayer tmp;
        tmp = currentPlayer;
        currentPlayer = nextPlayer;
        nextPlayer = tmp;
        currentPlayer.Play();
    }

    void loadVideoInNextPlayer()
    {
        nextPlayer.CloseVideo();
        nextPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, vid2, false);
        advanceVideoStrings();
    }

    void advanceVideoStrings()
    {
        nextVidIndex++;
        nextVidIndex %= totalVideos;
        vid1 = vid2;
        vid2 = "Videos/Test" + nextVidIndex.ToString() + ".mkv";
    }

    void loadInitialVideos()
    {
        currentPlayer.CloseVideo();
        nextPlayer.CloseVideo();

        currentPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, vid1, false);
        nextPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, vid2, false);
    }

    void PlayCurrentPlayer()
    {
        currentPlayer.Play();
    }
}
