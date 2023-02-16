using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveClass : ScriptableObject
{
    public float skillCD;
    public abstract void skill(GameObject player);
}

public abstract class PassiveClass : ScriptableObject
{
    public abstract void StartPassive(GameObject player);

    public abstract void EndPassive(GameObject player);
}

public class PlayerStatus : ScriptableObject
{
    public int currentHP, maxHP, money, speed, normalAttackDamage, specialAttackDamage, currentLevel;
    public ActiveClass activeClass;
    public PassiveClass passiveClass;

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
                _currentPlayer.normalAttackDamage = 20;
                _currentPlayer.specialAttackDamage = 200;
                _currentPlayer.currentLevel = 0;
                _currentPlayer.activeClass = null;
                _currentPlayer.passiveClass = null;
                _currentPlayer.hideFlags = HideFlags.HideAndDontSave;
            }
            return _currentPlayer;
        }
    }

   
}
