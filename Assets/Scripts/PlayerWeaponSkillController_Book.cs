using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSkillController_Book : PlayerWeaponSkillController
{

    [Header("Reference")] 
    [SerializeField] private Animator Animator;
    [SerializeField] private CombatHandler combatHandler;
    [SerializeField] private GameFlowController GameFlowCtrl;

    [Header("Weapons")] 
    [SerializeField] private GameObject Weapon;

    [Header("Damage Parameters")]
    [SerializeField] private float NormalAttackDamage;
    [SerializeField] private float HoldAttackDamage;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private float SkillDamage;
    [SerializeField] private int normalAttackCount = 0;
    [SerializeField] private bool ValidHit = false;
    [SerializeField] private CombatHandler currentAttackingTarget;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void NormalAttack()
    {
        if (!ValidHit) return;
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            Animator.Play("NormalAttack");
            combatHandler.DoDamage(currentAttackingTarget, NormalAttackDamage);
        }
        
    }

    public override void StartHoldAttack()
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            Animator.Play("HoldAttack");
        }
    }

    public override void EndHoldAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, 0.2f, whatIsEnemy);
        if (Physics.CheckSphere(transform.position, 0.5f))
        {
            if (hitColliders.Length > 1)
            {
                for (int i = 0; i < hitColliders.Length; ++i)
                { 
                    if (hitColliders[i].gameObject != this.gameObject)
                    {
                        combatHandler.DoDamage(hitColliders[i].gameObject.GetComponent<CombatHandler>(), HoldAttackDamage);
                    }
                } 
            }
        }
    }

     public void OverrideDefaultWeapon(GameObject newWeapon)
    {
        Weapon = newWeapon;
    }
    
    public void OverrideDefaultSkill()
    {
        
    }
    
    public void Defense()
    {
        
    }

    public override void CastSkill()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            ValidHit = true;
            currentAttackingTarget = other.transform.GetComponent<CombatHandler>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ValidHit = false;
        // shd be after animation end and depends on collider of the weapon
    }

    public override void Rolling()
    {
        
    }
    
    public override float OnrecieveDamage(float damageAmount, CombatHandler attacker)
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("rolling")) return 0;
        
        // Animator.Play("ReceiveDamage");
        return damageAmount;
    }}
