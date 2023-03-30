using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSkillController_Bottle : PlayerWeaponSkillController
{
    public GameObject bottlePrefab; 
    public GameObject lightSaber;
    public GameObject grenadePrefab;
    public float grenadeDamage = 20;
    public float grenadeDistance = 2;
    public float beamDistance;
    public float damagePerSec;

    private bool WithinLaser = false;
    private float SkillCDRemain = 0.0F;

    public Vector3 rotation;

    // Start is called before the first frame update
    void Start()
    {
        PlayerStatus.CurrentPlayer.weaponStat.UpdateStat(gameObject);
        var laser = lightSaber.GetComponent<LaserController>();
        laser.damagePerSec = damagePerSec;
        laser.combatHandler = combatHandler;

        bottlePrefab.transform.localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        if (SkillCDRemain > 0.0F) SkillCDRemain -= Time.deltaTime;
        if (WithinLaser)
        {
            lightSaber.transform.position = bottlePrefab.transform.position;
            var pos = lightSaber.transform.localPosition;
            pos.z += beamDistance / 2 + 0.1F;
            lightSaber.transform.localPosition = pos;
            //bottlePrefab.transform.localRotation = Quaternion.Euler(-100, -25, 0);
        }
    }


    public override void NormalAttack()
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            Animator.Play("throw");

            GameObject grenade = Instantiate(grenadePrefab);
            grenade.transform.position = new Vector3(20, 20, 20);
            var controller = grenade.GetComponent<BottleController>();
            controller.Damage = grenadeDamage;
            controller.OriginWeanpon = Weapon;
            controller.damageDistance = grenadeDistance;
            controller.character = this.gameObject;
            controller.combatHandler = combatHandler;
            combatHandler.AddAttackingColliders(controller.Collider);
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
        WithinLaser = true;
        Animator.Play("hold");
        Animator.SetBool("Hold", true);
        lightSaber.SetActive(true);
        //bottlePrefab.transform.rotation = Quaternion.Euler(rotation);
        //lightSaber.transform.eulerAngles = Vector3.zero;
        extendLightSaber(beamDistance);
    }

    private void extendLightSaber(float length) {
        var scale = lightSaber.transform.localScale;
        scale.y = length / 30 * 15;
        lightSaber.transform.localScale = scale;
        bottlePrefab.transform.localRotation = Quaternion.Euler(-100, -25, 0);
        bottlePrefab.transform.localPosition -= new Vector3(0, 0.3F, 0.5F);
        lightSaber.transform.position = bottlePrefab.transform.position;
        var pos = lightSaber.transform.localPosition;
        pos.z += length / 2 + 0.1F;
        lightSaber.transform.localPosition = pos;
        //var angle = Quaternion.FromToRotation(bottlePrefab.transform.up, new Vector3(transform.forward.x, 0, transform.forward.z));
        //var angle = Quaternion.FromToRotation(bottlePrefab.transform.up, new Vector3(1, 0, 0));
        //var angle = new Quaternion(0,0.26823F,0,0.9633F);
        //Debug.Log("rotation: " + angle);
        //bottlePrefab.transform.localRotation = angle;
    }
    private void shrinkLightSaber(float length)
    {

        //var pos = lightSaber.transform.localPosition;
        //pos.z -= length / 2;
        //lightSaber.transform.localPosition = pos;
    }

    public override void EndHoldAttack()
    {
        WithinLaser = false;
        shrinkLightSaber(beamDistance);
        lightSaber.SetActive(false);
        Animator.SetBool("Hold", false);
        bottlePrefab.transform.localRotation = Quaternion.identity;
        bottlePrefab.transform.localPosition += new Vector3(0F, 0.3F, 0.5F);

    }

    public override float OnrecieveDamage(float damageAmount, CombatHandler attacker) 
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("rolling")) return 0;
        

        if (WithinLaser)
            {


                var attackDir = (attacker.transform.position - this.transform.position).normalized;
                var attackAngle = Math.Atan2(attackDir.x, attackDir.z);
                var centerAngle = this.transform.rotation.eulerAngles.y;


                //if (InBetween(centerAngle, shieldAngle, attackAngle)) return 0;
        }
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

}
