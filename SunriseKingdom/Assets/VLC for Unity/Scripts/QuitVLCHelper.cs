using UnityEngine;
using System.Collections;

public class QuitVLCHelper : MonoBehaviour {

    private PlayVLC[] videos;

	void Start () {
	    videos = GameObject.FindObjectsOfType<PlayVLC>();
	}

    public void QuitAllVideos() {
        foreach (PlayVLC video in videos) {
            video.StopVideo();
        }
    }

    public void QuitApplication() {
        foreach (PlayVLC video in videos) {
            video.StopVideo();
        }

        Application.Quit();
    }

    void OnApplicationQuit() {
        foreach (PlayVLC video in videos)
        {
            video.StopVideo();
        }

    }
}
