using System;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {
        
        [Serializable]
        public class JumpState : GroundedState {
            
            [SerializeField] private float _FadeSpeed = 0.25f;
            [SerializeField] private AnimationClip _JumpClip;
            [SerializeField] private int _JumpCount;
            [SerializeField] private float _ForwardSpeedScale = 0f;
            [SerializeField] private float _MaxJumpTime;
            [SerializeField] private float _MaxJumpHeight;
            
            /************************************************************************************************************************/

            private int _JumpsRemaining;
            private float _InitialJumpVelocity;
            
            /************************************************************************************************************************/
            
            public override bool CanEnterState => _JumpsRemaining > 0;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();
                _JumpsRemaining--;
                Instance.Animancer.Play(_JumpClip, _FadeSpeed);

                float timeToApex = _MaxJumpTime / 2f;
                _InitialJumpVelocity = (2 * _MaxJumpHeight) / timeToApex;
                
                Instance._InAirState.Gravity = (-2 * _MaxJumpHeight) / (timeToApex * timeToApex);
            }

            public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
                base.UpdateVelocity(ref currentVelocity, deltaTime);
            
                // Makes the character skip ground probing/snapping on its next update. 
                Motor.ForceUnground();
            
                currentVelocity.y = _InitialJumpVelocity;
                currentVelocity += (Instance.MoveInputVector * _ForwardSpeedScale);

                StateMachine.TrySetState(Instance._InAirState);
            }

            /************************************************************************************************************************/
            
            public void ResetJumps() => _JumpsRemaining = _JumpCount;

        }
        
    }
}