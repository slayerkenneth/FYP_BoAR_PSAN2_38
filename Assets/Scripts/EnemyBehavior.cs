using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Combat related")]
    public GameFlowController GameFlowCtrl; 
    public CombatHandler CombatHandler;
    public List<Collider> AttackingColliders;
    public List<Collider> DamageTakingColliders;

    [Header("Data Related")]
    public float attackRange;
    public float sightRange;
    public float DamageAmount;

    private Vector3 playerPosition;
    private bool isDamagePlayer = false;
    private bool isDamageTower = false;
    private Collider tempCollider;

        // This version is just making the enemy do damage to players if their colliders collide
    void Start()
    {
        AttackingColliders = CombatHandler.GetAttackingColliders();
        DamageTakingColliders = CombatHandler.GetDamageReceivingColliders();
    }

    // Update is called once per frame
    void Update()
    {
        if (CombatHandler.GetCurrentHP() <= 0)
        {
            EnemySpawner.currentEnemyCount--;
            GameFlowController.EnemyKillCount++;
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!AttackingColliders.Contains(other) && !DamageTakingColliders.Contains(other))
        { 
            if (other.transform.CompareTag("Player"))
            {
                isDamagePlayer = true;
                tempCollider = other;
            }
            else if (other.transform.CompareTag("Tower(D)"))
            {
                isDamageTower = true;
            }
        }
    }

    //Animation Event
    public void AttackEnd()
    {
        // if(GameFlowCtrl.getPlayerMovementCtrl())
        // {
        //     //Debug.Log("Halo");
        //     playerPosition = GameFlowCtrl.getPlayerMovementCtrl().getPlayerPosition();
        //     //if melee atttack
        //     if (isDamagePlayer)
        //     {
        //         Debug.Log(gameObject.name + "attacking player.");
        //         CombatHandler.DoDamage(tempCollider.transform.GetComponent<CombatHandler>(), DamageAmount);
        //         isDamagePlayer = false;
        //     }
        // }
        if (isDamagePlayer)
        {
            Debug.Log(gameObject.name + "attacking player.");
            CombatHandler.DoDamage(tempCollider.transform.GetComponent<CombatHandler>(), DamageAmount);
            isDamagePlayer = false;
        }
        
        if (isDamageTower)
        {
            Debug.Log(gameObject.name + "attacking tower.");
            isDamageTower = false;
        }
    
        //if Long Range Attack
    }

    // private void OnTriggerStay(Collider other)
    // {
    //     if (!AttackingColliders.Contains(other) && !DamageTakingColliders.Contains(other))
    //     {
    //         if (other.transform.CompareTag("Player"))
    //         {
    //             CombatHandler.DoDamage(other.transform.GetComponent<CombatHandler>(), 10f);
    //         }
    //     }
    // }
}
