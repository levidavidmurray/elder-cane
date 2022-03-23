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
        public float _CameraSphereRadius = 0.2f;
        public float _CameraCollisionOffset = 0.2f;
        public float _MinCollisionOffset = 0.2f;
        public float _MaxLockOnDist = 30f;

        public Transform CurrentLockOnTarget;
        public Transform LeftLockTarget;
        public Transform RightLockTarget;
        public bool IsLockedOn => CurrentLockOnTarget != null;

        private Transform _MyTransform;
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
            _MyTransform = transform;
            _DefaultPosition = _CameraTransform.localPosition.z;
            _IgnoreLayers = ~(1 << 2 | 1 << 8 | 1 << 9 | 1 << 10);
        }

        public void FollowTarget(float delta) {
            Vector3 targetPos = Vector3.SmoothDamp(_MyTransform.position, _TargetTransform.position,
                ref _CameraFollowVelocity, delta / _FollowSpeed);
            _MyTransform.position = targetPos;

            HandleCameraCollision(delta);
        }

        public void HandleCameraRotation(float delta, Vector2 lookInput) {
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
                _CameraPivotTransform.localEulerAngles = eulerAngles;
            }
            else {
                _LookAngle += (lookInput.x * _LookSpeed) / delta;
                _PivotAngle -= (lookInput.y * _PivotSpeed) / delta;
                _PivotAngle = Mathf.Clamp(_PivotAngle, _MinPivot, _MaxPivot);

                Vector3 rotation = Vector3.zero;
                rotation.y = _LookAngle;
                Quaternion targetRotation = Quaternion.Euler(rotation);
                _MyTransform.rotation = targetRotation;

                rotation = Vector3.zero;
                rotation.x = _PivotAngle;

                targetRotation = Quaternion.Euler(rotation);
                _CameraPivotTransform.localRotation = targetRotation;
            }
        }

        public void HandleLockOn() {
            float shortestDistance = Mathf.Infinity;
            float shortestDistanceLeftTarget = Mathf.Infinity;
            float shortestDistanceRightTarget = Mathf.Infinity;

            Collider[] colliders = Physics.OverlapSphere(_TargetTransform.position, 26);

            for (int i = 0; i < colliders.Length; i++) {
                if (!colliders[i].CompareTag("Enemy")) continue;

                var col = colliders[i];

                Vector3 lockTargetDir = col.transform.position - _TargetTransform.position;
                float distFromTarget = Vector3.Distance(_TargetTransform.position, col.transform.position);
                float viewableAngle = Vector3.Angle(lockTargetDir, _CameraTransform.forward);

                bool isViewable = viewableAngle > -50f && viewableAngle < 50f;
                if (col.transform.root != _TargetTransform.root && isViewable && distFromTarget <= _MaxLockOnDist) {
                    _AvailableTargets.Add(col.transform);
                }
            }

            for (int i = 0; i < _AvailableTargets.Count; i++) {
                float distFromTarget = Vector3.Distance(_TargetTransform.position, _AvailableTargets[i].position);
                
                if (distFromTarget < shortestDistance) {
                    shortestDistance = distFromTarget;
                    NearestLockOnTarget = _AvailableTargets[i];
                }

                if (IsLockedOn) {
                    var target = _AvailableTargets[i];
                    Vector3 relativeTargetPos = CurrentLockOnTarget.InverseTransformPoint(target.position);
                    float distFromLeftTarget = CurrentLockOnTarget.position.x - target.position.x;
                    float distFromRightTarget = CurrentLockOnTarget.position.x + target.position.x;

                    if (relativeTargetPos.x > 0f && distFromLeftTarget < shortestDistanceLeftTarget) {
                        shortestDistanceLeftTarget = distFromLeftTarget;
                        LeftLockTarget = _AvailableTargets[i];
                    }
                    
                    if (relativeTargetPos.x < 0f && distFromRightTarget < shortestDistanceRightTarget) {
                        shortestDistanceRightTarget = distFromRightTarget;
                        RightLockTarget = _AvailableTargets[i];
                    }
                }
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