using CodeMonkey.HealthSystemCM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "HallGuardPassive", menuName = "PassiveClass/HallGuard")]
public class HallGuardPassive : PassiveClass
{
    [SerializeField] private List<float> ShieldAmount;
    [SerializeField] private List<float> MaxShield;
    private CombatHandler player;

    public override void StartPassive(GameObject player)
    {
        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        this.player = player.GetComponent<CombatHandler>();
        foreach (GameObject enemy in enemyList)
        {
            var healthSystem = enemy.GetComponent<HealthSystemComponent>().GetHealthSystem();
            healthSystem.OnDead += OnEnemyDead;
        }
        if (!EnemySpawner.OnEnemyDead.Contains(OnEnemyDead)) EnemySpawner.OnEnemyDead.Add(OnEnemyDead);
    }


    public override void EndPassive(GameObject player)
    {
        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        this.player = null;
        foreach (GameObject enemy in enemyList)
        {
            var healthSystem = enemy.GetComponent<HealthSystemComponent>().GetHealthSystem();
            healthSystem.OnDead -= OnEnemyDead;
        }
        if (EnemySpawner.OnEnemyDead.Contains(OnEnemyDead)) EnemySpawner.OnEnemyDead.Remove(OnEnemyDead);
    }

    private void OnEnemyDead(object sender, System.EventArgs e)
    {
        if(player.GetCurrentShield()+ ShieldAmount[lv] < MaxShield[lv])player.ReceiveShield(ShieldAmount[lv]);
        if(player.GetCurrentShield() < MaxShield[lv])player.ReceiveShield(MaxShield[lv] - player.GetCurrentShield());
    }
}
