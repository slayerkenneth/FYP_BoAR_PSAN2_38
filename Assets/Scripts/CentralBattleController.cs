using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralBattleController : MonoBehaviour
{
    // Central Processor of Battle / Combat related Commands
    
    // Start is called before the first frame update
    public void DamageTransfer(CombatHandler targetHitTarget, float damageAmount)
    {
        if (targetHitTarget.GetCurrentHP() > 0)
        {
            targetHitTarget.ReceiveDamage(damageAmount);
        }
    }
}
