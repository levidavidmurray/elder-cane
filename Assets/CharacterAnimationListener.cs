using System.Collections;
using System.Collections.Generic;
using EC.Core;
using UnityEngine;

public class CharacterAnimationListener : MonoBehaviour {

    public KCController Controller;
    
    // Animation Event
    public void OnRollComplete() {
        Controller.OnRollComplete?.Invoke();
    }
}
