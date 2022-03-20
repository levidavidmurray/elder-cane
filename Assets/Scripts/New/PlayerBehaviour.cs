using System;
using Animancer;
using Animancer.FSM;
using Cinemachine;
using EC.Control;
using KinematicCharacterController;
using UnityEngine;

namespace New {
    public enum OrientationMethodType {
        TowardsCamera,
        TowardsMovement,
    }

    public sealed partial class PlayerBehaviour : MonoBehaviour, ICharacterController, ICharacterBehaviour {
        
        public static PlayerBehaviour Instance { get; private set; }
        
        /************************************************************************************************************************/

        public KinematicCharacterMotor Motor;
        public PlayerInputHandler InputHandler;
        public AnimancerComponent Animancer;
        public OrientationMethodType OrientationMethod;
        public float OrientationSharpness = 10f;
        public float StableMovementSharpness = 15f;
        // Camera
        public Transform CameraTransform;
        public CinemachineFreeLook FreeLookCamera;
        public CinemachineVirtualCamera LockedCamera;
        public CinemachineTargetGroup TargetLockGroup;

        // Debugging
        public Transform TEMP_testTarget;
        
        /************************************************************************************************************************/
        
        #region Blackboard
        
        public Vector2 MoveInput { get; private set; }
        public Vector3 MoveInputVector { get; private set; }
        public Vector3 LookInputVector { get; private set; }
        public Vector3 VelocityLastTick { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsRolling { get; private set; }
        
        #endregion
        
        /************************************************************************************************************************/
        
        #region Finite State Machine
        
        private readonly StateMachine<LocomotionState> LocomotionStateMachine = new();
        
        [Foldout("Locomotion States", styled = true)]
        [SerializeField] private IdleState _IdleState;
        [SerializeField] private MoveState _MoveState;
        [SerializeField] private JumpState _JumpState;
        [SerializeField] private InAirState _InAirState;
        [SerializeField] private LandState _LandState;
        [SerializeField] private RollState _RollState;
        
        // [Foldout("Action States", styled = true)]
        private readonly StateMachine<ActionState> ActionStateMachine = new();
        
        #endregion
        
        /************************************************************************************************************************/
        
        private Vector3 _SpawnPosition;
        private bool _FreezeVelocityThisTick;
        private Transform _LockedTarget;
        private CameraViewTargetSelector _CameraViewTargetSelector;

        /************************************************************************************************************************/

        public bool IsMoving => MoveInput.magnitude > 0;
        
        /************************************************************************************************************************/
        
        #region Unity Callbacks
        
        private void Awake() {
            AnimancerUtilities.Assert(Instance == null, $"The {nameof(PlayerBehaviour)}.{nameof(Instance)} is already assigned.");
            Instance = this;
            _SpawnPosition = transform.position;
            LocomotionStateMachine.ForceSetState(_IdleState);
            TargetLockGroup.AddMember(transform, 1f, 2f);
            _CameraViewTargetSelector = GetComponent<CameraViewTargetSelector>();
            
            InitializeInputCallbacks();
        }

        private void Start() {
            Motor.CharacterController = this;
        }

        private void Update()
        {
            UpdateBlackboard();
            LocomotionStateMachine.CurrentState.Update();
        }

        private void FixedUpdate() {
            LocomotionStateMachine.CurrentState.FixedUpdate();
        }

        #endregion
        
        /************************************************************************************************************************/

        #region Kinematic Character Controller
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

            if (currentVelocity.magnitude > 0) {
                print($"State: {LocomotionStateMachine.CurrentState.GetType().Name}, Velocity: {currentVelocity}");
            }

            if (_FreezeVelocityThisTick) {
                currentVelocity = Vector3.zero;
                _FreezeVelocityThisTick = false;
            }
            
            VelocityLastTick = currentVelocity;
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
        
        #endregion
        
        /************************************************************************************************************************/

        #region State Callbacks
        
        public void OnEnterGroundedState(GroundedState state) {
            if (state == _JumpState) return;
            InputHandler.UseJumpInput();
        }
        
        public void OnEnterRollState(GroundedState state) {
            InputHandler.UseRollInput();
        }
        
        #endregion
        
        /************************************************************************************************************************/
        
        #region Helper Functions

        private void InitializeInputCallbacks() {

            InputHandler.OnLockTargetCb += _ => {
                if (!_LockedTarget) {
                    LockTarget(_CameraViewTargetSelector.GetTargetToLock());
                }
                else {
                    UnlockTarget(_LockedTarget);
                }
            };
            
            // Debugging (Reset position)
            InputHandler.OnResetCb += _ => {
                Motor.SetPosition(_SpawnPosition);
                _FreezeVelocityThisTick = true;
                LocomotionStateMachine.ForceSetState(_IdleState);
            };
        }

        private void LockTarget(Transform lockTarget) {
            if (!lockTarget) return;
            
            _LockedTarget = lockTarget;
            TargetLockGroup.AddMember(lockTarget, 1f, 2f);
            LockedCamera.Priority = 11;
            OrientationMethod = OrientationMethodType.TowardsCamera;
        }

        private void UnlockTarget(Transform lockTarget) {
            _LockedTarget = null;
            TargetLockGroup.RemoveMember(lockTarget);
            LockedCamera.Priority = 9;
            OrientationMethod = OrientationMethodType.TowardsMovement;
        }
        
        private void UpdateBlackboard() {
            MoveInput = InputHandler.MoveInput;
            IsJumping = InputHandler.JumpInput;
            IsSprinting = InputHandler.SprintInput;
            IsRolling = InputHandler.RollInput;
            
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
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(moveInput.x, 0f, moveInput.y), 1f);
            
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
        
        #endregion
        
        /************************************************************************************************************************/
        
    }
}