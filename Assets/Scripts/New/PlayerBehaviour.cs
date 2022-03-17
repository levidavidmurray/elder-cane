using System;
using Animancer;
using Animancer.FSM;
using EC.Control;
using KinematicCharacterController;
using UnityEngine;

namespace New {
    public enum OrientationMethodType {
        TowardsCamera,
        TowardsMovement,
    }
    
    public sealed partial class PlayerBehaviour : MonoBehaviour, ICharacterController {
        
        public static PlayerBehaviour Instance { get; private set; }
        
        /************************************************************************************************************************/

        public KinematicCharacterMotor Motor;
        public PlayerInputHandler InputHandler;
        public AnimancerComponent Animancer;
        public Transform CameraTransform;
        public OrientationMethodType OrientationMethod;
        public float OrientationSharpness = 10f;
        public float StableMovementSharpness = 15f;
        
        private readonly StateMachine<LocomotionState> LocomotionStateMachine = new();
        private readonly StateMachine<ActionState> ActionStateMachine = new();
        
        public Vector2 MoveInput { get; private set; }
        public Vector3 MoveInputVector { get; private set; }
        public Vector3 LookInputVector { get; private set; }
        public bool IsGrounded { get; private set; }

        /************************************************************************************************************************/
        private void Awake() {
            AnimancerUtilities.Assert(Instance == null, $"The {nameof(PlayerBehaviour)}.{nameof(Instance)} is already assigned.");
            Instance = this;
            LocomotionStateMachine.ForceSetState(_IdleState);
        }

        private void Start() {
            Motor.CharacterController = this;
        }

        private void Update()
        {
            UpdateBlackboard();
            
            LocomotionStateMachine.CurrentState.Update();
        }
        
        /************************************************************************************************************************/

        private void UpdateBlackboard() {
            MoveInput = InputHandler.MoveInput;
            
            Vector3 cameraPlanarDirection = Vector3.zero;
            MoveInputVector = CalculateMoveInputVector(MoveInput, ref cameraPlanarDirection);
            
            if (OrientationMethod == OrientationMethodType.TowardsCamera) 
                LookInputVector = cameraPlanarDirection;
            else
                LookInputVector = MoveInputVector.normalized;
            
            IsGrounded = Motor.GroundingStatus.IsStableOnGround;
        }
        
        // Calculate input direction relative to camera
        private Vector3 CalculateMoveInputVector(Vector2 moveInput, ref Vector3 cameraPlanarDirection) {
            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(
                new Vector3(moveInput.x, 0f, moveInput.y),
                1f
            );
            
            // Calculate camera direction and rotation on the character plane
            Quaternion cameraRotation = CameraTransform.rotation;
            cameraPlanarDirection =
                Vector3.ProjectOnPlane(cameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            
            if (cameraPlanarDirection.sqrMagnitude == 0f) {
                cameraPlanarDirection = Vector3.ProjectOnPlane(cameraRotation * Vector3.up, Motor.CharacterUp).normalized;
            }
            
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);
            
            return cameraPlanarRotation * moveInputVector;
        }
        
        /************************************ Kinematic Character Controller ***************************************************/

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime) { }
        
        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
            if (LookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f) {
                // Smoothly interpolate from current to target look direction
                Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, LookInputVector,
                    1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                // Set the current rotation (which will be used by the KinematicCharacterMotor)
                currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
            }

            Vector3 currentUp = (currentRotation * Vector3.up);
            // Taken from KCController. Not sure what this extra orientation sharpness is for
            float orientationSharpnessExtra = Mathf.Exp(-OrientationSharpness * deltaTime);
            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - orientationSharpnessExtra);
            
            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            LocomotionStateMachine.CurrentState.UpdateVelocity(ref currentVelocity, deltaTime);
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime) { }

        // Handle landing and leaving ground
        public void PostGroundingUpdate(float deltaTime) {
            // Landed on ground
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround) {
                IsGrounded = true;
            }
            // Left stable ground
            else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround) {
                IsGrounded = false;
            }
        }

        public bool IsColliderValidForCollisions(Collider coll) {
            // TODO: Check Camera collisions
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, 
            ref HitStabilityReport hitStabilityReport) { }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport) { }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

        public void OnDiscreteCollisionDetected(Collider hitCollider) { }
        
        /************************************************************************************************************************/
    }
}