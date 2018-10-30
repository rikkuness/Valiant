using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR.InteractionSystem;

public class LiftButton : MonoBehaviour {
    public GameObject LiftObject;
    private AnimateLift lift;

    public void OnButtonDown(Hand hand) {
        hand.TriggerHapticPulse(1000);
	    
        if (!lift.IsMoving()) {
            if (lift.IsAtTop()) {
                lift.GoDown();
            } else if (lift.IsAtBottom()) {
                lift.GoUp();
            }
        }
    }


    // Use this for initialization
    void Start () {
	lift = transform.parent.GetComponent<AnimateLift>();
    }
}
