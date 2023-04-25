using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadStartMenu()
    {
        SceneManager.LoadScene(0);
    }

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

    public void LoadDialogue(int dialogueIndex)
    {
        SceneManager.LoadScene(dialogueIndex + 4);
    }

    public void LoadDialogueSceneFromStaticIndex()
    {
        SceneManager.LoadScene(ChapterLoadStore.selectedChapter + 4);
    }
}


