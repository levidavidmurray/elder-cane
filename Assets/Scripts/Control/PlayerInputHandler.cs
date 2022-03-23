using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EC.Control {
    public class PlayerInputHandler : MonoBehaviour {

        public enum InputActionEvent {
            Started,
            Performed,
            Canceled,
            All
        }

        public static PlayerInputHandler Instance;
        
        /************************************************************************************************************************/
        
        #region Custom Callbacks

        public Action<InputAction.CallbackContext> OnResetCb;
        public Action<InputAction.CallbackContext> OnLockTargetCb;
        // public Action<Vector2> OnTargetSnapCb;
        
        #endregion
        
        /************************************************************************************************************************/
        
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool JumpInputStop { get; private set; }
        public bool SprintInput { get; private set; }
        public bool RollInput { get; private set; }
        public bool AttackLightInput { get; private set; }
        public bool AttackLightInputStop { get; private set; }
        public bool AttackHeavyInput { get; private set; }
        public bool TargetSnapLeftInput { get; private set; }
        public bool TargetSnapRightInput { get; private set; }
        
        /************************************************************************************************************************/

        [SerializeField] private float inputHoldTime = 0.2f;
        // [SerializeField] private float snapDirectionMagnitudeThreshold = 0.8f;
        // [SerializeField] private float snapDirectionTimeLimit = 0.2f;
        // [SerializeField] private float snapDirectionRepeatDotThreshold = 0.1f;
        
        /************************************************************************************************************************/

        private float jumpInputStartTime;
        private float attackInputStartTime;
        private float snapDirectionStartTime;
        private Vector2 snapDirection;
        private bool snapAttempt;
        
        /************************************************************************************************************************/

        private PlayerActions _PlayerActions;
        public PlayerActions.GameActions GameActions { get; private set; }
        
        /************************************************************************************************************************/

        private void Awake() {
            if (Instance != null) {
                Instance = this;
            }
            
            InitializeInputActions();
        }

        private void Update() {
            CheckAttackInputHoldTime();
            // CheckTargetSnapDirectionInput();
        }
        
        /************************************************************************************************************************/
        
        #region Input Action Callbacks

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

        public void OnLockTargetSnapLeft(InputAction.CallbackContext context) {
            TargetSnapLeftInput = true;
        }
        
        public void OnLockTargetSnapRight(InputAction.CallbackContext context) {
            TargetSnapRightInput = true;
        }
        
        #endregion
        
        /************************************************************************************************************************/

        public void UseJumpInput() => JumpInput = false;
        public void UseRollInput() => RollInput = false;
        public void UseAttackLightInput() => AttackLightInput = false;
        public void UseTargetSnapLeftInput() => TargetSnapLeftInput = false;
        public void UseTargetSnapRightInput() => TargetSnapRightInput = false;
        
        /************************************************************************************************************************/

        private void InitializeInputActions() {
            _PlayerActions = new PlayerActions();
            GameActions = _PlayerActions.Game;
            
            EnableAction(GameActions.Reset, OnReset);
            EnableAction(GameActions.Move, OnMove);
            EnableAction(GameActions.Look, OnLook);
            EnableAction(GameActions.Jump, OnJump);
            EnableAction(GameActions.Sprint, OnSprint);
            EnableAction(GameActions.Roll, OnRoll);
            EnableAction(GameActions.LockTarget, OnLockTarget, InputActionEvent.Performed);
            EnableAction(GameActions.AttackLight, OnAttackLight);
            EnableAction(GameActions.LockOnTargetLeft, OnLockTargetSnapLeft, InputActionEvent.Performed);
            EnableAction(GameActions.LockOnTargetRight, OnLockTargetSnapRight, InputActionEvent.Performed);
        }
        
        private void EnableAction(InputAction action, Action<InputAction.CallbackContext> actionCb, InputActionEvent actionEvent = InputActionEvent.All) {
            if (!action.enabled)
                action.Enable();
            
            switch (actionEvent) {
                case InputActionEvent.Started:
                    action.started += actionCb;
                    break;
                case InputActionEvent.Performed:
                    action.performed += actionCb;
                    break;
                case InputActionEvent.Canceled:
                    action.canceled += actionCb;
                    break;
                default:
                    EnableAction(action, actionCb, InputActionEvent.Started);
                    EnableAction(action, actionCb, InputActionEvent.Performed);
                    EnableAction(action, actionCb, InputActionEvent.Canceled);
                    break;
            }
        }

        private void CheckAttackInputHoldTime() {
            if (Time.time - attackInputStartTime >= inputHoldTime) {
                AttackLightInput = false;
                AttackHeavyInput = false;
            }
        }

        // TODO: Get this to work
        // private void CheckTargetSnapDirectionInput() {
        //     if (Time.time - snapDirectionStartTime > snapDirectionTimeLimit) {
        //         snapAttempt = false;
        //         snapDirection = Vector2.zero;
        //         return;
        //     }
        //     
        //     if (!snapAttempt) return;
        //     
        //     if (GameActions.Look.ReadValue<Vector2>().magnitude > 0) return;
        //
        //     OnTargetSnapCb?.Invoke(snapDirection);
        //     snapAttempt = false;
        //     snapDirection = Vector2.zero;
        // }
        
    }
}