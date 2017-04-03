using UnityEngine;
using RenderHeads.Media.AVProVideo;
using System.Collections;

public class PlaybackController : MonoBehaviour {

    public MediaPlayer player;

    public CameraController cameraController;

    string lastVideo;

    string rootFolder = "D:\\SunriseNAS";

	// Use this for initialization
	void Start () {
        lastVideo = cameraController.getRecordingPath(rootFolder);
        if (lastVideo != "")
        {
            player.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, lastVideo, true);
        }
    }
	
	// Update is called once per frame
	void Update () {
        if(lastVideo != cameraController.getRecordingPath(rootFolder))
        {
            lastVideo = cameraController.getRecordingPath(rootFolder);
            player.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, lastVideo, true);
        }

    }
}
