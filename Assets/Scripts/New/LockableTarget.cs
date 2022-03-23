using System;
using System.Collections;
using System.Collections.Generic;
using New;
using UnityEngine;

public class LockableTarget : MonoBehaviour {

    public CameraViewTargetSelector ViewTargetSelector;
    
    /************************************************************************************************************************/
    
    private void OnBecameVisible() {
        ViewTargetSelector.AddTarget(transform);
    }

    private void OnBecameInvisible() {
        ViewTargetSelector.RemoveTarget(transform);
    }
    
    /************************************************************************************************************************/
    
}
