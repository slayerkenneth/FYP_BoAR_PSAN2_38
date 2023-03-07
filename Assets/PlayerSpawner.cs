using System.Collections;
using System.Collections.Generic;
using CodeMonkey.HealthSystemCM;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Active")] public GameObject activeCharacter;
    public GameFlowController GameFlowController;
    
    [Header("Character Prefabs")]
    public GameObject KendoPrefab;
    public GameObject HallSecurityPrefab;
    public GameObject MagicCasterPrefab;
    public GameObject ProfessorPrefab;
    public GameObject RoboticsTeamPrefab;
    public GameObject trackNFieldPrefab;

    public void SpawnPlayer(PlayerStatus globalStatus, Transform parentTransform, Vector3 spawnLocation)
    {
        activeCharacter = Instantiate(KendoPrefab, spawnLocation, new Quaternion(0, 0, 0, 0), parentTransform);
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
                ui.SetHealthSystem(charCombat.GetHealthSystemComponent().GetHealthSystem());
            }
        }
        
        GameFlowController.SetPlayerMovementCtrl(CharMoveCtrl);
    }

    public void DestroyPlayer()
    {
        
    }
}
