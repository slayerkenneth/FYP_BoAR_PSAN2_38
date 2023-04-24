using System;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "ProfessorActive", menuName = "ActiveClass/Professor")]
public class ProfessorActive : ActiveClass
{

    [SerializeField] private GameObject soilderPrefab;
    [SerializeField] private List<int> NoOfSoilderLv;
    [SerializeField] private List<int> SoliderDamageLv;

    [Header ("Data")]
    public GameObject currentPlayer;
    private int NoOfSoilder;
    private GameObject[] soilder;


    public override void skill(GameObject player) {
        
        NoOfSoilder = NoOfSoilderLv[lv];
        soilder = new GameObject[NoOfSoilder];
        currentPlayer = player;
        //spawn the solider and set their damage
        for (int i = 0; i < NoOfSoilder; ++i)
        {   
            float randomIndex = Random.Range(-0.3f, 0.3f);
            Vector3 temp_position = new Vector3(player.transform.localPosition.x + 
                randomIndex, player.transform.localPosition.y, player.transform.localPosition.z + randomIndex);
            GameObject temp_soilder = Instantiate(soilderPrefab, temp_position, Quaternion.identity);
            soilder[i] = temp_soilder;
            soilder[i].GetComponent<ProfessorSoilders>().parent = player;
            soilder[i].GetComponent<ProfessorSoilders>().Damage = SoliderDamageLv[lv];
        }
        
    }
}
