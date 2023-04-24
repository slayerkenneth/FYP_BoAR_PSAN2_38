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

    /*
     * Need to Change based on what character player ios playing with: the weapon, active class
     */
    public static PlayerStatus CurrentPlayer
    {
        get {
            if (!_currentPlayer) {
                PlayerStatus defaultStat = (PlayerStatus)Resources.Load<PlayerStatus>("PlayerStatus/Default");
                _currentPlayer = CreateInstance<PlayerStatus>();
                _currentPlayer.currentHP = 100;
                _currentPlayer.maxHPLv = 0;
                _currentPlayer.money = 0;
                _currentPlayer.speedLv = 1;
                _currentPlayer.weaponLv = 0;
                _currentPlayer.currentLevel = 0;
                _currentPlayer.maxHPStat = defaultStat.maxHPStat;
                _currentPlayer.speedStat = defaultStat.speedStat;

                // switch (PlayerPrefs.GetInt("selectedChar"))
                // {
                //     case 0:
                //         _currentPlayer.activeClass = (HallGuardActive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/HallGuardActive.asset", typeof(HallGuardActive));
                //         _currentPlayer.weaponStat = (CableStat)AssetDatabase.LoadAssetAtPath("Assets/Scripts/weapon_character/Cable/CableStat.asset", typeof(CableStat));
                //         _currentPlayer.passiveClass = (ProfessorPassive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/Professor/ProfessorPassive.asset", typeof(ProfessorPassive));
                //         break;
                //     
                //     case 1:
                //         
                //         break;
                //     
                //     case 2:
                //         _currentPlayer.activeClass = (ProfessorActive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/Professor/ProfessorActive.asset", typeof(ProfessorActive));
                //         _currentPlayer.passiveClass = (ProfessorPassive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/Professor/ProfessorPassive.asset", typeof(ProfessorPassive));
                //         _currentPlayer.weaponStat = (BookStat)AssetDatabase.LoadAssetAtPath("Assets/Scripts/weapon_character/Book/BookStat.asset", typeof(BookStat));
                //
                //         break;
                //     case 3:
                //         break;
                //     
                //     case 4:
                //         _currentPlayer.activeClass = (HallGuardActive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/HallGuardActive.asset", typeof(HallGuardActive));
                //         _currentPlayer.weaponStat = (CableStat)AssetDatabase.LoadAssetAtPath("Assets/Scripts/weapon_character/Cable/CableStat.asset", typeof(CableStat));
                //         _currentPlayer.passiveClass = (ProfessorPassive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/Professor/ProfessorPassive.asset", typeof(ProfessorPassive));
                //         break;
                // }
                _currentPlayer.activeClass = (ProfessorActive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/ProfessorActive.asset", typeof(ProfessorActive));
                // _currentPlayer.activeClass = (ProfessorActive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/Professor/ProfessorActive.asset", typeof(ProfessorActive));
                _currentPlayer.weaponStat = (BookStat)AssetDatabase.LoadAssetAtPath("Assets/Scripts/weapon_character/Book/BookStat.asset", typeof(BookStat));
                // _currentPlayer.weaponStat = (BackpackStat)AssetDatabase.LoadAssetAtPath("Assets/Scripts/weapon_character/Backpack/BackpackStat.asset", typeof(BackpackStat));
                // _currentPlayer.weaponStat = (BookStat)AssetDatabase.LoadAssetAtPath("Assets/Scripts/weapon_character/Book/BookStat.asset", typeof(BookStat));
                _currentPlayer.passiveClass = (ProfessorPassive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/Professor/ProfessorPassive.asset", typeof(ProfessorPassive));
            
                // _currentPlayer.activeClass = (HallGuardActive)Resources.Load("Assets/Scripts/ActiveClass/HallGuardActive.asset", typeof(HallGuardActive));
                // // _currentPlayer.activeClass = (ProfessorActive)AssetDatabase.LoadAssetAtPath("Assets/Scripts/ActiveClass/Professor/ProfessorActive.asset", typeof(ProfessorActive));
                // _currentPlayer.weaponStat = (CableStat)Resources.Load("Assets/Scripts/weapon_character/Cable/CableStat.asset", typeof(CableStat));
                // // _currentPlayer.weaponStat = (BackpackStat)AssetDatabase.LoadAssetAtPath("Assets/Scripts/weapon_character/Backpack/BackpackStat.asset", typeof(BackpackStat));
                // // _currentPlayer.weaponStat = (BookStat)AssetDatabase.LoadAssetAtPath("Assets/Scripts/weapon_character/Book/BookStat.asset", typeof(BookStat));
                // _currentPlayer.passiveClass = (ProfessorPassive)Resources.Load("Assets/Scripts/ActiveClass/Professor/ProfessorPassive.asset", typeof(ProfessorPassive));
                
                _currentPlayer.hideFlags = HideFlags.HideAndDontSave;
            }
            return _currentPlayer;
        }
    }

   
}
