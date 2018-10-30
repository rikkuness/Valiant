using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Valve.VR.InteractionSystem
{
    public class ShowMenu : MonoBehaviour
    {
        public SteamVR_Action_Single showMenu;
        public GameObject menuUI;
        private Hand hand;

        private void OnEnable() {
            if (hand == null)
                hand = this.GetComponent<Hand>();

            if (showMenu == null) {
                Debug.LogError("No show menu action assigned");
                return;
            }
        }

        private void Update() {
            menuUI.transform.localScale = new Vector3(showMenu.GetAxis(hand.handType),showMenu.GetAxis(hand.handType),showMenu.GetAxis(hand.handType));
        }
    }
}
