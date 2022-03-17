using System;
using System.Collections;
using System.Collections.Generic;
using EC.Core;
using KinematicCharacterController;
using New;
using UnityEngine;

public class CharacterAnimationListener : MonoBehaviour {

    /************************************************************************************************************************/

    private Animator _Anim;
    private PlayerBehaviour _PlayerBehaviour;
    
    /************************************************************************************************************************/

    private void Awake() {
        _Anim = GetComponent<Animator>();
        _PlayerBehaviour = GetComponentInParent<PlayerBehaviour>();
        _Anim.applyRootMotion = false;
    }

    private void OnAnimatorMove() {
        if (!_Anim.applyRootMotion) return;
        _PlayerBehaviour.Motor.SetPosition(transform.position + _Anim.deltaPosition);
    }
    
    /************************************************************************************************************************/
    
}
