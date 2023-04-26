using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroToChapterSelect : MonoBehaviour
{

    void Start()
    {
        // Get the VideoPlayer component, which is inside a RawImage component
        VideoPlayer videoPlayer = GetComponentInChildren<VideoPlayer>();

        // Subscribe to the loopPointReached event of the VideoPlayer component
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Load the next scene
        SceneManager.LoadScene(2);
    }
}