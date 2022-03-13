using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using EC.Core;
using UI;
using UnityEngine;
using Cursor = UnityEngine.Cursor;
using PlayerInput = EC.Core.PlayerInput;

namespace EC.Control {
    public class PlayerController : MonoBehaviour {
        
        public KCController Character;
        public Camera CharacterCamera;
        public DebugUI debugUI;

        public CinemachineVirtualCamera CameraLocked;
        public CinemachineFreeLook CameraFreeLook;
        public CinemachineTargetGroup TargetGroup;

        public Transform LockTargetTest;
        
        private PlayerInputHandler InputHandler;
        private Transform _lockTarget;

        private void Start() {
            Cursor.lockState = CursorLockMode.Locked;

            InputHandler = GetComponent<PlayerInputHandler>();

            Character.InputHandler = InputHandler;
            
            // Tell camera to follow transform
            // CharacterCamera.SetFollowTransform(Character.CameraFollowPoint);

            Character.StateMachine.OnStateChange += OnPlayerStateChange;

            Vector3 charStartPos = Character.transform.position;
            
            InputHandler.OnResetCb += context => {
                if (!context.performed) return;
                Character.Motor.SetPosition(charStartPos);
                Character.ResetVelocity();
            };

            InputHandler.OnLockTargetCb += context => {
                if (!context.performed) return;

                // Lock Target
                if (!_lockTarget) {
                    _lockTarget = LockTargetTest;
                    TargetGroup.AddMember(_lockTarget, 1f, 2f);
                    CameraLocked.m_Priority = 11;
                    return;
                }

                // Unlock Target
                if (_lockTarget) {
                    TargetGroup.RemoveMember(_lockTarget);
                    _lockTarget = null;
                    CameraLocked.m_Priority = 9;
                }
            };

            // Ignore character colliders for camera obstruction checks
            // CharacterCamera.IgnoredColliders.Clear();
            // CharacterCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());
        }

        private void Update() {
            HandleCharacterInput();
        }

        private void LateUpdate() {
            HandleCameraInput();
            debugUI.VelocityMagnitude.text = Character.Velocity.magnitude.ToString();
        }

        private void HandleCameraInput() {
            Vector3 lookInputVector = InputHandler.LookInput;

            if (Cursor.lockState != CursorLockMode.Locked) {
                lookInputVector = Vector3.zero;
            }
            
            // CharacterCamera.UpdateWithInput(Time.deltaTime, 0f, lookInputVector);

            // CharacterCamera.SetLockTarget(Character.LockedTarget);
        }

        private void HandleCharacterInput() {
            PlayerInput inputs = new PlayerInput();
            inputs.MoveInput = InputHandler.MoveInput;
            inputs.CameraRotation = CharacterCamera.transform.rotation;
            inputs.JumpDown = InputHandler.JumpInput;
            inputs.RollDown = InputHandler.RollInput;
            inputs.LockTarget = _lockTarget;
            
            Character.SetInputs(ref inputs);

            debugUI.Velocity.text = Character.Velocity.ToString();
        }

        private void OnPlayerStateChange(PlayerState oldState, PlayerState newState) {
            string oldName = oldState?.GetType()?.Name;
            string newName = newState.GetType().Name;
            print($"[State Change]: Old({oldName}) New({newName})");
            debugUI.CurrentState.text = newState.GetType().Name;
        }
        
        
    }
    
}
