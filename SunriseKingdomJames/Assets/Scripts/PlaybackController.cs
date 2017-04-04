using UnityEngine;
using RenderHeads.Media.AVProVideo;
using System.Collections;

public class PlaybackController : MonoBehaviour {

    public MediaPlayer player;

    public CameraController cameraController;

    string lastVideo;

    public int delaySeconds;
    private float timeLoadStarted;

	// Use this for initialization
	void Start () {
        lastVideo = "";
    }
	
	// Update is called once per frame
	void Update () {
        if(lastVideo != cameraController.newestPath)
        {
            lastVideo = cameraController.newestPath;
            StartCoroutine(loadVideoAfterDelay(delaySeconds));
        }
    }

    IEnumerator loadVideoAfterDelay(int _delayInSeconds)
    {
        timeLoadStarted = Time.time;
        while(Time.time - timeLoadStarted < _delayInSeconds)
        {
            yield return null;
        }
        player.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, lastVideo, true);
    }
}
