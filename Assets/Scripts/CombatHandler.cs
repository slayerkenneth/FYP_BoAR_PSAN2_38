using System.Collections;
using System.Collections.Generic;
using CodeMonkey.HealthSystemCM;
using UnityEngine;
using Assets.Scripts;

public class CombatHandler : MonoBehaviour
{
    // Low Level Commands for combat related functionalities
    // Every Enemy, Player, maybe Tower, and anything interact with Central Battle Controller have this component
    [Header("Battle Parameters")] 
    [SerializeField] private float hp;
    [SerializeField] private float skillCoolDown;
    [SerializeField] private List<Collider> AttackingColliders;
    [SerializeField] private List<Collider> DamageTakingColliders;
    [SerializeField] private CombatHandler AttackTarget;
    [SerializeField] private CombatHandler DamageSource;
    [SerializeField] private HealthSystemComponent healthSystemComponent;


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

    public void DoDamage(CombatHandler targetHitTarget, float damageAmount)
    {
        CentralBattleCtrl.DamageTransfer(targetHitTarget, damageAmount, this);
    }

    public void ReceiveDamage(float damageAmount, CombatHandler attacker)
    {
        Debug.Log("ReceiveDamage");
        float modifiedDamageAmount;
        PlayerWeaponSkillController playerWeaponSkillController;
        if (TryGetComponent<PlayerWeaponSkillController>(out playerWeaponSkillController))
        {
            modifiedDamageAmount = playerWeaponSkillController.OnrecieveDamage(damageAmount, attacker);
            Debug.Log("Receive no Damage: " + modifiedDamageAmount);
        }
        else {
            modifiedDamageAmount = damageAmount;
        }
        
        var tempHP = hp - modifiedDamageAmount;
        if (hp <= 0)
        {
            hp = 0;
        }
        else hp = tempHP;
        healthSystemComponent.GetHealthSystem().Damage(modifiedDamageAmount);
    }

    public float GetCurrentHP()
    {
        return hp;
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
