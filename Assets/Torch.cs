using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Valve.VR.InteractionSystem {
  public class Torch : MonoBehaviour {
	  
    public SteamVR_Action_Boolean torchToggle;
    public Hand hand;
    public Light torchBulb;
    
    private void OnEnable() {
      if (hand == null)
        hand = this.GetComponent<Hand>();

      if (torchToggle == null)
      {
        Debug.LogError("No torch action assigned");
        return;
      }

      torchToggle.AddOnChangeListener(OnTorchToggle, hand.handType);
    }

    private void OnTorchToggle(SteamVR_Action_In actionIn) {
      if (torchToggle.GetStateDown(hand.handType)) {
        torchBulb.enabled = !torchBulb.enabled;
      }
    }
  }
}
