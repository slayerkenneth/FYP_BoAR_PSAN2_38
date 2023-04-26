using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroStoryDoubleSpeed : MonoBehaviour
{
    public RawImage rawImage;
    private VideoPlayer videoPlayer;
    private bool is2xSpeed = false;

    public void Start()
    {
        videoPlayer = rawImage.GetComponentInChildren<VideoPlayer>();
    }

    public void TogglePlaybackSpeed()
    {
        Debug.Log("TogglePlaybackSpeed called");
        if (is2xSpeed)
        {
            videoPlayer.playbackSpeed = 1f; 
            is2xSpeed = false;
        }
        else
        {
            videoPlayer.playbackSpeed = 2f;
            is2xSpeed = true;
        }
    }
}
