using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CableStat", menuName = "WeaponStat/Cable")]

public class CableStat : WeaponStat
{
    [SerializeField] private List<float> NormalAttackDamageLv = new List<float>();
    [SerializeField] private List<float> NormalAttackRangeLv = new List<float>();
    [SerializeField] private List<float> HoldDamagePreSecLv = new List<float>();

    public override void UpdateStat(GameObject player) {
        var cable = player.GetComponent<PlayerWeaponSkillController_Cable>();
        cable.singleHitDamage = NormalAttackDamageLv[lv];
        cable.HitRange = NormalAttackRangeLv[lv];
        cable.BurstDamage = HoldDamagePreSecLv[lv];

    }
}