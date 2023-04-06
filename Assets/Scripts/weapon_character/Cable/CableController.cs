using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class CableController : MonoBehaviour
{
    public Animator PlayerAnimator;
    public Animator CableAnimator;
    public float HoldAttackDamage;
    public CombatHandler combatHandler;
    public LayerMask enemyLayer;
    public GameObject thunder;
    public List<LineRenderer> thunderRenderers;
    private bool thunderActive;

    public float aimTime = 0;
    public bool casting = false;

    private void Start()
    {
        foreach (var go in thunder.GetComponentsInChildren<LineRenderer>())
        {
            thunderRenderers.Add(go);
            go.enabled = false;
            thunderActive = false;
        }
    }

    private void Update()
    {
        if (casting)
        {
            aimTime += Time.deltaTime;
        }

        thunderRenderers.ForEach(i => i.enabled = thunderActive);
    }

    public void AreaElectricAttack(float damage, float range)
    {
        var EnemyBeingHit = Physics.OverlapSphere(transform.position, range, enemyLayer);
        foreach (var enemy in EnemyBeingHit)
        {
            var enemyCombat = enemy.GetComponent<CombatHandler>();
            if (!enemyCombat) continue;
            combatHandler.DoDamage(enemyCombat, damage);
            combatHandler.AddAttackingColliders(enemy);
        }

        StartCoroutine("AnimateThunder");
    }

    public IEnumerator AnimateThunder()
    {
        thunderActive = true;
        yield return new WaitForSeconds(1.5f);
        thunderActive = false;
    }
    

    public void Aim()
    {
        casting = true;
    }

    public void DashAttack(float damage)
    {
        casting = false;

        if (combatHandler.GetAttackingColliders() != null)
        {
            var targetPos = combatHandler.GetAttackingColliders()[0].transform.position;
            var playerTransform = PlayerAnimator.transform;
            var playerPos = playerTransform.position;
        }
        
        aimTime = 0;
    }
}
