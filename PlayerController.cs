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
        [SerializeField] float gravityMultiplier = 3f;
        
        [Header("Dash Settings")]
        [SerializeField] float dashForce = 10f;
        [SerializeField] float dashDuration = 1f;
        [SerializeField] float dashCooldown = 2f;

        [Header("Attack Settings")] 
        [SerializeField] float attackCooldown = 0.5f;
        [SerializeField] float attackDistance = 1f;
        [SerializeField] int attackDamage = 10;
        
        const float Zerof = 0f;

        Transform mainCam;
        
        float currentSpeed;
        float velocity;
        float jumpVelocity;
        float dashVelocity = 1f;

        Vector3 movement;

        List<Timer> timers;
        CountdownTimer jumpTimer;
        CountdownTimer jumpCooldownTimer;
        CountdownTimer dashTimer;
        CountdownTimer dashCooldownTimer;
        CountdownTimer attackTimer;

        StateMachine stateMachine;

        static readonly int Speed = Animator.StringToHash(name:"Speed");
        
        void Awake()
        {
            mainCam = Camera.main.transform;
            freeLookVCam.Follow = transform;
            freeLookVCam.LookAt = transform;
            freeLookVCam.OnTargetObjectWarped(transform, transform.position - freeLookVCam.transform.position - Vector3.forward);
            
            rb.freezeRotation = true;
            
            SetupTimers();

            SetupStateMachine();
        }

        void SetupStateMachine()
        {
            stateMachine = new StateMachine();

            var locomotionState = new LocomotionState(this, animator);
            var jumpState = new JumpState(this, animator);
            var dashState = new DashState(this, animator);
            var attackState = new AttackState(this, animator);
            
            At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
            At(locomotionState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
            At(locomotionState, attackState, new FuncPredicate(() => attackTimer.IsRunning));
            At(attackState, locomotionState, new FuncPredicate(() => !attackTimer.IsRunning));
            Any(locomotionState, condition:new FuncPredicate(ReturnToLocomotionState));
            
            stateMachine.SetState(locomotionState);
        }

        bool ReturnToLocomotionState()
        {
            return groundChecker.IsGrounded 
                   && !attackTimer.IsRunning 
                   && !jumpTimer.IsRunning 
                   && !dashTimer.IsRunning;
        }

        void SetupTimers()
        {
            jumpTimer = new CountdownTimer(jumpDuration);
            jumpCooldownTimer = new CountdownTimer(jumpCooldown);
            
            jumpTimer.OnTimerStart += () => jumpVelocity = jumpForce;
            jumpTimer.OnTimerStop += () =>  jumpCooldownTimer.Start();

            dashTimer = new CountdownTimer(dashDuration);
            dashCooldownTimer = new CountdownTimer(dashCooldown);
            
            
            dashTimer.OnTimerStart += () => dashVelocity = dashForce;
            dashTimer.OnTimerStop += () =>
            {
                dashVelocity = 1f;
                dashCooldownTimer.Start();
            };

            attackTimer = new CountdownTimer(attackCooldown);
            
            timers = new List<Timer>(5) {jumpTimer, jumpCooldownTimer, dashTimer, dashCooldownTimer, attackTimer};
        }

        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        void Start() => input.EnablePlayerActions();

        void OnEnable()
        {
            input.Jump += OnJump;
            input.Dash += OnDash;
            input.Attack += OnAttack;
        }

        void OnDisable()
        {
            input.Jump += OnJump;
            input.Dash += OnDash;
            input.Attack += OnAttack;
        }

        void OnAttack()
        {
            if (!attackTimer.IsRunning)
            {
                attackTimer.Start();
            }
        }

        public void Attack()
        {
            Vector3 attackPos = transform.position + transform.forward;
            Collider[] hitEnemies = Physics.OverlapSphere(attackPos, attackDistance);

            foreach (var enemy in hitEnemies)
            {
                Debug.Log(enemy.name);
                if (enemy.CompareTag("Enemy"))
                {
                    enemy.GetComponent<Health>().TakeDamage(attackDamage);
                }
            }
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

        void OnDash(bool performed)
        {
            if(performed && !dashTimer.IsRunning && !dashCooldownTimer.IsRunning)
            {
                dashTimer.Start();
            }
            else if (!performed && dashTimer.IsRunning)
            {
                dashTimer.Stop();
            }
        }

        void Update()
        {
            movement = new Vector3(input.Direction.x, y: 0f, z: input.Direction.y);
            stateMachine.Update();
            
            HandleTimers();
            UpdateAnimator();
        }
        void FixedUpdate()
        {
            stateMachine.FixedUpdate(); 
        }
        void HandleTimers()
        {
            foreach (var timer in timers)
            {
                timer.Tick(Time.deltaTime);
            }
        }
        
         public void HandleJump()
        {
            if(!jumpTimer.IsRunning && groundChecker.IsGrounded)
            {
                jumpVelocity = Zerof;
                jumpTimer.Stop();
                return;
            }

            if(!jumpTimer.IsRunning)
            {
                jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            }
            
            rb.velocity = new Vector3(rb.velocity.x, y:jumpVelocity, rb.velocity.z);
        }

        void UpdateAnimator()
        {
            animator.SetFloat(id:Speed, currentSpeed);
        }


        public void HandleMovement()
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
            Vector3 velocity = adjustedDirection * moveSpeed * dashVelocity * Time.fixedDeltaTime;
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