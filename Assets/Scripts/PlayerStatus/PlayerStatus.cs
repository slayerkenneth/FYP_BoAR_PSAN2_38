using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class ActiveClass : ScriptableObject
{
    public int lv;
    public float skillCD;

    public List<string> upgradeHint;

    public abstract void skill(GameObject player);
}

public abstract class PassiveClass : ScriptableObject
{
    public int lv;

    public List<string> upgradeHint;

    public abstract void StartPassive(GameObject player);

    public abstract void EndPassive(GameObject player);
}

public abstract class WeaponStat : ScriptableObject
{
    public int lv;

    public List<string> upgradeHint;

    public abstract void UpdateStat(GameObject player);
}

[CreateAssetMenu(fileName = "PlayerStatus", menuName = "PlayerStatus")]
public class PlayerStatus : ScriptableObject
{
    public int currentHP, maxHPLv, money, speedLv, weaponLv, currentLevel;
    public ActiveClass activeClass;
    public PassiveClass passiveClass;
    public WeaponStat weaponStat;
    public List<int> maxHPStat;
    public List<float> speedStat;

    private static PlayerStatus _currentPlayer;

    public static PlayerStatus CurrentPlayer
    {
        get {
            if (!_currentPlayer) {
                //_currentPlayer = (PlayerStatus)AssetDatabase.LoadAssetAtPath("Assets/Scripts/PlayerStatus/Default.asset", typeof(PlayerStatus));
                _currentPlayer = (PlayerStatus)Resources.Load<PlayerStatus>("PlayerStatus/Default");
                DontDestroyOnLoad(_currentPlayer);
            }
            return _currentPlayer;
        }
    }

   
}
