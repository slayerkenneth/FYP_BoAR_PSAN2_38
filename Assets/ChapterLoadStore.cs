using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterLoadStore : MonoBehaviour
{
    public static int selectedChapter;

    private void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void SetSelectedChapter(int chap)
    {
        selectedChapter = chap;
    }
}
