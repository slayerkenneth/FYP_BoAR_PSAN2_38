using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BookStat", menuName = "WeaponStat/Book")]

public class BookStat : WeaponStat
{
    [SerializeField] private List<float> NormalAttackDamageLv = new List<float>();
    [SerializeField] private List<float> maxScaleLv = new List<float>();
    [SerializeField] private List<float> HoldAttackDamageLv = new List<float>();

    public override void UpdateStat(GameObject player) {
        var book = player.GetComponent<PlayerWeaponSkillCharacter_Book>();
        book.NormalAttackDamage = NormalAttackDamageLv[lv];
        book.maxScale = maxScaleLv[lv];
        book.HoldAttackDamage = HoldAttackDamageLv[lv];

    }
}
