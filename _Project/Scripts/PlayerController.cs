using System;
using System.Collections.Generic;
using Assets;
using Cinemachine;
using KBCore.Refs;
using UnityEngine;

namespace FirstSteps
{
    public class PlayerController : ValidatedMonoBehaviour
    {
        
        [Header("References")] 
        [SerializeField, Self]  Rigidbody rb;
        [SerializeField, Self] GroundChecker groundChecker;
        [SerializeField, Self] Animator animator;
        [SerializeField, Anywhere] CinemachineFreeLook freeLookVCam;
        [SerializeField, Anywhere] InputReader input;
        
        [Header("Movement Settings")]
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] float smoothTime = 0.2f;
        
        [Header("Jump Settings")]
        [SerializeField] float jumpForce = 10f;
        [SerializeField] float jumpDuration = 0.5f;
        [SerializeField] float jumpCooldown = 0f;
        [SerializeField] float jumpMaxHeight = 2f;
        [SerializeField] float gravityMultiplier = 3f;
        
        const float Zerof = 0f;

        Transform mainCam;
        
        float currentSpeed;
        float velocity;
        float jumpVelocity;

        Vector3 movement;

        List<Timer> timers;
        CountdownTimer jumpTimer;
        CountdownTimer jumpCooldownTimer;

        static readonly int Speed = Animator.StringToHash(name:"Speed");
        
        void Awake()
        {
            mainCam = Camera.main.transform;
            freeLookVCam.Follow = transform;
            freeLookVCam.LookAt = transform;
            freeLookVCam.OnTargetObjectWarped(transform, transform.position - freeLookVCam.transform.position - Vector3.forward);
            
            rb.freezeRotation = true;
            
            jumpTimer = new CountdownTimer(jumpDuration);
            jumpCooldownTimer = new CountdownTimer(jumpCooldown);
            timers = new List<Timer>(2) {jumpTimer, jumpCooldownTimer};
            
            jumpTimer.OnTimerStop += () =>  jumpCooldownTimer.Start();
        }

        void Start() => input.EnablePlayerActions();

        void OnEnable()
        {
            input.Jump += OnJump;
        }

        void OnDisable()
        {
            input.Jump += OnJump;
        }

        void OnJump(bool performed)
        {
            if (performed && !jumpTimer.IsRunning && !jumpCooldownTimer.IsRunning && groundChecker.IsGrounded)
            {
                jumpTimer.Start();
            } 
            else if (!performed && jumpCooldownTimer.IsRunning)
            {
                jumpTimer.Stop();
            }
        }

        void Update()
        {
            movement = new Vector3(input.Direction.x, y: 0f, z: input.Direction.y);
            
            HandleTimers();
            UpdateAnimator();
        }
        void FixedUpdate()
        {
            HandleJump();
            HandleMovement(); 
        }
        void HandleTimers()
        {
            foreach (var timer in timers)
            {
                timer.Tick(Time.deltaTime);
            }
        }
        
        void HandleJump()
        {
            if(!jumpTimer.IsRunning && groundChecker.IsGrounded)
            {
                jumpVelocity = Zerof;
                jumpTimer.Stop();
                return;
            }

            if(jumpTimer.IsRunning)
            {
                float launchPoint = 0.9f;
                if(jumpTimer.Progress > launchPoint)
                {
                    jumpVelocity = Mathf.Sqrt(f:2*jumpMaxHeight * Mathf.Abs(Physics.gravity.y));
                }
                else
                {
                    jumpVelocity += (1 - jumpTimer.Progress) * jumpForce * Time.fixedDeltaTime;
                }
            }
            else
            {
                jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            }
            
            rb.velocity = new Vector3(rb.velocity.x, y:jumpVelocity, rb.velocity.z);
        }

        void UpdateAnimator()
        {
            animator.SetFloat(id:Speed, currentSpeed);
        }


        void HandleMovement()
        {
            var adjustedDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;
            
            if (adjustedDirection.magnitude > Zerof)
            {
                HandleRotation(adjustedDirection);
                HandleHorizontalMovement(adjustedDirection);
                SmoothSpeed(adjustedDirection.magnitude);
            }
            else
            {
                SmoothSpeed(Zerof);

                rb.velocity = new Vector3(x: Zerof, rb.velocity.y, z: Zerof);
            }
            
        }

        void HandleHorizontalMovement(Vector3 adjustedDirection)
        {
            Vector3 velocity = adjustedDirection * moveSpeed * Time.fixedDeltaTime;
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        }

        void HandleRotation(Vector3 adjustedDirection)
        {
            var targetRotation = Quaternion.LookRotation(adjustedDirection);
            transform.rotation = Quaternion.RotateTowards(from:transform.rotation, to:targetRotation, rotationSpeed * Time.deltaTime);
            transform.LookAt(worldPosition:transform.position + adjustedDirection);
        }

        void SmoothSpeed(float value)
        {
            currentSpeed = Mathf.SmoothDamp(current:currentSpeed, target:value, ref velocity, smoothTime);
        }
    }
}