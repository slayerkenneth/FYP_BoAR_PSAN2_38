using System;
using System.Collections;
using System.Collections.Generic;
using CodeMonkey.HealthSystemCM;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Active")] public GameObject activeCharacter;
    public GameFlowController GameFlowController;
    [SerializeField] private PlayerStatus PlayerStatusRef;
    
    [Header("Character Prefabs")]
    public GameObject KendoPrefab;
    public GameObject HallSecurityPrefab;
    public GameObject MagicCasterPrefab;
    public GameObject ProfessorPrefab;
    public GameObject RoboticsTeamPrefab;
    public GameObject trackNFieldPrefab;

    public void SpawnPlayer(PlayerStatus globalStatus, Transform parentTransform, Vector3 spawnLocation)
    {
        PlayerStatusRef = PlayerStatus.CurrentPlayer;
        // Type PlayerType = PlayerStatusRef.activeClass.GetType();
        
        // // Hall Guard not work
        // if (PlayerType == typeof(HallGuardActive))
        // {
        activeCharacter = Instantiate(RoboticsTeamPrefab, spawnLocation, new Quaternion(0, 0, 0, 0), parentTransform);    
        // }
        // else if (PlayerType == typeof(ProfessorActive))
        // {
        //     activeCharacter = Instantiate(ProfessorPrefab, spawnLocation, new Quaternion(0, 0, 0, 0), parentTransform);    
        // }
        // else if (PlayerType == typeof(TrackNFieldActive))
        // {
        //     activeCharacter = Instantiate(trackNFieldPrefab, spawnLocation, new Quaternion(0, 0, 0, 0), parentTransform);
        // }
        
        var CharMoveCtrl = activeCharacter.GetComponent<CharacterMovementController>();
        CharMoveCtrl.ARController = GameFlowController.getARCtrl();
        CharMoveCtrl.GameFlowController = GameFlowController;
        
        var charCombat = activeCharacter.GetComponent<CombatHandler>();
        charCombat.SetCentralCombatHandler(GameFlowController.GetCentralBattleController());
        
        var healthBarUI = FindObjectsOfType<HealthBarUI>();
        foreach (var ui in healthBarUI)
        {
            if (ui.tag == "Player")
            {
                // ui.SetHealthSystem(new HealthSystem(PlayerStatusRef.maxHP));
                ui.SetHealthSystem(charCombat.GetHealthSystemComponent().GetHealthSystem());
                charCombat.GetHealthSystemComponent().GetHealthSystem()
                    .SetHealthMax(PlayerStatusRef.maxHPStat[PlayerStatusRef.maxHPLv], true);
                charCombat.GetHealthSystemComponent().GetHealthSystem().SetHealth(PlayerStatusRef.currentHP);
            }
        }
        
        GameFlowController.SetPlayerMovementCtrl(CharMoveCtrl);
    }

    public void DespawnPlayer(int currentHP, int totalNewMoney, int weaponLv)
    {
        PlayerStatus.CurrentPlayer.currentHP = currentHP;
        PlayerStatus.CurrentPlayer.money = totalNewMoney;
        PlayerStatus.CurrentPlayer.weaponLv = weaponLv;
        
        Destroy(activeCharacter);
        activeCharacter = null;
    }
}
