using CodeMonkey.HealthSystemCM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookController : LandscapeController
{
    public float duration;              //duration that the prompt last
    public GameObject PromptPrefabs;
    public string PromptText;
    public string MaxLvPromptText;

    private GameObject Prompt;
    private bool onEffect;
    private float TimeEclipse;
    private GameObject Player;


    protected override void OnTriggerEnter(Collider other)
    {
        if (Active && other.CompareTag("Player"))
        {
            Active = false;
            remainCD = CD;
            ActiveSign.Stop();

            Player = other.gameObject;
            TimeEclipse = 0;
            onEffect = true;

            var option = new Dictionary<string, System.Delegate>();

            if (PlayerStatus.CurrentPlayer.weaponStat.lv < 9) option.Add("Weapon", new Action(() => { 
                PlayerStatus.CurrentPlayer.weaponStat.lv++;
                PlayerStatus.CurrentPlayer.weaponStat.UpdateStat(Player);
                })) ;

            Debug.Log("Delegate: " + PlayerStatus.CurrentPlayer.weaponStat.lv);

            if (PlayerStatus.CurrentPlayer.maxHPLv < 9) option.Add("HP", new Action(() => PlayerStatus.CurrentPlayer.maxHPLv++));
            
            if (PlayerStatus.CurrentPlayer.speedLv < 9) option.Add("Speed", new Action(() => PlayerStatus.CurrentPlayer.speedLv++));
            
            if (PlayerStatus.CurrentPlayer.passiveClass.lv < 9) option.Add("Passive class", new Action(() => PlayerStatus.CurrentPlayer.passiveClass.lv++));
            
            if (PlayerStatus.CurrentPlayer.activeClass.lv < 9) option.Add("Active class", new Action(() => PlayerStatus.CurrentPlayer.activeClass.lv++));

            if (option.Count != 0)
            {
                var pair = option.ElementAt(UnityEngine.Random.Range(0, option.Count));

                pair.Value.DynamicInvoke(null);

                Prompt.SetActive(true);

                Prompt.GetComponent<TextMeshProUGUI>().text = pair.Key + PromptText;
            }
            else {
                Prompt.SetActive(true);

                Prompt.GetComponent<TextMeshProUGUI>().text = MaxLvPromptText;
            }

            

        }
    }

    protected override void OnTriggerExit(Collider other)
    {
    }


    // Start is called before the first frame update
    void Start()
    {
        var canvas = GameObject.Find("UI Canvas");
        Prompt = Instantiate(PromptPrefabs);
        Prompt.transform.SetParent(canvas.transform);
        Prompt.transform.localPosition = new Vector3(0, 220, 0);
        Prompt.SetActive(false);
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
            if (TimeEclipse > duration)
            {
                onEffect = false;
                TimeEclipse = 0;
                Prompt.SetActive(false);
            }
        }
    }
}
