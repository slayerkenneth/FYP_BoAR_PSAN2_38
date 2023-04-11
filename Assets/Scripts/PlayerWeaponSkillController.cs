using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerWeaponSkillController : MonoBehaviour
{

    [Header("Reference")] 
    [SerializeField] protected Animator Animator;
    [SerializeField] protected CombatHandler combatHandler;
    [SerializeField] protected GameFlowController GameFlowCtrl;

    [Header("Weapons")] 
    [SerializeField] protected GameObject Weapon;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OverrideDefaultWeapon(GameObject newWeapon)
    {
        Weapon = newWeapon;
    }
    
    public void OverrideDefaultSkill()
    {
        
    }

    public abstract void NormalAttack();

    public abstract void Rolling();
    // public abstract void NormalAttack();

    // public abstract void StartHoldAttack();

    // public abstract void EndHoldAttack();

    public void Defense()
    {
        
    }

    public abstract void CastSkill();

    public abstract void StartHoldAttack();

    public abstract void EndHoldAttack();

    public abstract float OnrecieveDamage(float damageAmount, CombatHandler attacker);

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
