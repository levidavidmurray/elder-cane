using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour {
    
    [SerializeField] private Transform _FollowTarget;
    
    private void LateUpdate() {
        transform.position = _FollowTarget.position;
        
        // var euler = transform.rotation.eulerAngles;
        // euler.z = 0f;
        // transform.rotation = Quaternion.Euler(euler);
    }
}
