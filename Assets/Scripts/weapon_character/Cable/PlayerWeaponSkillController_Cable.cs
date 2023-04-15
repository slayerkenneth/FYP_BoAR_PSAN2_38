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
    public float GroundPoundRange = 1f;
    public LayerMask enemyLayer;
    public ParticleSystem ChargingEffect;

    private float SkillCDRemain = 0.0F;
    
    // Start is called before the first frame update
    void Start()
    {
        PlayerStatus.CurrentPlayer.weaponStat.UpdateStat(gameObject);
        ChargingEffect.Stop();
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
        ChargingEffect.Play();
        CableController.Aim();
    }

    public override void EndHoldAttack()
    {
        Animator.Play("Jump");
        CableController.DashAttack(BurstDamage);
        StartCoroutine(Dash());
        ChargingEffect.Stop();
    }

    public IEnumerator Dash()
    {
        var velocity = CharacterController.velocity;
        var locate = new Vector3(velocity.x, velocity.y, velocity.z);
        yield return new WaitForSeconds(0.2f);
        locate = new Vector3(velocity.x + transform.forward.x/2, velocity.y, velocity.z+ transform.forward.z/2);
        CharacterController.Move(locate);
        
        var EnemyBeingHit = Physics.OverlapSphere(transform.position, GroundPoundRange, enemyLayer);
        foreach (var enemy in EnemyBeingHit)
        {
            var enemyCombat = enemy.GetComponent<CombatHandler>();
            if (!enemyCombat) continue;
            combatHandler.DoDamage(enemyCombat, BurstDamage);
            Debug.Log("GP Damage!!");
            combatHandler.AddAttackingColliders(enemy);
        }
    }

    public override float OnrecieveDamage(float damageAmount, CombatHandler attacker)
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("rolling")) return 0;
        
        // Animator.Play("ReceiveDamage");
        return damageAmount;
    }
}
