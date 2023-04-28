using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSkillCharacter_Book : PlayerWeaponSkillController
{
    public GameObject bookPrefab;
    public float bookAngle;
    // public float revertDistance;
    // public float revertDamage;
    public float NormalAttackDamage;
    public float HoldAttackDamage;
    public CharacterMovementController CharMoveCtrl;
    
    private float NormalAttackTime;
    private bool isHoldAttack = false;
    public float maxScale;
    public float rotationSpeed_NormalAttack;
    public float rotationSpeed_HoldAttack;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    private bool WithinBook = false;
    private float SkillCDRemain = 0.0F;



    // Start is called before the first frame update
    void Start()
    {
        PlayerStatus.CurrentPlayer.weaponStat.UpdateStat(gameObject);
        NormalAttackTime = 0;
        originalScale = bookPrefab.transform.localScale;
        originalPosition = bookPrefab.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {

        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            var controller = bookPrefab.GetComponent<BookController>();
            controller.isAttack = false;
        }

        if (SkillCDRemain > 0.0F) SkillCDRemain -= Time.deltaTime;

        if (isHoldAttack)
        {
            Vector3 temp_scale = bookPrefab.transform.localScale;
            Vector3 temp_position = bookPrefab.transform.localPosition;

            //Scale up the size of book
            temp_scale.x += Time.deltaTime;
            temp_scale.y += Time.deltaTime;
            temp_scale.z += Time.deltaTime;
            if (temp_scale.x > maxScale) temp_scale.x = maxScale;
            if (temp_scale.y > maxScale) temp_scale.y = maxScale;
            if (temp_scale.z > maxScale) temp_scale.z = maxScale;

            //fit the position from the character hand
            temp_position.x = temp_scale.x * 1.6f + 0.55f;
            temp_position.y = temp_scale.y * (-0.25f) + 0.6f;
            bookPrefab.transform.localScale = temp_scale;
            bookPrefab.transform.localPosition = temp_position;
            
        }

        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("NormalAttack"))
        {
            Vector3 temp = CharMoveCtrl.getPlayerPosition();
            //fit the action of character
            //swipe the book out in the first half of animation
            if (NormalAttackTime < Animator.GetCurrentAnimatorStateInfo(0).length / 2f)
            {
                bookPrefab.transform.RotateAround(temp, Vector3.up, -Time.deltaTime * rotationSpeed_NormalAttack);
            }
            //go back to the original position in the second half of anaimation
            if (NormalAttackTime >= Animator.GetCurrentAnimatorStateInfo(0).length / 2f)
            {
                bookPrefab.transform.RotateAround(temp, Vector3.up, Time.deltaTime * rotationSpeed_NormalAttack);
            }
            NormalAttackTime += Time.deltaTime;           
        }

        //reset the normal attack time 
        if (NormalAttackTime > 0 && Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            NormalAttackTime = 0;
        }

        //fit the hold attack animation and it will rotate around the character
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("HoldAttack"))
        {
            Vector3 temp = CharMoveCtrl.getPlayerPosition();
            bookPrefab.transform.RotateAround(temp, Vector3.up, -Time.deltaTime * rotationSpeed_HoldAttack);
        }

        //change it back the original size from the scale up size
        if (Animator.GetBool("EndAttack") && Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            bookPrefab.transform.localScale = originalScale;
            bookPrefab.transform.localPosition = originalPosition;
        }
        
    }


    public override void NormalAttack()
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            //Play the normal attck animation
            Animator.Play("NormalAttack");
            //set the damage for easily call the DoDamage API
            var controller = bookPrefab.GetComponent<BookController>();
            controller.Damage = NormalAttackDamage;
            controller.isAttack = true;
            controller.combatHandler = combatHandler;
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
        isHoldAttack = true;
        Animator.SetBool("EndAttack", false);
    }
    public override void EndHoldAttack()
    {
        //Play the hold attck animation
        Animator.Play("HoldAttack");
        Animator.SetBool("EndAttack", true);
        //set the damage for easily call the DoDamage API
        var controller = bookPrefab.GetComponent<BookController>();
        controller.Damage = HoldAttackDamage;
        controller.combatHandler = combatHandler;
        controller.isAttack = true;
        isHoldAttack = false;
    }

    public override float OnrecieveDamage(float damageAmount, CombatHandler attacker) 
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("rolling")) return 0;
        

        if (WithinBook)
            {


                var attackDir = (attacker.transform.position - this.transform.position).normalized;
                var attackAngle = Math.Atan2(attackDir.x, attackDir.z);
                var centerAngle = this.transform.rotation.eulerAngles.y;


                if (InBetween(centerAngle, bookAngle, attackAngle)) return 0;
        }
        Animator.Play("ReceiveDamage");
        return damageAmount;
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
