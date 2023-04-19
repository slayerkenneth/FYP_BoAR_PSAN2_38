using DigitalRuby.LightningBolt;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSkillController_Ipad : PlayerWeaponSkillController
{
    public GameObject ipadPrefab;
    public float NormalAttackDamage = 10;
    public int MaxHitCount = 3;
    public float HoldAttackperSec = 10;
    public GameObject lightningPrefab;
    public float MaxLightningRange;             //the max range for one lightning, the length of whole chain can be larger.

    private float SkillCDRemain = 0.0F;
    private List<LightningBoltScript> lightnings = new List<LightningBoltScript>();
    private List<CombatHandler> enemies = new List<CombatHandler>();
    private bool Hold = false;

    // Start is called before the first frame update
    void Start()
    {
        PlayerStatus.CurrentPlayer.weaponStat.UpdateStat(gameObject);
        for (int i = 0; i < MaxHitCount; i++) {
            var lightning = Instantiate(lightningPrefab);
            lightning.SetActive(false);
            var script = lightning.GetComponent<LightningBoltScript>();
            script.enable = false;
            lightnings.Add(script);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SkillCDRemain > 0.0F) SkillCDRemain -= Time.deltaTime;
        //foreach (var enemy in enemies) {
        //    combatHandler.DoDamage(enemy, HoldAttackperSec * Time.deltaTime);
        //}
        if(Hold) recursiveHoldAttack(gameObject, null, MaxHitCount);
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
            controller.direction = transform.forward;
            controller.OriginWeanpon = Weapon;
            controller.combatHandler = combatHandler;
            combatHandler.AddAttackingColliders(ipad.GetComponent<Collider>());
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
        if (lightnings.Count != MaxHitCount) {
            lightnings.Clear();
            for (int i = 0; i < MaxHitCount; i++)
            {
                var lightning = Instantiate(lightningPrefab);
                var script = lightning.GetComponent<LightningBoltScript>();
                script.enable = false;
                lightnings.Add(script);
            }
        }
        for (int i = 0; i < MaxHitCount; i++)
        {
            lightnings[i].gameObject.SetActive(true);
        }
        Hold = true;
    }
    public override void EndHoldAttack()
    {
        for (int i = 0; i < MaxHitCount; i++)
        {
            lightnings[i].enable = false;
        }
        //enemies.Clear();
        for (int i = 0; i < MaxHitCount; i++)
        {
            lightnings[i].gameObject.SetActive(false);
        }
        Hold = false;
    }

    public override float OnrecieveDamage(float damageAmount, CombatHandler attacker) 
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("rolling")) return 0;
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

    private void recursiveHoldAttack(GameObject current, GameObject previous, int depth) {
        if (depth == 0) return;

        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");

        GameObject minTarget = null;
        var minDistance = float.PositiveInfinity;
        foreach (GameObject enemy in enemyList)
        {
            if (enemy == previous || enemy == current) continue;
            var dir = enemy.transform.position - current.transform.position;
            if (dir.magnitude < minDistance)
            {
                minDistance = dir.magnitude;
                minTarget = enemy;
            }
        }


        if (minTarget != null && MaxLightningRange > minDistance)
        {
            lightnings[MaxHitCount - depth].enable = true;
            lightnings[MaxHitCount - depth].StartObject = current;
            lightnings[MaxHitCount - depth].EndObject = minTarget;
            //enemies.Add(minTarget.GetComponent<CombatHandler>());

            combatHandler.DoDamage(minTarget.GetComponent<CombatHandler>(), HoldAttackperSec * Time.deltaTime);
            recursiveHoldAttack(minTarget, current, depth - 1);
        }
    }
}
