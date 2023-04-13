using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IpadStat", menuName = "WeaponStat/Ipad")]

public class IpadStat : WeaponStat
{
    [SerializeField] private List<float> NormalAttackDamageLv = new List<float>();
    [SerializeField] private List<int> MaxHitCountLv = new List<int>();

    public override void UpdateStat(GameObject player)
    {
        var ipad = player.GetComponent<PlayerWeaponSkillController_Ipad>();
        ipad.NormalAttackDamage = NormalAttackDamageLv[lv];
        ipad.MaxHitCount = MaxHitCountLv[lv];
    }
}
