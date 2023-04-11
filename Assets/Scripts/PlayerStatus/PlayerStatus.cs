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

public class PlayerStatus : ScriptableObject
{
    public int currentHP, maxHP, money, speed, weaponLv, currentLevel;
    public ActiveClass activeClass;
    public PassiveClass passiveClass;
    public WeaponStat weaponStat;

    private static PlayerStatus _currentPlayer;

    public static PlayerStatus CurrentPlayer
    {
        get {
            if (!_currentPlayer) {
                _currentPlayer = ScriptableObject.CreateInstance<PlayerStatus>();
                _currentPlayer.currentHP = 100;
                _currentPlayer.maxHP = 100;
                _currentPlayer.money = 0;
                _currentPlayer.speed = 1;
                _currentPlayer.weaponLv = 0;
                _currentPlayer.currentLevel = 0;
                // _currentPlayer.activeClass = (HallGuardActive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/HallGuardActive.asset", typeof(HallGuardActive));
                _currentPlayer.activeClass = (ProfessorActive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/Professor/ProfessorActive.asset", typeof(ProfessorActive));
                _currentPlayer.passiveClass = (ProfessorPassive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/Professor/ProfessorPassive.asset", typeof(ProfessorPassive));
                // _currentPlayer.weaponStat = (BackpackStat)AssetDatabase.LoadAssetAtPath("Assets/Scripts/weapon_character/Backpack/BackpackStat.asset", typeof(BackpackStat));
                _currentPlayer.weaponStat = (BookStat)AssetDatabase.LoadAssetAtPath("Assets/Scripts/weapon_character/Book/BookStat.asset", typeof(BookStat));
                _currentPlayer.hideFlags = HideFlags.HideAndDontSave;
            }
            return _currentPlayer;
        }
    }

   
}
