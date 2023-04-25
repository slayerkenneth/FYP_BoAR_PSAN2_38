using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class BattlePlayerIconLoader : MonoBehaviour
{
    [SerializeField] public Image ActiveIconImage;
    public List<Sprite> playerIconSpites;

    private void Start()
    {
        ActiveIconImage = GetComponent<Image>();
    }

    public void SetPlayerIcon()
    {
        int playerIndex = PlayerPrefs.GetInt("selectedChar");
        ActiveIconImage.sprite = playerIconSpites[playerIndex];
    }
}
