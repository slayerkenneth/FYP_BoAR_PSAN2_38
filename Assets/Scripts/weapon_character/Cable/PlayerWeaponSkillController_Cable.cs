using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSkillController_Cable : PlayerWeaponSkillController
{
    public CableController CableController;
    public CharacterController CharacterController;

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
        if (SkillCDRemain > 0.0F) SkillCDRemain -= Time.deltaTime;
    }

    public override void NormalAttack()
    {
        if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) return; //anim state name not correct
        
        Animator.Play("SingleHit"); //anim state name not correct
        CableController.AreaElectricAttack(singleHitDamage, HitRange);
    }

    public override void Rolling()
    {
        
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
        Animator.Play("Idle_Shoot_Ar");
        CableController.Aim();
    }

    public override void EndHoldAttack()
    {
        Animator.SetTrigger("Release");
        CableController.DashAttack(BurstDamage);
    }

    public override float OnrecieveDamage(float damageAmount, CombatHandler attacker)
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("rolling")) return 0;
        
        // Animator.Play("ReceiveDamage");
        return damageAmount;
    }
}
