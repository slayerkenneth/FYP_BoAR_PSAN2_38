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
            if (isAttackCharacter())
            {
                Debug.Log(gameObject.name + "attacking player.");
                CombatHandler.DoDamage(GameFlowCtrl.getPlayerMovementCtrl().getCharacterTransform().GetComponent<CombatHandler>(), DamageAmount);
            } 
            else if (isAttackTower())
            {
                Debug.Log(gameObject.name + "attacking tower.");
            }
        }

        if (type == AttackType.Range)
        {
            if (isAttackCharacter())
            {
                Instantiate(RangePrefab, RangeSpawnPoint.transform.position, Quaternion.identity);
                RangePrefab.GetComponent<RangeAttack>().CombatHandler = CombatHandler;
                RangePrefab.GetComponent<RangeAttack>().target = GameFlowCtrl.getPlayerMovementCtrl().getCharacterTransform();
                RangePrefab.GetComponent<RangeAttack>().GameFlowCtrl = GameFlowCtrl;
                RangePrefab.GetComponent<RangeAttack>().enemy = this.transform;
            }
            else if (isAttackTower())
            {
                //attack tower
                Debug.Log(gameObject.name + "attacking tower.");
            }
            
        }
        
    
        
    }

    private bool isAttackCharacter()
    {
        return Physics.CheckSphere(attackPoint.position, attackRange, whatIsPlayer);
    }

    private bool isAttackTower()
    {
        return Physics.CheckSphere(attackPoint.position, attackRange, whatIsTower);
    }

}
