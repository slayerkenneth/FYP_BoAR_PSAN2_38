using CodeMonkey.HealthSystemCM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "GoldRicePassive", menuName = "PassiveClass/GoldRice")]
public class GoldRicePassive : PassiveClass
{
    [SerializeField] private List<float> DropRate;
    [SerializeField] private GameObject food;
    [SerializeField] private List<float> HealHp;

    public override void StartPassive(GameObject player)
    {
        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");
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
        foreach (GameObject enemy in enemyList)
        {
            var healthSystem = enemy.GetComponent<HealthSystemComponent>().GetHealthSystem();
            healthSystem.OnDead -= OnEnemyDead;
        }
        if (EnemySpawner.OnEnemyDead.Contains(OnEnemyDead)) EnemySpawner.OnEnemyDead.Remove(OnEnemyDead);
    }

    private void OnEnemyDead(object sender, System.EventArgs e)
    {
        var drop = UnityEngine.Random.Range(0.0F, 1.0F);
        if (DropRate[lv] >= drop) {
            var pos = ((HealthSystem)sender).character.transform.position;
            var healfood = Instantiate(food);
            healfood.transform.position = pos + new Vector3(0,0.07F,0);
            healfood.GetComponent<HealFood>().healAmount = HealHp[lv];
        }

    }
}
