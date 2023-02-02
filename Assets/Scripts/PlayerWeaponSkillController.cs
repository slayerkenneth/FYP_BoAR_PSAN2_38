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
}
