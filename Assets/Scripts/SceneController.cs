using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadIntroScene()
    {
        SceneManager.LoadScene(1);
    }


    public void LoadChapterScene()
    {
        SceneManager.LoadScene(2);
    }

    public void LoadCharacterScene()
    {
        SceneManager.LoadScene(3);
    }

    public void LoadMapScene()
    {
        SceneManager.LoadScene(4);
    }
}


