using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "ProfessorPassive", menuName = "PassiveClass/Professor")]
public class ProfessorPassive : PassiveClass
{

    [SerializeField] private List<int> UpgradeLv;

    public override void StartPassive(GameObject player)
    {
        player.GetComponent<CombatHandler>().UpgradeDamagePercentage = UpgradeLv[lv];    
    }

    public override void EndPassive(GameObject player)
    {
        player.GetComponent<CombatHandler>().UpgradeDamagePercentage = 100;    
    }


}