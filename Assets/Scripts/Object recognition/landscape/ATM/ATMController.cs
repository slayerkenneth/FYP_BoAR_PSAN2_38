using CodeMonkey.HealthSystemCM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ATMController : LandscapeController
{
    public int moneyGainPreDead;
    public float duration;
    public GameObject FeverTimePrefabs;

    private GameObject FeverTime;
    private bool onEffect;
    private float TimeEclipse;


    protected override void OnTriggerEnter(Collider other)
    {
        if (Active && other.CompareTag("Player"))
        {
            Active = false;
            remainCD = CD;
            ActiveSign.Stop();
            OnCollectStart();

            var enemyList = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemyList)
            {
                var healthSystem = enemy.GetComponent<HealthSystemComponent>().GetHealthSystem();
                healthSystem.OnDead += OnEnemyDead;
            }
            if (!EnemySpawner.OnEnemyDead.Contains(OnEnemyDead)) EnemySpawner.OnEnemyDead.Add(OnEnemyDead);
            FeverTime.SetActive(true);

            TimeEclipse = 0;
            onEffect = true;
        }
    }

    private void OnEnemyDead(object sender, EventArgs e)
    {
        PlayerStatus.CurrentPlayer.money += moneyGainPreDead;
        Debug.Log("Money: " + PlayerStatus.CurrentPlayer.money);
    }

    protected override void OnTriggerExit(Collider other)
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        var canvas = GameObject.Find("UI Canvas");
        FeverTime = Instantiate( FeverTimePrefabs);
        FeverTime.transform.SetParent(canvas.transform);
        FeverTime.transform.localPosition = new Vector3(0, 166, 0);
        FeverTime.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active)
        {
            remainCD -= Time.deltaTime;
            if (remainCD < 0)
            {
                Active = true;
                remainCD = 0;
                ActiveSign.Play();
            }
        }
        if (onEffect)
        {
            TimeEclipse += Time.deltaTime;
            FeverTime.transform.Find("Bar").GetComponent<Image>().fillAmount = 1.0F - TimeEclipse/duration;
            if (TimeEclipse > duration)
            {
                onEffect = false;
                TimeEclipse = 0;

                var enemyList = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (GameObject enemy in enemyList)
                {
                    var healthSystem = enemy.GetComponent<HealthSystemComponent>().GetHealthSystem();
                    healthSystem.OnDead -= OnEnemyDead;
                }
                if (EnemySpawner.OnEnemyDead.Contains(OnEnemyDead)) EnemySpawner.OnEnemyDead.Remove(OnEnemyDead);
                FeverTime.SetActive(false);
            }
        }
    }
}
