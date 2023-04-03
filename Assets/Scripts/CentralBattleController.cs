using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralBattleController : MonoBehaviour
{
    [Header("Reference & Other Controller")]
    [SerializeField] private CombatHandler currentEnemyAttackedByPlayer;
    [SerializeField] private GameFlowController GameFlowCtrl;
    
    // Central Processor of Battle / Combat related Commands
    
    // Start is called before the first frame update
    public void DamageTransfer(CombatHandler targetHitTarget, float damageAmount)
    {
        Debug.Log("Dmg transfer");
        if (targetHitTarget.GetCurrentHP() > 0)
        {
            targetHitTarget.ReceiveDamage(damageAmount);
            if (targetHitTarget.CompareTag("Enemy"))
            {
                currentEnemyAttackedByPlayer = targetHitTarget;
                GameFlowCtrl.SetCurrentEnemyBeenAttacked(currentEnemyAttackedByPlayer);
                
            }
        }
    }
}
