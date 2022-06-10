using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Refs Settings")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform gameMainCamera;
    [SerializeField] CinemachineFreeLook freeLook;

    [Header("Constants")]
    [SerializeField] float mass = 4;
    [SerializeField] float speed = 6;
    [SerializeField] float gravity = -9.82f;
    [SerializeField] float inputThreshold = 0.1f;
    [SerializeField] float jumpHeight = 3;
    Vector3 _velocity;
    Vector3 _moveDirection;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.4f;
    [SerializeField] LayerMask groundMask;
    bool _isGrounded;

    [Header("Smoothness")]
    [SerializeField] float turnSmoothTime = 0.4f;
    [SerializeField] float recenterThreshold = 0.75f;
    float _turnSmoothVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //jump
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = gravity; //gravity push us down when we stand on ground

        if (Input.GetButtonDown("Jump") && _isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity); //-2 is mathematical coefficient

        if (Input.GetMouseButtonDown(0))
            Player.Instance.Shoot();

        //gravity
        _velocity.y += gravity * mass * Time.deltaTime;
        controller.Move(_velocity * Time.deltaTime);

        //walk
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude > inputThreshold)
        {
            float cameraAngle = gameMainCamera.eulerAngles.y;
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraAngle;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity,
                turnSmoothTime);

            //recentring camera
            freeLook.m_RecenterToTargetHeading.m_enabled =
                Mathf.Abs(cameraAngle - transform.eulerAngles.y) > recenterThreshold;

            if (vertical >= Mathf.Sqrt(inputThreshold) && Mathf.Abs(horizontal) < Mathf.Sqrt(inputThreshold))
                transform.rotation = Quaternion.Euler(0f, angle, 0f); //player's rotation follows the camera

            _moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(_moveDirection.normalized * speed * Time.deltaTime);
        }
        else
            freeLook.m_RecenterToTargetHeading.m_enabled = false;
    }

    public Vector3 GetMoveDirection() => _moveDirection;
}
