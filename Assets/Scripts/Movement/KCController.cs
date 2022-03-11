using System;
using System.Collections.Generic;
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
        public KinematicCharacterMotor Motor;

        public List<Collider> IgnoredColliders = new List<Collider>();
        public Transform MeshRoot;
        public Transform CameraFollowPoint;
        public Vector3 MoveInputVector { get; private set; }

        public PlayerInputHandler InputHandler;
        public Animator Anim { get; private set; }

        #region States

        public PlayerStateMachine StateMachine { get; private set; }
        
        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerJumpState JumpState { get; private set; }
        public PlayerInAirState InAirState { get; private set; }
        public PlayerLandState LandState { get; private set; }

        [SerializeField] private KCControllerData controllerData;
        #endregion

        private Collider[] _probedColliders = new Collider[8];
        private Vector3 _lookInputVector;
        private bool _shouldBeCrouching;
        private bool _isCrouching;
        private bool _isRolling;
        private Vector3 _internalVelocityAdd = Vector3.zero;

        private void Awake() {
            // Assign the characterController to the motor
            Motor.CharacterController = this;
            
            // States
            StateMachine = new PlayerStateMachine();
            
            IdleState = new PlayerIdleState(this, StateMachine, controllerData);
            MoveState = new PlayerMoveState(this, StateMachine, controllerData);
            JumpState = new PlayerJumpState(this, StateMachine, controllerData);
            LandState = new PlayerLandState(this, StateMachine, controllerData);
            InAirState = new PlayerInAirState(this, StateMachine, controllerData);
        }

        private void Start() {
            StateMachine.Initialize(IdleState);
        }

        private void Update() {
            StateMachine.CurrentState.LogicUpdate();
        }

        private void FixedUpdate() {
            StateMachine.CurrentState.PhysicsUpdate();
        }

        public Vector3 Velocity => Motor.Velocity;

        /// <summary>
        /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerInput inputs) {
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

            // Roll input
            // if (inputs.RollDown) {
            //     if (!ShouldBeRolling) {
            //         _isRolling = true;
            //         _rollStartTime = Time.time;
            //         Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
            //         MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
            //     }
            // }

            // Crouching input
            if (inputs.CrouchDown) {
                _shouldBeCrouching = true;

                if (!_isCrouching) {
                    _isCrouching = true;
                    Motor.SetCapsuleDimensions(0.5f, controllerData.CrouchedCapsuleHeight, controllerData.CrouchedCapsuleHeight * 0.5f);
                    MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
                }
            }
            else if (inputs.CrouchUp) {
                _shouldBeCrouching = false;
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
            if (_lookInputVector.sqrMagnitude > 0f && controllerData.OrientationSharpness > 0f) {
                // Smoothly interpolate from current to target look direction
                Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector,
                    1 - Mathf.Exp(-controllerData.OrientationSharpness * deltaTime)).normalized;

                // Set the current rotation (which will be used by the KinematicCharacterMotor)
                currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
            }

            Vector3 currentUp = (currentRotation * Vector3.up);
            if (controllerData.BonusOrientationMethod == BonusOrientationMethod.TowardsGravity) {
                // Rotate from current up to invert gravity
                Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -controllerData.Gravity.normalized,
                    1 - Mathf.Exp(-controllerData.BonusOrientationSharpness * deltaTime));
                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
            }
            else if (controllerData.BonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity) {
                if (Motor.GroundingStatus.IsStableOnGround) {
                    Vector3 initialCharacterBottomHemiCenter =
                        Motor.TransientPosition + (currentUp * Motor.Capsule.radius);

                    Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp,
                        Motor.GroundingStatus.GroundNormal,
                        1 - Mathf.Exp(-controllerData.BonusOrientationSharpness * deltaTime));
                    currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) *
                                      currentRotation;

                    // Move the position to create a rotation around the bottom hemi center instead of around the pivot
                    Motor.SetTransientPosition(initialCharacterBottomHemiCenter +
                                               (currentRotation * Vector3.down * Motor.Capsule.radius));
                }
                else {
                    Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -controllerData.Gravity.normalized,
                        1 - Mathf.Exp(-controllerData.BonusOrientationSharpness * deltaTime));
                    currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) *
                                      currentRotation;
                }
            }
            else {
                Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up,
                    1 - Mathf.Exp(-controllerData.BonusOrientationSharpness * deltaTime));
                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
            }

        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            // Input based movement when grounded
            // Ground movement
            StateMachine.CurrentState.UpdateVelocity(ref currentVelocity, deltaTime);
        
            // Take into account additive velocity
            if (_internalVelocityAdd.sqrMagnitude > 0f) {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }
        
        }
        
        // public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        // {
        //     // Ground movement
        //     if (Motor.GroundingStatus.IsStableOnGround)
        //     {
        //         float currentVelocityMagnitude = currentVelocity.magnitude;
        //
        //         Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
        //
        //         // Reorient velocity on slope
        //         currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;
        //
        //         // Calculate target velocity
        //         Vector3 inputRight = Vector3.Cross(MoveInputVector, Motor.CharacterUp);
        //         Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * MoveInputVector.magnitude;
        //         Vector3 targetMovementVelocity = reorientedInput * controllerData.MaxStableMoveSpeed;
        //
        //         // Smooth movement Velocity
        //         currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-controllerData.StableMovementSharpness * deltaTime));
        //     }
        //     // Air movement
        //     else
        //     {
        //         // Add move input
        //         if (MoveInputVector.sqrMagnitude > 0f)
        //         {
        //             Vector3 addedVelocity = MoveInputVector * controllerData.AirAccelerationSpeed * deltaTime;
        //
        //             Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);
        //
        //             // Limit air velocity from inputs
        //             if (currentVelocityOnInputsPlane.magnitude < controllerData.MaxAirMoveSpeed)
        //             {
        //                 // clamp addedVel to make total vel not exceed max vel on inputs plane
        //                 Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, controllerData.MaxAirMoveSpeed);
        //                 addedVelocity = newTotal - currentVelocityOnInputsPlane;
        //             }
        //             else
        //             {
        //                 // Make sure added vel doesn't go in the direction of the already-exceeding velocity
        //                 if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
        //                 {
        //                     addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
        //                 }
        //             }
        //
        //             // Prevent air-climbing sloped walls
        //             if (Motor.GroundingStatus.FoundAnyGround)
        //             {
        //                 if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
        //                 {
        //                     Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
        //                     addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
        //                 }
        //             }
        //
        //             // Apply added velocity
        //             currentVelocity += addedVelocity;
        //         }
        //
        //         // Gravity
        //         currentVelocity += controllerData.Gravity * deltaTime;
        //
        //         // Drag
        //         currentVelocity *= (1f / (1f + (controllerDataDrag * deltaTime)));
        //     }
        //
        //     // Take into account additive velocity
        //     if (_internalVelocityAdd.sqrMagnitude > 0f)
        //     {
        //         currentVelocity += _internalVelocityAdd;
        //         _internalVelocityAdd = Vector3.zero;
        //     }
        // }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime) {

            bool shouldUncrounch = _isCrouching && !_shouldBeCrouching;

            // Handle uncrouching
            if (shouldUncrounch) {
                // Do an overlap test with the character's standing height to see if there are any obstructions
                Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                if (Motor.CharacterOverlap(
                        Motor.TransientPosition,
                        Motor.TransientRotation,
                        _probedColliders,
                        Motor.CollidableLayers,
                        QueryTriggerInteraction.Ignore) > 0) {
                    // If obstructions, just stick to crouching dimensions
                    Motor.SetCapsuleDimensions(0.5f, controllerData.CrouchedCapsuleHeight, controllerData.CrouchedCapsuleHeight * 0.5f);
                }
                else {
                    // If no obstructions, uncrouch
                    MeshRoot.localScale = new Vector3(1f, 1f, 1f);
                    _isCrouching = false;
                }
            }

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
            StateMachine.ChangeState(LandState);
        }

        protected void OnLeaveStableGround() {
            StateMachine.ChangeState(InAirState);
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider) {
        }
    }
}