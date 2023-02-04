using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Combat related")] 
    public CombatHandler CombatHandler;
    public List<Collider> AttackingColliders;
    public List<Collider> DamageTakingColliders;

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
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!AttackingColliders.Contains(other) && !DamageTakingColliders.Contains(other))
        {
            if (other.transform.CompareTag("Player"))
            {
                CombatHandler.DoDamage(other.transform.GetComponent<CombatHandler>(), 10f);
            }
        }
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
