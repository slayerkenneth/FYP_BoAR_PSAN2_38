using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSkillController_Ipad : PlayerWeaponSkillController
{
    public GameObject ipadPrefab;
    public float revertDistance;
    public float revertDamage;
    public float NormalAttackDamage;
    public int MaxHitCount;

    private bool WithinShield = false;
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
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            Animator.Play("IpadNormalAttack");

            GameObject ipad = Instantiate(ipadPrefab);
            ipad.transform.position = new Vector3(20, 20, 20);
            var controller = ipad.GetComponent<IpadController>();
            controller.Damage = NormalAttackDamage;
            //controller.OriginWeanpon = Weapon;
            //controller.MaxHitCount = MaxHitCount;
            //controller.combatHandler = combatHandler;
            //combatHandler.AddAttackingColliders(shield.GetComponent<Collider>());
        }
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
        Animator.Play("IpadHoldAttack");
    }
    public override void EndHoldAttack()
    {
        var centerAngle = this.transform.rotation.eulerAngles.y;
        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemyList)
        {
            var dir = enemy.transform.position - transform.position;
            var angle = Math.Atan2(dir.x, dir.z);
            if (dir.magnitude < revertDistance && InBetween(centerAngle, shieldAngle, angle))
            {
                combatHandler.DoDamage(enemy.GetComponent<CombatHandler>(), revertDamage);
            }
        }

    }

    public override float OnrecieveDamage(float damageAmount, CombatHandler attacker) 
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("rolling")) return 0;
        

        //if (WithinShield)
        //    {


        //        var attackDir = (attacker.transform.position - this.transform.position).normalized;
        //        var attackAngle = Math.Atan2(attackDir.x, attackDir.z);
        //        var centerAngle = this.transform.rotation.eulerAngles.y;


        //        if (InBetween(centerAngle, shieldAngle, attackAngle)) return 0;
        //}
        Animator.Play("ReceiveDamage");
        return damageAmount;
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Enemy"))
        //{
        //    ValidHit = true;
        //    currentAttackingTarget = other.transform.GetComponent<CombatHandler>();
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        //ValidHit = false;
        // shd be after animation end and depends on collider of the weapon
    }

    private bool InBetween(double center, float range, double dir)
    {

        var upperBound1 = center + range / 2 - 360;
        var lowerBound1 = center - range / 2 - 360;
        var InBetween1 = upperBound1 > dir && dir > lowerBound1;

        var upperBound2 = center + range / 2;
        var lowerBound2 = center - range / 2;
        var InBetween2 = upperBound2 > dir && dir > lowerBound2;

        var upperBound3 = center + range / 2;
        var lowerBound3 = center - range / 2;
        var InBetween3 = upperBound3 > dir && dir > lowerBound3;

        return InBetween1 || InBetween2 || InBetween3;
    }

}
