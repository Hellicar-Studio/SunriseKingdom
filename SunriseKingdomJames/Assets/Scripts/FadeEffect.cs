using UnityEngine;
using System.Collections;
using RenderHeads.Media.AVProVideo;


[ExecuteInEditMode]
public class FadeEffect : MonoBehaviour
{
    public GameObject player;
    private MediaPlayer mediaPlayer;

    private Material material;

    // Creates a private material used to the effect
    void Awake()
    {
        material = new Material(Shader.Find("Custom/fade"));
    }

    // Postprocess the image
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        float percentage = 0;
        if(mediaPlayer != null)
        {
            float duration = mediaPlayer.Info.GetDurationMs();
            float currentPosition = mediaPlayer.Control.GetCurrentTimeMs();
            percentage = currentPosition / duration;
        }
        else
        {
            mediaPlayer = player.GetComponent<MediaPlayer>();
        }

        material.SetFloat("_Percentage", percentage);
        Graphics.Blit(source, destination, material);
    }
}