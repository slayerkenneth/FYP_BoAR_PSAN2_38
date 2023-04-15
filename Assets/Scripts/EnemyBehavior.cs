using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Combat related")]
    public GameFlowController GameFlowCtrl;
    // public AttackCharacter AtkCharacter; 
    public CombatHandler CombatHandler;
    // public List<Collider> AttackingColliders;
    // public List<Collider> DamageTakingColliders;

    [Header("Data Related")]
    public float attackRange;
    public float sightRange;
    public float colliderRange;
    public float DamageAmount;
    public float attackTime;
    public Transform attackPoint;
    public enum AttackType {Melee, Range};
    public AttackType type;
    //private bool performRangeAttack = true;

    [Header("Range Related")]
    public GameObject RangePrefab;
    public Transform RangeSpawnPoint;

    [Header("Layer Mask")]
    public LayerMask whatIsPlayer;
    public LayerMask whatIsTower;

    private Vector3 playerPosition;
    private bool isDamagePlayer = false;
    private bool isDamageTower = false;
    private GameObject tempCollider;

    [Header("Damage Parameters")]
    [SerializeField] private bool ValidHit = false;
    [SerializeField] private CombatHandler currentAttackingTarget;

        // This version is just making the enemy do damage to players if their colliders collide
    void Start()
    {
        // AttackingColliders = CombatHandler.GetAttackingColliders();
        // DamageTakingColliders = CombatHandler.GetDamageReceivingColliders();
    }

    // Update is called once per frame
    void Update()
    {
        if (CombatHandler.GetCurrentHP() <= 0)
        {
            EnemySpawner.currentEnemyCount--;
            //GameFlowController.EnemyKillCount++;
            Destroy(gameObject);
        }
    }

    //Animation Event
    public void AttackEnd()
    {
        //tempCollider = AtkCharacter.getAttackTarget();
        //Debug.Log(gameObject.transform.name + " temp Collider: " + tempCollider);
        if (type == AttackType.Melee)
        {
            if (!ValidHit) return;           
            if (currentAttackingTarget.CompareTag("Player"))
            {
                Debug.Log(gameObject.name + "attacking player.");
                CombatHandler.DoDamage(currentAttackingTarget, DamageAmount);
            } 
            else if (currentAttackingTarget.CompareTag("Tower(D)"))
            {
                Debug.Log(gameObject.name + "attacking tower.");
            }
        }

        if (type == AttackType.Range)
        {        
 
            Instantiate(RangePrefab, RangeSpawnPoint.transform.position, Quaternion.identity);
            RangePrefab.GetComponent<RangeAttack>().CombatHandler = CombatHandler;
            RangePrefab.GetComponent<RangeAttack>().GameFlowCtrl = GameFlowCtrl;
            RangePrefab.GetComponent<RangeAttack>().enemy = this.transform;
            RangePrefab.GetComponent<RangeAttack>().target = GameFlowCtrl.getPlayerMovementCtrl().getCharacterTransform();

        }
        
    
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ValidHit = true;
            currentAttackingTarget = other.transform.GetComponent<CombatHandler>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ValidHit = false;
            currentAttackingTarget = null;
        }
        // shd be after animation end and depends on collider of the weapon
    }

    // private bool isAttackCharacter()
    // {
    //     return Physics.CheckSphere(attackPoint.position, attackRange, whatIsPlayer);
    // }

    // private bool isAttackTower()
    // {
    //     return Physics.CheckSphere(attackPoint.position, attackRange, whatIsTower);
    // }

}
