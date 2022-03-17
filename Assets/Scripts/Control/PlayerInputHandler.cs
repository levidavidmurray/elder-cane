using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EC.Control {
    public class PlayerInputHandler : MonoBehaviour {

        public static PlayerInputHandler Instance;
        
        #region Custom Callbacks

        public Action<InputAction.CallbackContext> OnResetCb;
        public Action<InputAction.CallbackContext> OnLockTargetCb;
        
        #endregion
        
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool JumpInputStop { get; private set; }
        public bool SprintInput { get; private set; }
        public bool RollInput { get; private set; }
        public bool AttackLightInput { get; private set; }
        public bool AttackLightInputStop { get; private set; }
        public bool AttackHeavyInput { get; private set; }

        [SerializeField] private float inputHoldTime = 0.2f;

        private float jumpInputStartTime;
        private float rollInputStartTime;
        private float attackInputStartTime;

        private PlayerActions _PlayerActions;
        public PlayerActions.GameActions GameActions { get; private set; }

        private void Awake() {
            if (Instance != null) {
                Instance = this;
            }
            
            InitializeInputActions();
        }

        private void Update() {
            CheckAttackInputHoldTime();
            CheckJumpInputHoldTime();
        }

        private void InitializeInputActions() {
            _PlayerActions = new PlayerActions();
            GameActions = _PlayerActions.Game;
            
            EnableAll(GameActions.Reset, OnReset);
            EnableAll(GameActions.Move, OnMove);
            EnableAll(GameActions.Jump, OnJump);
            EnableAll(GameActions.Sprint, OnSprint);
            EnableAll(GameActions.Roll, OnRoll);
        }

        private void EnableAll(InputAction action, Action<InputAction.CallbackContext> actionCb) {
            action.Enable();
            action.started += actionCb;
            action.performed += actionCb;
            action.canceled += actionCb;
        }

        public void OnMove(InputAction.CallbackContext context) {
            MoveInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context) {
            LookInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.started)
                JumpInput = true;
            if (context.canceled)
                JumpInput = false;
            // if (context.started) {
            //     JumpInput = true;
            //     JumpInputStop = false;
            //     jumpInputStartTime = Time.time;
            // }
            //
            // if (context.canceled) {
            //     JumpInputStop = true;
            // }
        }

        public void OnSprint(InputAction.CallbackContext context) {
            if (context.performed)
                SprintInput = true;
            if (context.canceled)
                SprintInput = false;
        }
        
        public void OnRoll(InputAction.CallbackContext context) {
            if (context.performed) {
                RollInput = true;
                rollInputStartTime = Time.time;
            }
        }

        public void OnAttackLight(InputAction.CallbackContext context) {
            if (context.started) {
                AttackLightInput = true;
                AttackLightInputStop = false;
                attackInputStartTime = Time.time;
                return;
            }

            if (context.performed || context.canceled) {
                AttackLightInputStop = true;
            }
        }

        public void OnReset(InputAction.CallbackContext context) {
            OnResetCb?.Invoke(context);
        }
        
        public void OnLockTarget(InputAction.CallbackContext context) {
            OnLockTargetCb?.Invoke(context);
        }

        public void UseJumpInput() => JumpInput = false;
        public void UseRollInput() => RollInput = false;
        public void UseAttackLightInput() => AttackLightInput = false;

        private void CheckAttackInputHoldTime() {
            if (Time.time - attackInputStartTime >= inputHoldTime) {
                AttackLightInput = false;
                AttackHeavyInput = false;
            }
        }

        private void CheckJumpInputHoldTime() {
            // if (Time.time - jumpInputStartTime >= inputHoldTime) {
            //     JumpInput = false;
            // }
        }

        private void CheckRollInputHoldTime() {
            if (Time.time - rollInputStartTime >= inputHoldTime) {
                RollInput = false;
            }
        }
        
    }
}