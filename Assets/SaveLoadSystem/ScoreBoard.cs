using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreBoard : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public float currentTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        scoreText.text = "Score: " + currentTime.ToString();
    }

    public void LoadScore() {
        SaveData data = SaveSystem.LoadSave();
        if (data != null) currentTime = data.score;
    }

    public void SaveScore()
    {
        SaveSystem.SaveScore((int)currentTime);
    }

    public void NextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (SceneManager.sceneCount > nextSceneIndex)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
