using System;
using System.Collections.Generic;
using Animancer;
using EC.Control;
using EC.Core.SubStates;
using KinematicCharacterController;
using UnityEngine;

namespace EC.Core {

    public enum OrientationMethod {
        TowardsCamera,
        TowardsMovement,
    }

    public struct PlayerInput {
        public Vector2 MoveInput;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool RollDown;
        public bool CrouchDown;
        public bool CrouchUp;
        public Transform LockTarget;
    }

    public struct AIInput {
        public Vector3 MoveVector;
        public Vector3 LookVector;
    }

    public enum BonusOrientationMethod {
        None,
        TowardsGravity,
        TowardsGroundSlopeAndGravity,
    }

    public class KCController : MonoBehaviour, ICharacterController {
        
        # region Anim Properties
        
        public readonly int AnimProp_YVelocity = Animator.StringToHash("yVelocity");
        public readonly int AnimProp_IsGrounded = Animator.StringToHash("IsGrounded");
        public readonly int AnimProp_Roll = Animator.StringToHash("Roll");
        public readonly int AnimProp_Land = Animator.StringToHash("Land");
        public readonly int AnimProp_MoveMagnitude = Animator.StringToHash("MoveMagnitude");
        public readonly int AnimProp_Forward = Animator.StringToHash("Forward");
        public readonly int AnimProp_Sideways = Animator.StringToHash("Sideways");
        public readonly int AnimProp_AttackLight = Animator.StringToHash("AttackLight");
        public readonly int AnimProp_AttackHeavy = Animator.StringToHash("AttackHeavy");
        
        #endregion
        
        public KinematicCharacterMotor Motor;
        
        [SerializeField]
        private AnimancerComponent _Animancer;
        public AnimancerComponent Animancer => _Animancer;

        public List<Collider> IgnoredColliders = new List<Collider>();
        public PlayerInputHandler InputHandler;
        public Animator Anim;
        
        public Vector3 MoveInputVector { get; private set; }

        public Action OnRollComplete;
        public Action OnAttackComplete;
        
        #region Computed Properties
        
        public Vector3 Velocity => Motor.Velocity;
        public bool IsLockedOn => _lockTarget != null;
        
        #endregion
        

        #region States

        public PlayerStateMachine ActionStateMachine { get; private set; }
        public PlayerStateMachine MoveStateMachine { get; private set; }
        
        public PlayerEmptyState EmptyState { get; private set; }
        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerJumpState JumpState { get; private set; }
        public PlayerInAirState InAirState { get; private set; }
        public PlayerLandState LandState { get; private set; }
        public PlayerRollState RollState { get; private set; }
        public PlayerAttackState AttackState { get; private set; }

        [SerializeField] private KCControllerData controllerData;
        #endregion

        private bool _shouldResetVelocity;
        private Transform _lockTarget;
        private Vector3 _lookInputVector;
        private Vector3 _internalVelocityAdd = Vector3.zero;
        

        #region Unity Callbacks
        private void Awake() {
            // Assign the characterController to the motor
            Motor.CharacterController = this;
            
            // States
            ActionStateMachine = new PlayerStateMachine();
            MoveStateMachine = new PlayerStateMachine();
            
            // Move States
            IdleState = new PlayerIdleState(this, MoveStateMachine, controllerData);
            MoveState = new PlayerMoveState(this, MoveStateMachine, controllerData);
            JumpState = new PlayerJumpState(this, MoveStateMachine, controllerData);
            InAirState = new PlayerInAirState(this, MoveStateMachine, controllerData);
            LandState = new PlayerLandState(this, MoveStateMachine, controllerData);
            
            // Action States
            EmptyState = new PlayerEmptyState(this, ActionStateMachine, controllerData);
            RollState = new PlayerRollState(this, ActionStateMachine, controllerData);
            AttackState = new PlayerAttackState(this, ActionStateMachine, controllerData);
        }

        private void Start() {
            ActionStateMachine.Initialize(EmptyState);
            MoveStateMachine.Initialize(IdleState);
        }

        private void Update() {
            ActionStateMachine.CurrentState.LogicUpdate();
            MoveStateMachine.CurrentState.LogicUpdate();
            
            Anim.SetFloat(AnimProp_MoveMagnitude, InputHandler.MoveInput.magnitude);

            float moveInputY = InputHandler.MoveInput.y;
            float moveInputX = InputHandler.MoveInput.x;

            if (!_lockTarget) {
                moveInputY = Mathf.Abs(moveInputY);
                moveInputX = Mathf.Abs(moveInputX);
            }
            
            Anim.SetFloat(AnimProp_Forward, moveInputY);
            Anim.SetFloat(AnimProp_Sideways, moveInputX);
        }

        private void FixedUpdate() {
            ActionStateMachine.CurrentState.PhysicsUpdate();
            MoveStateMachine.CurrentState.PhysicsUpdate();
        }
        
        #endregion

        public void ResetVelocity() {
            _shouldResetVelocity = true;
        }

        /// <summary>
        /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerInput inputs) {
            
            _lockTarget = inputs.LockTarget;

            // if (_lockTarget) {
            //     controllerData.OrientationMethod = OrientationMethod.TowardsCamera;
            // }
            // else {
            //     controllerData.OrientationMethod = OrientationMethod.TowardsMovement;
            // }
            
            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveInput.x, 0f, inputs.MoveInput.y), 1f);

            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection =
                Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f) {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp)
                    .normalized;
            }

            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);
            // Move and look inputs
            MoveInputVector = cameraPlanarRotation * moveInputVector;

            switch (controllerData.OrientationMethod) {
                case OrientationMethod.TowardsCamera:
                    _lookInputVector = cameraPlanarDirection;
                    break;
                case OrientationMethod.TowardsMovement:
                    _lookInputVector = MoveInputVector.normalized;
                    break;
            }

        }

        /// <summary>
        /// This is called every frame by the AI script in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref AIInput inputs) {
            MoveInputVector = inputs.MoveVector;
            _lookInputVector = inputs.LookVector;
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime) {
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {

            // if (_lockTarget) {
            //     var targetDir = (_lockTarget.position - transform.position).normalized;
            //     var lookRotation = Quaternion.LookRotation(targetDir, Motor.CharacterUp);
            //     var currentEuler = currentRotation.eulerAngles;
            //     currentEuler.y = lookRotation.eulerAngles.y;
            //     currentRotation = Quaternion.Euler(currentEuler);
            //     return;
            // }
            
            var pos = transform.position;
            Debug.DrawLine(pos, pos + (Motor.CharacterForward * 2f), Color.red);

            if (_lockTarget) {
                Vector3 dir = _lockTarget.position - transform.position;
                dir.Normalize();
                dir.y = 0;
                //
                // Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, dir,
                //     1 - Mathf.Exp(-controllerData.OrientationSharpness * deltaTime)).normalized;
                //
                // currentRotation = Quaternion.LookRotation(dir);
                // return;
                // _lookInputVector = dir;
                var tr = Quaternion.LookRotation(dir);
                Quaternion targetRotation = Quaternion.Slerp(
                    currentRotation, tr,
                    deltaTime * controllerData.RotationSpeed
                );
                currentRotation = targetRotation;
                return;
            }
            
            if (_lookInputVector.sqrMagnitude > 0f && controllerData.OrientationSharpness > 0f) {
                // Smoothly interpolate from current to target look direction
                Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector,
                    1 - Mathf.Exp(-controllerData.OrientationSharpness * deltaTime)).normalized;

                // Set the current rotation (which will be used by the KinematicCharacterMotor)
                currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
            }

            Vector3 currentUp = (currentRotation * Vector3.up);
            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up,
                1 - Mathf.Exp(-controllerData.BonusOrientationSharpness * deltaTime));
            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            // Input based movement when grounded
            // Ground movement
            ActionStateMachine.CurrentState.UpdateVelocity(ref currentVelocity, deltaTime);
            MoveStateMachine.CurrentState.UpdateVelocity(ref currentVelocity, deltaTime);
        
            // Take into account additive velocity
            if (_internalVelocityAdd.sqrMagnitude > 0f) {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }

            if (_shouldResetVelocity) {
                currentVelocity = Vector3.zero;
                _shouldResetVelocity = false;
            }
        
        }

        public Vector3 ReorientedInput() {
            Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
            Vector3 inputRight = Vector3.Cross(MoveInputVector, Motor.CharacterUp);
            return Vector3.Cross(effectiveGroundNormal, inputRight).normalized * MoveInputVector.magnitude;
        }
        
        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime) {
        }

        public void PostGroundingUpdate(float deltaTime) {
            // Handle landing and leaving ground
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround) {
                OnLanded();
            }
            else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround) {
                OnLeaveStableGround();
            }
        }

        public bool CheckIfGrounded() {
            return Motor.GroundingStatus.IsStableOnGround;
        }

        public bool IsColliderValidForCollisions(Collider coll) {
            if (IgnoredColliders.Count == 0) {
                return true;
            }

            if (IgnoredColliders.Contains(coll)) {
                return false;
            }

            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport) {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport) {
        }

        public void AddVelocity(Vector3 velocity) {
            _internalVelocityAdd += velocity;
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) {
        }

        protected void OnLanded() {
            MoveStateMachine.ChangeState(LandState);
        }

        protected void OnLeaveStableGround() {
            MoveStateMachine.ChangeState(InAirState);
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider) {
        }
    }
}