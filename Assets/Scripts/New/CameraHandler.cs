using System;
using System.Collections.Generic;
using UnityEngine;

namespace New {
    public class CameraHandler : MonoBehaviour {
        
        public static CameraHandler Instance;

        public Transform _TargetTransform;
        public Transform _CameraTransform;
        public Transform _CameraPivotTransform;
        public float _LookSpeed = 0.1f;
        public float _FollowSpeed = 0.1f;
        public float _PivotSpeed = 0.03f;
        public float _MinPivot = -35;
        public float _MaxPivot = 35;
        public float _MinPivotLocked = -20f;
        public float _MaxPivotLocked = 20f;
        public float _CameraSphereRadius = 0.2f;
        public float _CameraCollisionOffset = 0.2f;
        public float _MinCollisionOffset = 0.2f;
        public float _MaxLockOnDist = 30f;
        public float _ViewableAngle = 50f;
        public float _MaxDistForLookPercent = 10f;
        

        public Transform CurrentLockOnTarget;
        public Transform LeftLockTarget;
        public Transform RightLockTarget;

        [Foldout("Debug", styled = true)]
        public Material TargetDefaultMat;
        public Material TargetNearestMat;
        public Material TargetAvailableMat;
        public Material TargetLockedMat;
        public bool DisablePositionHandler;
        public bool DisableRotationHandler;
        
        public bool IsLockedOn => CurrentLockOnTarget != null;

        private Vector3 _CameraTransformPosition;
        private Vector3 _CameraFollowVelocity = Vector3.zero;
        private LayerMask _IgnoreLayers;

        private float _DefaultPosition;
        private float _TargetPosition;
        private float _LookAngle;
        private float _PivotAngle;
        private List<Transform> _AvailableTargets = new();
        
        public Transform NearestLockOnTarget { get; private set; }

        private void Awake() {
            Instance = this;
            _DefaultPosition = _CameraTransform.localPosition.z;
            _IgnoreLayers = ~(1 << 2 | 1 << 8 | 1 << 9 | 1 << 10);
        }

        public void FollowTarget(float delta) {
            if (DisablePositionHandler) return;
            Vector3 targetPos = Vector3.SmoothDamp(transform.position, _TargetTransform.position,
                ref _CameraFollowVelocity, delta / _FollowSpeed);
            transform.position = targetPos;

            HandleCameraCollision(delta);
        }

        public void HandleCameraRotation(float delta, Vector2 lookInput) {
            if (DisableRotationHandler) return;
            if (IsLockedOn) {
                float velocity = 0;
                Vector3 dir = CurrentLockOnTarget.position - transform.position;
                dir.Normalize();
                dir.y = 0;

                Quaternion targetRotation = Quaternion.LookRotation(dir);
                transform.rotation = targetRotation;

                dir = CurrentLockOnTarget.position - _CameraPivotTransform.position;
                dir.Normalize();

                targetRotation = Quaternion.LookRotation(dir);
                Vector3 eulerAngles = targetRotation.eulerAngles;
                eulerAngles.y = 0;
                eulerAngles.x = Mathf.Clamp(eulerAngles.x, _MinPivotLocked, _MaxPivotLocked);
                _CameraPivotTransform.localEulerAngles = eulerAngles;
            }
            else {
                _LookAngle += (lookInput.x * _LookSpeed) / delta;
                _PivotAngle -= (lookInput.y * _PivotSpeed) / delta;
                _PivotAngle = Mathf.Clamp(_PivotAngle, _MinPivot, _MaxPivot);

                Vector3 rotation = Vector3.zero;
                rotation.y = _LookAngle;
                Quaternion targetRotation = Quaternion.Euler(rotation);
                transform.rotation = targetRotation;

                rotation = Vector3.zero;
                rotation.x = _PivotAngle;

                targetRotation = Quaternion.Euler(rotation);
                _CameraPivotTransform.localRotation = targetRotation;
            }
        }

        public void HandleLockOn() {
            _AvailableTargets.Clear();
            LeftLockTarget = null;
            RightLockTarget = null;
            
            float shortestDistance = Mathf.Infinity;
            float shortestDistanceLeftTarget = Mathf.Infinity;
            float shortestDistanceRightTarget = Mathf.Infinity;
            float highestLookPercentage = 0f;

            Collider[] colliders = Physics.OverlapSphere(_TargetTransform.position, 26);

            for (int i = 0; i < colliders.Length; i++) {
                if (!colliders[i].CompareTag("Enemy")) continue;

                var col = colliders[i];

                Vector3 lockTargetDir = col.transform.position - _TargetTransform.position;
                float distFromTarget = Vector3.Distance(_TargetTransform.position, col.transform.position);
                float viewableAngle = Vector3.Angle(lockTargetDir, _CameraTransform.forward);

                bool isViewable = viewableAngle > -_ViewableAngle && viewableAngle < _ViewableAngle;
                if (col.transform.root != _TargetTransform.root) {
                    if (isViewable && distFromTarget <= _MaxLockOnDist) {
                        _AvailableTargets.Add(col.transform);
                        col.transform.GetComponent<MeshRenderer>().material = TargetAvailableMat;
                    }
                    else {
                        col.transform.GetComponent<MeshRenderer>().material = TargetDefaultMat;
                    }
                }
            }
            
            for (int i = 0; i < _AvailableTargets.Count; i++) {
                Transform target = _AvailableTargets[i];
                float distFromTarget = Vector3.Distance(_TargetTransform.position, _AvailableTargets[i].position);
                
                var camDir = _CameraTransform.forward;
                var dirToTarget = target.position - _CameraTransform.position;
                // Calculate how close the target is to the camera's look direction (is player looking at target?)
                var lookPercentage = Vector3.Dot(camDir.normalized, dirToTarget.normalized);
                
                if (distFromTarget < shortestDistance) {
                    shortestDistance = distFromTarget;
                    NearestLockOnTarget = target;
                }

                // Only use look percentage for target lock if target is within range
                // This prevents locking onto targets that are much further away but happen to be
                // closer to the camera direction
                if (!IsLockedOn && lookPercentage > highestLookPercentage && distFromTarget < _MaxDistForLookPercent) {
                    highestLookPercentage = lookPercentage;
                    NearestLockOnTarget = target;
                }

                if (IsLockedOn) {
                    Vector3 relativeTargetPos = _TargetTransform.InverseTransformPoint(target.position);
                    float distFromLeftTarget = 1000f; // CurrentLockOnTarget.position.x - target.position.x;
                    float distFromRightTarget = 1000f; // CurrentLockOnTarget.position.x + target.position.x;

                    if (target == CurrentLockOnTarget) continue;
                    
                    if (relativeTargetPos.x < 0f) {
                        distFromLeftTarget = Vector3.Distance(CurrentLockOnTarget.position, target.position);
                    }
                    else if (relativeTargetPos.x > 0f) {
                        distFromRightTarget = Vector3.Distance(CurrentLockOnTarget.position, target.position);
                    }
                    
                    if (relativeTargetPos.x < 0f && distFromLeftTarget < shortestDistanceLeftTarget) {
                        shortestDistanceLeftTarget = distFromLeftTarget;
                        LeftLockTarget = _AvailableTargets[i];
                    }
                    
                    if (relativeTargetPos.x > 0f && distFromRightTarget < shortestDistanceRightTarget) {
                        shortestDistanceRightTarget = distFromRightTarget;
                        RightLockTarget = _AvailableTargets[i];
                    }
                }
            }

            if (NearestLockOnTarget) {
                NearestLockOnTarget.GetComponent<MeshRenderer>().material = TargetNearestMat;
            }
            
            if (CurrentLockOnTarget) {
                CurrentLockOnTarget.GetComponent<MeshRenderer>().material = TargetLockedMat;
            }
        }
        
        private void HandleCameraCollision(float delta) {
            _TargetPosition = _DefaultPosition;
            RaycastHit hit;
            Vector3 dir = _CameraTransform.position - _CameraPivotTransform.position;
            dir.Normalize();

            if (Physics.SphereCast(_CameraPivotTransform.position, _CameraSphereRadius, dir, out hit,
                    Mathf.Abs(_TargetPosition), _IgnoreLayers)) {
                float dist = Vector3.Distance(_CameraPivotTransform.position, hit.point);
                _TargetPosition = -(dist - _CameraCollisionOffset);
            }

            if (Mathf.Abs(_TargetPosition) < _MinCollisionOffset) {
                _TargetPosition = -_MinCollisionOffset;
            }

            _CameraTransformPosition.z =
                Mathf.Lerp(_CameraTransform.localPosition.z, _TargetPosition, delta / 0.2f);
            _CameraTransform.localPosition = _CameraTransformPosition;
        }

        public void ClearLockOnTargets() {
            _AvailableTargets.Clear();
            NearestLockOnTarget = null;
            CurrentLockOnTarget = null;
        }
    }
}