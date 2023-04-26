using System.Collections;
using System.Collections.Generic;
using CodeMonkey.HealthSystemCM;
using UnityEngine;
using Assets.Scripts;

public class CombatHandler : MonoBehaviour
{
    // Low Level Commands for combat related functionalities
    // Every Enemy, Player, maybe DTower, and anything interact with Central Battle Controller have this component
    [Header("Battle Parameters")] 
    [SerializeField] private float hp;
    [SerializeField] private float shield;
    [SerializeField] private float skillCoolDown;
    public float UpgradeDamagePercentage = 100;
    [SerializeField] private List<Collider> AttackingColliders;
    [SerializeField] private List<Collider> DamageTakingColliders;
    [SerializeField] private CombatHandler AttackTarget;
    [SerializeField] private CombatHandler DamageSource;
    [SerializeField] private HealthSystemComponent healthSystemComponent;
    [SerializeField] private float extraDamage = 0.0F;


    [Header("Reference")] 
    [SerializeField] private ARController ARCtrl;
    [SerializeField] private GameFlowController GameFlowCtrl;
    [SerializeField] private CentralBattleController CentralBattleCtrl;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setExtraDamage(float extra) {
        extraDamage = extra;
    }

    public void resetExtraDamage() {
        extraDamage = 0.0F;
    }

    public void DoDamage(CombatHandler targetHitTarget, float damageAmount)
    {
        CentralBattleCtrl.DamageTransfer(targetHitTarget, damageAmount + extraDamage, this);
    }

    public void ReceiveDamage(float damageAmount, CombatHandler attacker)
    {
        float modifiedDamageAmount;
        PlayerWeaponSkillController playerWeaponSkillController;
        if (TryGetComponent<PlayerWeaponSkillController>(out playerWeaponSkillController))
        {
            modifiedDamageAmount = playerWeaponSkillController.OnrecieveDamage(damageAmount, attacker);
        }
        else {
            modifiedDamageAmount = damageAmount;
        }
        
        shield -= modifiedDamageAmount;
        if (shield < 0)
        {
            hp += shield;
            shield = 0;

            if (hp < 0)
            {
                hp = 0;
            }
        }

        healthSystemComponent.GetHealthSystem().Damage(modifiedDamageAmount);
    }

    public void ReceiveHeal(float healAmount)
    {
        
        var tempHP = hp + healAmount;
        var maxHP = healthSystemComponent.GetHealthSystem().GetHealthMax();
        if (tempHP > maxHP)
        {
            hp = maxHP;
        }
        else hp = tempHP;
        healthSystemComponent.GetHealthSystem().Heal(healAmount);
    }

    public void ReceiveShield(float shieldAmount)
    {

        shield += shieldAmount;
        healthSystemComponent.GetHealthSystem().Shield(shieldAmount);
    }

    public float GetCurrentHP()
    {
        return hp;
    }

    public float GetCurrentShield()
    {
        return shield;
    }

    // Only called in initialization
    public void InitHP(float initHP)
    {
        //hp = initHP;
        hp = PlayerStatus.CurrentPlayer.currentHP;
    }

    public void SetCentralCombatHandler(CentralBattleController cbc)
    {
        CentralBattleCtrl = cbc;
    }

    public CentralBattleController GetCentralCombatHandler()
    {
        return CentralBattleCtrl;
    }

    public List<Collider> GetAttackingColliders()
    {
        return AttackingColliders;
    }

    public void AddAttackingColliders(Collider collider)
    {
        AttackingColliders.Add(collider);
    }
    
    public List<Collider> GetDamageReceivingColliders()
    {
        return DamageTakingColliders;
    }

    public HealthSystemComponent GetHealthSystemComponent()
    {
        return healthSystemComponent;
    }
}
