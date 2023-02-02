using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSkillController : MonoBehaviour
{

    [Header("Reference")] 
    [SerializeField] private Animator Animator;
    [SerializeField] private CombatHandler combatHandler;
    [SerializeField] private GameFlowController GameFlowCtrl;

    [Header("Weapons")] 
    [SerializeField] private GameObject Weapon;

    [Header("Damage Parameters")]
    [SerializeField] private float NormalAttackDamage;
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

    public void OverrideDefaultWeapon(GameObject newWeapon)
    {
        Weapon = newWeapon;
    }
    
    public void OverrideDefaultSkill()
    {
        
    }

    public void NormalAttack()
    {
        if (!ValidHit) return;
        if (normalAttackCount == 2)
        {
            normalAttackCount = 0;
        }
        else
        {
            normalAttackCount++;
        }
        
        combatHandler.DoDamage(currentAttackingTarget, NormalAttackDamage);
    }

    public void Defense()
    {
        
    }

    public void CastSkill()
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
    }
}
