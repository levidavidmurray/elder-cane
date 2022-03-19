using System;
using System.Collections;
using System.Collections.Generic;
using New;
using UnityEngine;

public class LockableTarget : MonoBehaviour {

    public CameraViewTargetSelector ViewTargetSelector;
    
    /************************************************************************************************************************/
    
    private void OnBecameVisible() {
        print($"{transform.name} VISIBLE");
        ViewTargetSelector.AddTarget(transform);
    }

    private void OnBecameInvisible() {
        print($"INVISIBLE {transform.name}");
        ViewTargetSelector.RemoveTarget(transform);
    }
    
    /************************************************************************************************************************/
    
}
