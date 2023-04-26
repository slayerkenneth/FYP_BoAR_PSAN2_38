using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BottleStat", menuName = "WeaponStat/Bottle")]

public class BottleStat : WeaponStat
{
    [SerializeField] private List<float> NormalAttackDamageLv = new List<float>();
    [SerializeField] private List<float> NormalAttackRangeLv = new List<float>();
    [SerializeField] private List<float> HoldDamagePreSecLv = new List<float>();

    public override void UpdateStat(GameObject player) {
        var bottle = player.GetComponent<PlayerWeaponSkillController_Bottle>();
        bottle.grenadeDamage = NormalAttackDamageLv[lv];
        bottle.grenadeDistance = NormalAttackRangeLv[lv];
        bottle.damagePerSec = HoldDamagePreSecLv[lv];

    }
}
