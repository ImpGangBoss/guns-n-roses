using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Refs Settings")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform gameMainCamera;

    [Header("Constants")]
    [SerializeField] float mass = 4;
    [SerializeField] float speed = 6;
    [SerializeField] float gravity = -9.82f;
    [SerializeField] float inputThreshold = 0.1f;
    [SerializeField] float jumpHeight = 3;
    Vector3 velocity;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.4f;
    [SerializeField] LayerMask groundMask;
    bool isGrounded;

    [Header("Smoothness")]
    [SerializeField] float turnSmoothTime = 0.4f;
    //[SerializeField] float playerRotationThreshold = 0.05f;
    // [Range(0, 2)]
    // [SerializeField] float sensitivityX = 0.8f;
    // [Range(0, 2)]
    // [SerializeField] float sensitivityY = 1f;
    //Vector3 sensitivity;
    float turnSmoothVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //sensitivity = new Vector3(sensitivityX, 0f, sensitivityY);
    }

    void Update()
    {
        //jump
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0f)
            velocity.y = 0f;

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);

        if (Input.GetMouseButtonDown(0))
            Player.Instance.Shoot();

        //gravity
        velocity.y += gravity * mass * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //walk
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction =  new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude > inputThreshold)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + gameMainCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            if (vertical >= inputThreshold || Mathf.Abs(horizontal) >= inputThreshold)
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
    }
}
