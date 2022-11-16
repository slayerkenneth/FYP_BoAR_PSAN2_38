using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterMovementController : MonoBehaviour
{
    [Header("Character param")] 
    [SerializeField] private Vector3 playerVelocity;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float jumpHeight = 0.4f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private bool groundedPlayer;
    
    [Header("Movement reference")] 
    private CharacterControlMap mapper;
    [SerializeField] private CharacterController controller;
    [SerializeField] private int characterState;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;
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
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 movementInput = mapper.Player.Move.ReadValue<Vector2>();
        Vector3 move = (cameraTransform.forward * movementInput.y + cameraTransform.right * movementInput.x);
        move.y = 0;
        
        controller.Move(move * Time.deltaTime * playerSpeed);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        if (movementInput.magnitude != 0)
        {
            animator.Play("Run");
        }

        // Changes the height position of the player..
        if (mapper.Player.Jump.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            animator.Play("Jump");
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
