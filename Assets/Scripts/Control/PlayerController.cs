using System;
using System.Collections;
using System.Collections.Generic;
using EC.Core;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using PlayerInput = EC.Core.PlayerInput;

namespace EC.Control {
    public class PlayerController : MonoBehaviour {
        
        public KCController Character;
        public KCCamera CharacterCamera;
        public DebugUI debugUI;
        
        private PlayerInputHandler playerInput;

        private void Start() {
            Cursor.lockState = CursorLockMode.Locked;

            playerInput = GetComponent<PlayerInputHandler>();

            Character.InputHandler = playerInput;
            
            // Tell camera to follow transform
            CharacterCamera.SetFollowTransform(Character.CameraFollowPoint);

            Character.StateMachine.OnStateChange += OnPlayerStateChange;

            Vector3 charStartPos = Character.transform.position;
            playerInput.OnResetCb += context => {
                if (context.performed) {
                    Character.Motor.SetPosition(charStartPos);
                    Character.ResetVelocity();
                }
            };

            // Ignore character colliders for camera obstruction checks
            CharacterCamera.IgnoredColliders.Clear();
            CharacterCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());
        }

        private void Update() {
            HandleCharacterInput();
        }

        private void LateUpdate() {
            HandleCameraInput();

            debugUI.VelocityMagnitude.text = Character.Velocity.magnitude.ToString();
        }

        private void HandleCameraInput() {
            Vector3 lookInputVector = playerInput.LookInput;

            if (Cursor.lockState != CursorLockMode.Locked) {
                lookInputVector = Vector3.zero;
            }
            
            CharacterCamera.UpdateWithInput(Time.deltaTime, 0f, lookInputVector);
        }

        private void HandleCharacterInput() {
            PlayerInput inputs = new PlayerInput();
            inputs.MoveInput = playerInput.MoveInput;
            inputs.CameraRotation = CharacterCamera.Transform.rotation;
            inputs.JumpDown = playerInput.JumpInput;
            inputs.RollDown = playerInput.RollInput;
            
            Character.SetInputs(ref inputs);

            debugUI.Velocity.text = Character.Velocity.ToString();
        }

        private void ResetPlayer() {
            
        }

        private void OnPlayerStateChange(PlayerState oldState, PlayerState newState) {
            string oldName = oldState?.GetType()?.Name;
            string newName = newState.GetType().Name;
            print($"[State Change]: Old({oldName}) New({newName})");
            debugUI.CurrentState.text = newState.GetType().Name;
        }
        
        
    }
    
}
