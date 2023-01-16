using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatHandler : MonoBehaviour
{
    // Every Enemy, Player, maybe Tower, and anything interact with Central Battle Controller have this component
    [Header("Battle Parameters")] 
    [SerializeField] public float hp;
    [SerializeField] public float skillCoolDown;
    [SerializeField] public List<Collider> AttackColliders;

    [Header("Reference")] 
    public ARController ARCtrl;
    public GameFlowController GameFlowCtrl;
    public CentralBattleController CentralBattleCtrl;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoDamage(CombatHandler targetHitTarget)
    {
        
    }

    public void ReceiveDamage()
    {
        
    }
}
