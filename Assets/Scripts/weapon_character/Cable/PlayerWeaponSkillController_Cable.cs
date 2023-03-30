using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSkillController_Cable : PlayerWeaponSkillController
{
    public CableController CableController;

    public float singleHitDamage = 10f;
    public float HitRange = 2f;
    public float BurstDamage = 30f;
    
    private float SkillCDRemain = 0.0F;
    // Start is called before the first frame update
    void Start()
    {
        PlayerStatus.CurrentPlayer.weaponStat.UpdateStat(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void NormalAttack()
    {
        if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) return; //anim state name not correct
        
        Animator.Play("CableNormal"); //anim state name not correct
        CableController.AreaElectricAttack();
    }

    public override void Rolling()
    {
        throw new System.NotImplementedException();
    }

    public override void CastSkill()
    {
        if (SkillCDRemain <= 0.0F)
        {
            SkillCDRemain = PlayerStatus.CurrentPlayer.activeClass.skillCD;
            PlayerStatus.CurrentPlayer.activeClass.skill(gameObject);
        }
    }

    public override void StartHoldAttack()
    {
        throw new System.NotImplementedException();
    }

    public override void EndHoldAttack()
    {
        throw new System.NotImplementedException();
    }

    public override float OnrecieveDamage(float damageAmount, CombatHandler attacker)
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("rolling")) return 0;
        
        Animator.Play("ReceiveDamage");
        return damageAmount;
    }
}
