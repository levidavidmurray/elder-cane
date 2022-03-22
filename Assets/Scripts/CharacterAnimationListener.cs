using System;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using EC.Core;
using KinematicCharacterController;
using New;
using UnityEngine;

public class CharacterAnimationListener : MonoBehaviour {

    /************************************************************************************************************************/

    private Animator _Anim;
    private PlayerBehaviour _PlayerBehaviour;
    
    /************************************************************************************************************************/

    private int _LastFootIndexPlayed = -1;
    
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

    // Animation Event
    // footIndex 0 -> Left Foot
    // footIndex 1 -> Right Foot
    public void OnFootStep(int footIndex) {
        if (footIndex == _LastFootIndexPlayed) return;
        MasterAudio.PlaySound3DAtTransformAndForget("FootstepsRegular", transform);
        _LastFootIndexPlayed = footIndex;
    }
    
    /************************************************************************************************************************/
    
}
