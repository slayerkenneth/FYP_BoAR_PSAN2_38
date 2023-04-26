using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviour
{
    public CharacterList characterList;
    public TextMeshProUGUI charNameText;
    public Image artworkSprite;
    private int selectedChar = 0;
    // Start is called before the first frame update
    void Start()
    {
        // to check whether player has selected Character in previous session
        if (!PlayerPrefs.HasKey("selectedChar"))
        {
            selectedChar = 0;
        }
        else
        {
            Load();
        }
        UpdateCharacter(selectedChar);
    }
    public void NextChar()
    {
        selectedChar++;
        if(selectedChar >= characterList.charCount)
        {
            selectedChar = 0;
        }
        UpdateCharacter(selectedChar);
        Save();
    }

    public void BackOption()
    {
        selectedChar--;
        if (selectedChar < 0)
        {
            selectedChar = characterList.charCount - 1;
        }
        UpdateCharacter(selectedChar);
        Save();
    }

    private void UpdateCharacter(int selectedChar)
    {
        CharacterSelection character = characterList.GetCharacter(selectedChar);
        artworkSprite.sprite = character.charSprite;
        charNameText.text = character.charName;
    }

    // to save selected character
    private void Load()
    {
        selectedChar = PlayerPrefs.GetInt("selectedChar");
    }

    private void Save()
    {
        PlayerPrefs.SetInt("selectedChar", selectedChar);

    }


}

