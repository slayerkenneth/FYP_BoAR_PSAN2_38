using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "HallGuardActive", menuName = "ActiveClass/HallGuard")]
public class HallGuardActive : ActiveClass
{
    [SerializeField] private List<float> freezeTime;
    [SerializeField] private float freezeDistance = 1;
    [SerializeField] private float freezeAngle = 120;



    public override void skill(GameObject player) {
        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemyList)
        {

            var dir = enemy.transform.position - player.transform.position;
            var angle = Math.Atan2(dir.x, dir.z);
            if (dir.magnitude < freezeDistance && InBetween(player.transform.rotation.eulerAngles.y, freezeAngle, angle))
            {
                var enemyPathfinding = enemy.GetComponent<EnemyPathfinding>();
                enemyPathfinding.setSpeed(0);
                Task.Delay(Convert.ToInt32(freezeTime[lv]*1000)).ContinueWith(t => { enemyPathfinding.resetSpeed(); });
            }
        }
    }

    private bool InBetween(double center, float range, double dir)
    {

        var upperBound1 = center + range / 2 - 360;
        var lowerBound1 = center - range / 2 - 360;
        var InBetween1 = upperBound1 > dir && dir > lowerBound1;

        var upperBound2 = center + range / 2;
        var lowerBound2 = center - range / 2;
        var InBetween2 = upperBound2 > dir && dir > lowerBound2;

        var upperBound3 = center + range / 2;
        var lowerBound3 = center - range / 2;
        var InBetween3 = upperBound3 > dir && dir > lowerBound3;

        return InBetween1 || InBetween2 || InBetween3;
    }
}
