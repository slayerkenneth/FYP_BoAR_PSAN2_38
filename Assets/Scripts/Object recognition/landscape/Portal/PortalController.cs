using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : LandscapeController
{
    public float duration;              //The time that the teleport happen
    [SerializeField]public List<ParticleSystem> teleportEffect;
    public ARController ARCtrl;
    public float range;

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

            foreach (var part in teleportEffect)part.Play();
        
            TimeEclipse = 0;
            onEffect = true;
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (var part in teleportEffect) part.Stop();
        var scale = teleportEffect[0].transform.localScale;
        scale.x = range / 0.05F;
        scale.z = range / 0.05F;
        teleportEffect[0].transform.localScale = scale;
        ARCtrl = GameObject.FindObjectsOfType<ARController>()[0];
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
        if (!onEffect) return;
        TimeEclipse += Time.deltaTime;
        if (TimeEclipse <= duration) return;

        onEffect = false;
        TimeEclipse = 0;

        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");

        var gameboard = ARCtrl.GetActiveGameboard();

        foreach (GameObject enemy in enemyList)
        {
            if ((enemy.transform.position - transform.position).magnitude > range) continue;

            if (gameboard != null) {
                Vector3 pos;

                while (gameboard.FindRandomPosition(out pos)) {
                    if (gameboard.CheckFit(pos, 0.01f)) { 
                        enemy.transform.position = pos;
                        break;
                    }
                    else continue;
                }
            }
        }

        //var playerList = GameObject.FindGameObjectsWithTag("Player");
        //
        //Debug.Log("portal: " + playerList.Length);
        //
        //foreach (GameObject player in playerList)
        //{
        //    Debug.Log("portal: " + player.name);
        //    if ((player.transform.position - transform.position).magnitude > range) continue;
        //    
        //    if (gameboard != null)
        //    {
        //        Vector3 pos;
        //
        //        while (gameboard.FindRandomPosition(out pos))
        //        {
        //            if (gameboard.CheckFit(pos, 0.01f))
        //            {
        //                Debug.Log("portal: 2" + player.name);
        //
        //                Debug.Log("portal: " + pos + " | " + player.transform.position);
        //                player.transform.localPosition = pos;
        //                Debug.Log("portal: " + pos + " | " + player.transform.position);
        //                break;
        //            }
        //            else continue;
        //        }
        //    }
        //}

        foreach (var part in teleportEffect) part.Stop();

    }
}
