using System;
using System.Collections.Generic;
using EC.Control;
using UnityEngine;

namespace New {
    public class CameraViewTargetSelector : MonoBehaviour {
        
        /************************************************************************************************************************/
        
        public Camera ViewCamera;
        public LayerMask ObstructionMask;
        
        /************************************************************************************************************************/

        public Transform TargetToLock { get; private set; }
        
        /************************************************************************************************************************/
        
        private List<Transform> _PossibleTargets = new();
        private PlayerInputHandler _InputHandler;
        
        /************************************************************************************************************************/

        private void Awake() {
            _InputHandler = GetComponent<PlayerInputHandler>();

            // _InputHandler.OnTargetSnapCb += snapDir => {
            //     print($"TARGET SNAP: {snapDir}");
            // };
        }

        private void Update() {
            var camPos = ViewCamera.transform.position;
            var camForward = ViewCamera.transform.forward;
            Debug.DrawLine(camPos, camPos + (camForward * 10f), Color.red);
            
            if (TargetToLock) {
                var targetPos = TargetToLock.position;
                Debug.DrawLine(camPos, targetPos, Color.green);
            }
        }
        
        /************************************************************************************************************************/

        public void AddTarget(Transform target) {
            _PossibleTargets.Add(target);
        }

        public void RemoveTarget(Transform target) {
            _PossibleTargets.Remove(target);
        }

        public Transform GetTargetToLock() {
            var camTransform = ViewCamera.transform;

            Transform targetToLock = null;
            float highestLookPercentage = 0f;
            
            for (int i = 0; i < _PossibleTargets.Count; i++) {
                var target = _PossibleTargets[i];
                
                var camDir = camTransform.forward;
                var dirToTarget = target.position - camTransform.position;

                var lookPercentage = Vector3.Dot(camDir.normalized, dirToTarget.normalized);

                if (lookPercentage > highestLookPercentage) {
                    highestLookPercentage = lookPercentage;
                    targetToLock = target;
                }
            }

            if (targetToLock) {
                print($"TargetToLock: {targetToLock.name} LookPercentage: {highestLookPercentage}");
            }

            if (TargetIsObstructed(targetToLock)) return null;

            return targetToLock;
        }
        
        /************************************************************************************************************************/

        private bool TargetIsObstructed(Transform target) {
            var camPos = ViewCamera.transform.position;
            var dirToTarget = camPos - target.position;
            RaycastHit hit;
            bool didHit = Physics.Raycast(camPos, dirToTarget, out hit, dirToTarget.magnitude, ObstructionMask,
                QueryTriggerInteraction.Ignore);

            return didHit;
        }
        
    }
}