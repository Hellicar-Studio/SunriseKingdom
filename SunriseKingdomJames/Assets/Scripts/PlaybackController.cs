using UnityEngine;
using System.Collections;

public class PlaybackController : MonoBehaviour {

    RenderHeads.Media.AVProVideo.MediaPlayer player;

    public bool swapVideo;
    public string vid1;
    public string vid2;
	// Use this for initialization
	void Start () {
        if (player == null)
            player = FindObjectOfType<RenderHeads.Media.AVProVideo.MediaPlayer>();

        player.
	}
	
	// Update is called once per frame
	void Update () {

	}
}
