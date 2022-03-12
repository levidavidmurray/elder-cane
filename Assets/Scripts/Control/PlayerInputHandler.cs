using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EC.Control {
    public class PlayerInputHandler : MonoBehaviour {
        
        #region Custom Callbacks

        public Action<InputAction.CallbackContext> OnResetCb;
        
        #endregion
        
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool JumpInputStop { get; private set; }
        public bool RollInput { get; private set; }
        public bool RollInputStop { get; private set; }

        [SerializeField] private float inputHoldTime = 0.2f;

        private float jumpInputStartTime;
        private float rollInputStartTime;

        private void Update() {
            CheckJumpInputHoldTime();
            CheckRollInputHoldTime();
        }

        public void OnMove(InputAction.CallbackContext context) {
            MoveInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context) {
            LookInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.started) {
                JumpInput = true;
                JumpInputStop = false;
                jumpInputStartTime = Time.time;
            }

            if (context.canceled) {
                JumpInputStop = true;
            }
        }

        public void OnRoll(InputAction.CallbackContext context) {
            if (context.started) {
                RollInput = true;
                RollInputStop = false;
                rollInputStartTime = Time.time;
            }

            if (context.canceled) {
                RollInputStop = true;
            }
        }

        public void OnReset(InputAction.CallbackContext context) {
            OnResetCb?.Invoke(context);
        }

        public void UseJumpInput() => JumpInput = false;
        
        public void UseRollInput() => RollInput = false;

        private void CheckJumpInputHoldTime() {
            if (Time.time - jumpInputStartTime >= inputHoldTime) {
                JumpInput = false;
            }
        }

        private void CheckRollInputHoldTime() {
            if (Time.time - rollInputStartTime >= inputHoldTime) {
                RollInput = false;
            }
        }
        
    }
}