using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "TrackNFieldActive", menuName = "ActiveClass/TrackNField")]
public class TrackNFieldActive : ActiveClass
{

    [SerializeField] private List<float> TPdistance;


    public override void skill(GameObject player)
    {
        var ARCtrl = GameObject.FindObjectsOfType<ARController>()[0];
        var gameboard = ARCtrl.GetActiveGameboard();
        var trans = player.transform;

        Debug.Log("position: null");
        if (gameboard == null) return;
        
        Vector3 pos;
        Debug.Log("original position: " + trans.position);
        while (gameboard.FindRandomPosition(out pos))
        {
            if (gameboard.CheckFit(pos, 0.01f) && (pos - trans.position).magnitude < TPdistance[lv])
            {
                //trans.position = Vector3.zero;
                player.GetComponent<CharacterController>().Move(pos - trans.position);
                break;
            }
            else continue;
        }
        Debug.Log("position: " + trans.position);

    }
}
