using UnityEngine;

namespace FirstSteps
{
    public class JumpState : BaseState
    {
        public JumpState(PlayerController player, Animator animator) : base(player, animator) {}

        public override void OnEnter()
        {
            Debug.Log(message:"JumpState.OnEnter");
            animator.CrossFade(JumpHash, crossFadeDuration);
        }

        public override void FixedUpdate()
        {
            player.HandleJump();
            player.HandleMovement();
        }

        public class DashState : BaseState
        {
            public DashState(PlayerController player, Animator animator) : base(player, animator) {}

            public override void OnEnter()
            {
                animator.CrossFade(DashHash, crossFadeDuration);
            }

            public override void FixedUpdate()
            {
                player.HandleMovement();
            }
        }
    }
}