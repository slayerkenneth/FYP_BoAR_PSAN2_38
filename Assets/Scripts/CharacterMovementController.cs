using System;
using System.Collections;
using System.Collections.Generic;
using CodeMonkey.HealthSystemCM;
using Niantic.ARDK.Extensions.Gameboard;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterMovementController : MonoBehaviour
{
    [Header("Character param")]
    [SerializeField] private Vector3 playerVelocity;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float rollingSpeed = 4.0f;
    [SerializeField] private Vector3 rollingDir;
    [SerializeField] private float jumpHeight = 0.4f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private bool groundedPlayer;

    [Header("Movement reference")]
    private CharacterControlMap mapper;
    [SerializeField] private CharacterController controller;
    [SerializeField] private int characterState;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 nearestBoardPosition;

    [Header("Gameboard environment reference")]
    [SerializeField]
    public ARController ARController;
    public IGameboard _activeGameboard;
    private SpatialTree SpatialTree;

    [SerializeField] public GameFlowController GameFlowController;

    [Header("Battle / Combat related reference and variables")]
    [SerializeField] private CombatHandler playerOwnCombatHandler;
    [SerializeField] private HealthSystemComponent playerOwnHealthSystem;
    [SerializeField] private float maxHP;
    [SerializeField] private PlayerWeaponSkillController PlayerWeaponSkillCtrl;

    [Header("Debug")]
    public Text debugLog;
    private void Awake()
    {
        mapper = new CharacterControlMap();
    }

    private void OnEnable()
    {
        mapper.Enable();
    }

    private void OnDisable()
    {
        mapper.Disable();
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        _activeGameboard = ARController.GetActiveGameboard();
        debugLog = ARController.AreaText;
        maxHP = playerOwnHealthSystem.GetHealthSystem().GetHealthMax();
        playerOwnCombatHandler.InitHP(maxHP);
        PlayerWeaponSkillCtrl = FindObjectOfType<PlayerWeaponSkillController>();
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 movementInput = mapper.Player.Move.ReadValue<Vector2>();
        if (Mathf.Abs(movementInput.y) < 0.06) movementInput.y = 0.0F;
        if (Mathf.Abs( movementInput.x) < 0.06) movementInput.x = 0.0F;

         Vector3 move = (cameraTransform.forward * movementInput.y + cameraTransform.right * movementInput.x);
        move.y = 0;

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("rolling")) 
            controller.Move(rollingDir * Time.deltaTime * rollingSpeed); 
        else
            controller.Move(move.normalized * Time.deltaTime * playerSpeed);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move.normalized;
        }

        if (movementInput.magnitude != 0)
        {
            //Debug.Log("Move: " + movementInput);
            if(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) animator.Play("Run");
            _activeGameboard.FindNearestFreePosition(transform.position, out nearestBoardPosition);
            debugLog.text = debugLog.text + " Player nearest board pos " + nearestBoardPosition.ToString();
        }

        //// Changes the height position of the player.. (Button for Testing)
        //if (mapper.Player.Jump.triggered && groundedPlayer)
        //{
        //    playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        //    animator.Play("Jump");
        //}

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        if (mapper.Player.NormalAttack.triggered)
        {
            NormalAttack();
        }
        else if (mapper.Player.Defense.triggered)
        {
            rollingDir = gameObject.transform.forward.normalized;
            Rolling();
        }
        else if (mapper.Player.CastSkill.triggered)
        {
            CastSkill();
        }
        else if (mapper.Player.HoldAttack.triggered)
        {
            StartHoldAttack();
        }
        else if (mapper.Player.HoldAttack.WasReleasedThisFrame())
        {
            EndHoldAttack();
        }
    }
    public Vector3 GetPlayerPosition()
    {
        return nearestBoardPosition;
    }

    public CombatHandler GetPlayerCombatHandler()
    {
        return playerOwnCombatHandler;
    }

    public void NormalAttack()
    {
        PlayerWeaponSkillCtrl.NormalAttack();
    }

    public void Rolling()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))animator.Play("rolling");
        PlayerWeaponSkillCtrl.Rolling();
    }

    public void CastSkill()
    {
        PlayerWeaponSkillCtrl.CastSkill();
        //transform.position = new Vector3(0, 1, 0);
    }
    public void StartHoldAttack()
    {
        PlayerWeaponSkillCtrl.StartHoldAttack();
    }
    public void EndHoldAttack()
    {
        PlayerWeaponSkillCtrl.EndHoldAttack();
    }

    public Vector3 getPlayerPosition()
    {
        return transform.localPosition;
    }

    public void increaseSpeed(float speedIncrease, float time) { 
        //???
        }
    
    public Transform getCharacterTransform()
    {
        return transform;
    }
}
