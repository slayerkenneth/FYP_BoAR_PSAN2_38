using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BackpackStat", menuName = "WeaponStat/Backpack")]

public class BackpackStat : WeaponStat
{
    [SerializeField] private List<float> NormalAttackDamageLv = new List<float>();
    [SerializeField] private List<int> MaxHitCountLv = new List<int>();
    [SerializeField] private List<float> revertDamageLv = new List<float>();

    public override void UpdateStat(GameObject player) {
        var backpack = player.GetComponent<PlayerWeaponSkillController_Backpack>();
        backpack.NormalAttackDamage = NormalAttackDamageLv[lv];
        backpack.MaxHitCount = MaxHitCountLv[lv];
        backpack.revertDamage = revertDamageLv[lv];

    }
}
