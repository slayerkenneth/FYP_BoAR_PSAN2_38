using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoldRiceBowlActive", menuName = "ActiveClass/GoldRiceBowl")]
public class GoldRiceBowlActive : ActiveClass
{
    [SerializeField] private List<float> Duration;
    [SerializeField] private List<float> HealAmount;
    [SerializeField] private float HealAreaSize = 1;
    [SerializeField] private GameObject HealArea;

    public override void skill(GameObject player)
    {
        var ARCtrl = GameObject.FindObjectsOfType<ARController>()[0];
        var gameboard = ARCtrl.GetActiveGameboard();
        if (gameboard == null) return;

        Vector3 pos;
        while (gameboard.FindRandomPosition(out pos))
        {
            if (gameboard.CheckFit(pos, 0.01f))
            {
                var healArea = Instantiate(HealArea, pos, Quaternion.identity);
                var Controller = healArea.GetComponent<HealAreaController>();
                Controller.duration = Duration[lv];
                Controller.healPerSec = HealAmount[lv];
                Controller.range = HealAreaSize;
                break;
            }
            else continue;
        }
    }
}
