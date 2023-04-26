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
    public Transform USB_Head;

    [Header("VFX")] public ParticleSystem USB_Sparks;

    public float aimTime = 0;
    public bool casting = false;
    private static readonly int Dash = Animator.StringToHash("Dash");

    private void Start()
    {
        foreach (var go in thunder.GetComponentsInChildren<LineRenderer>())
        {
            thunderRenderers.Add(go);
            go.enabled = false;
            thunderActive = false;
        }
        USB_Sparks.Stop();
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
        USB_Sparks.Play();
        thunderActive = true;
        yield return new WaitForSeconds(1.5f);
        thunderActive = false;
        USB_Sparks.Stop();
    }
    

    public void Aim()
    {
        casting = true;
        CableAnimator.Play("CableLonger");
        USB_Sparks.Play();
    }

    public void DashAttack(float damage)
    {
        casting = false;
        CableAnimator.SetTrigger(Dash);
        HoldAttackDamage = damage;
        aimTime = 0;
        USB_Sparks.Stop();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Enemy"))
        {
            var enemyCombat = other.GetComponent<CombatHandler>();
            if (!enemyCombat) return;
            combatHandler.DoDamage(enemyCombat, HoldAttackDamage / 2);
            combatHandler.AddAttackingColliders(other);
            Debug.Log("Extended Cable Hit");
        }
    }
}
