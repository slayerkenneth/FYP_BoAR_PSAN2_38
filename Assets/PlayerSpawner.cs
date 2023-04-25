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

    public BattlePlayerIconLoader BattlePlayerIconLoader;

    public void SpawnPlayer(PlayerStatus globalStatus, Transform parentTransform, Vector3 spawnLocation)
    {
        PlayerStatusRef = PlayerStatus.CurrentPlayer;
        // Type PlayerType = PlayerStatusRef.activeClass.GetType();
        switch (PlayerPrefs.GetInt("selectedChar"))
        {
            case 0: 
                activeCharacter = Instantiate(HallSecurityPrefab, spawnLocation, new Quaternion(0, 0, 0, 0), parentTransform);
                break;
                    
            case 1:
                activeCharacter = Instantiate(trackNFieldPrefab, spawnLocation, new Quaternion(0, 0, 0, 0), parentTransform);
                break;
                    
            case 2:
                activeCharacter = Instantiate(ProfessorPrefab, spawnLocation, new Quaternion(0, 0, 0, 0), parentTransform);
                break;
            case 3:
                activeCharacter = Instantiate(KendoPrefab, spawnLocation, new Quaternion(0, 0, 0, 0), parentTransform); 
                break;
                    
            case 4: 
                activeCharacter = Instantiate(RoboticsTeamPrefab, spawnLocation, new Quaternion(0, 0, 0, 0), parentTransform);
                break;
        }
        
        BattlePlayerIconLoader.SetPlayerIcon();
        
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
