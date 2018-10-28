/**********************************************************************************
* Blueprint Reality Inc. CONFIDENTIAL
* 2018 Blueprint Reality Inc.
* All Rights Reserved.
*
* NOTICE:  All information contained herein is, and remains, the property of
* Blueprint Reality Inc. and its suppliers, if any.  The intellectual and
* technical concepts contained herein are proprietary to Blueprint Reality Inc.
* and its suppliers and may be covered by Patents, pending patents, and are
* protected by trade secret or copyright law.
*
* Dissemination of this information or reproduction of this material is strictly
* forbidden unless prior written permission is obtained from Blueprint Reality Inc.
***********************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BlueprintReality.MixCast
{
    public class ClearSelectedFromEventSystem : MonoBehaviour
    {
        Selectable selectable = null;
        
        private EventTrigger.Entry clickEv;
        void OnEnable()
        {
            if (selectable == null)
            {
                selectable = GetComponentInParent<Selectable>();
            }

            if (selectable != null)
            {
                EventTrigger evTrigger = selectable.GetComponent<EventTrigger>();

                if (evTrigger == null)
                {
                    evTrigger = selectable.gameObject.AddComponent<EventTrigger>();
                }
                if(clickEv != null) {
                    evTrigger.triggers.Remove( clickEv );
                }
                clickEv = new EventTrigger.Entry();
                clickEv.eventID = EventTriggerType.PointerClick;
                clickEv.callback.AddListener(CancelSelect);
                evTrigger.triggers.Add(clickEv);
            }

        }
        void OnDisable()
        {
            if (selectable != null)
            {
                EventTrigger evTrigger = selectable.GetComponent<EventTrigger>();

                if (evTrigger != null)
                {
                    evTrigger.triggers.Remove(clickEv);
                    clickEv.callback.RemoveAllListeners();
                    clickEv = null;
                }
            }
        }

        void CancelSelect(BaseEventData arg0)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
