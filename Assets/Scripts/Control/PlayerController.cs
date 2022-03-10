using System;
using System.Collections;
using System.Collections.Generic;
using EC.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EC.Control {
    public class PlayerController : MonoBehaviour {
        
        public KCController Character;
        public KCCamera CharacterCamera;

        private Vector2 m_playerMoveInput;
        private Vector2 m_playerLookInput;
        private bool m_jumpDown;
        private bool m_rollDown;

        private void Start() {
            Cursor.lockState = CursorLockMode.Locked;
            
            // Tell camera to follow transform
            CharacterCamera.SetFollowTransform(Character.CameraFollowPoint);
            
            // Ignore character colliders for camera obstruction checks
            CharacterCamera.IgnoredColliders.Clear();
            CharacterCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());
        }

        private void Update() {
            HandleCharacterInput();
        }

        private void LateUpdate() {
            HandleCameraInput();
        }

        public void OnMove(InputAction.CallbackContext context) {
            m_playerMoveInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context) {
            m_playerLookInput = context.ReadValue<Vector2>();
            print($"Look Input: {m_playerLookInput}");
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.canceled) {
                m_jumpDown = false;
                return;
            }
            
            if (context.started) m_jumpDown = true;
        }

        public void OnRoll(InputAction.CallbackContext context) {
            if (context.canceled) {
                m_rollDown = false;
                return;
            }
            
            if (context.started) m_rollDown = true;
        }

        public void OnFire(InputAction.CallbackContext context) {
            
        }

        private void HandleCameraInput() {
            Vector3 lookInputVector = m_playerLookInput;

            if (Cursor.lockState != CursorLockMode.Locked) {
                lookInputVector = Vector3.zero;
            }
            
            CharacterCamera.UpdateWithInput(Time.deltaTime, 0f, lookInputVector);
        }

        private void HandleCharacterInput() {
            PlayerCharacterInputs inputs = new PlayerCharacterInputs();
            inputs.MoveInput = m_playerMoveInput;
            inputs.MoveAxisForward = m_playerMoveInput.y;
            inputs.MoveAxisRight = m_playerMoveInput.x;
            inputs.CameraRotation = CharacterCamera.Transform.rotation;
            inputs.JumpDown = m_jumpDown;
            inputs.RollDown = m_rollDown;
            
            Character.SetInputs(ref inputs);
        }
        
        
    }
    
}
