using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FungShuiPassive", menuName = "PassiveClass/FungShui")]
public class FungShuiPassive : PassiveClass
{
    [SerializeField] private List<int> ExtraMoney;
    public override void EndPassive(GameObject player)
    {
        var landscapeList = GameObject.FindObjectsOfType<LandscapeController>();
        foreach (LandscapeController landscape in landscapeList)
        {
            landscape.OnCollect -= OnCollectLandscape;
        }
    }

    public override void StartPassive(GameObject player)
    {
        var landscapeList = GameObject.FindObjectsOfType<LandscapeController>();
        foreach (LandscapeController landscape in landscapeList)
        {
            landscape.OnCollect += OnCollectLandscape;
        }
    }

    private void OnCollectLandscape(object sender, System.EventArgs e)
    {
        PlayerStatus.CurrentPlayer.money += ExtraMoney[lv];
        Debug.Log("money: " + PlayerStatus.CurrentPlayer.money);
    }
}
