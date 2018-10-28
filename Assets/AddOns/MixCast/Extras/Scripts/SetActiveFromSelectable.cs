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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlueprintReality.UI
{
    public class SetActiveFromSelectable : MonoBehaviour
    {
        public enum Trigger
        {
            Hover, Press
        }

        public Selectable selectable;
        public Trigger trigger = Trigger.Hover;

        public List<GameObject> on = new List<GameObject>();
        public List<GameObject> off = new List<GameObject>();

        private EventTrigger.Entry startEv, endEv;

        List<EventTrigger.Entry> entryRemoveList = new List<EventTrigger.Entry>();

        EventTrigger.Entry AddOrCreateTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> triggerHandler) {
            foreach(var entry in trigger.triggers) {
                if(entry.eventID == type) {
                    entry.callback.AddListener( triggerHandler );
                    return entry;
                }
            }
            var newEntry = new EventTrigger.Entry();
            newEntry.eventID = type;
            newEntry.callback.AddListener( triggerHandler );
            trigger.triggers.Add( newEntry );
            return newEntry;
        }

        void RemoveTriggerEntry( EventTrigger trigger, 
                                 EventTrigger.Entry entryToRemove, 
                                 UnityEngine.Events.UnityAction<BaseEventData> handlerToRemove ) {

            foreach(var entry in trigger.triggers) {
                if(entry.eventID == entryToRemove.eventID) {
                    entry.callback.RemoveListener( handlerToRemove );
                    if(entry.callback.GetPersistentEventCount() == 0) {
                        if(!entryRemoveList.Contains(entry)) {
                            entryRemoveList.Add( entry );
                        }
                    }
                    //break; // There could be more than one existing entry of the same type, so look at all of them
                }
            }

            EventTrigger evTrigger = selectable.GetComponent<EventTrigger>();
            if(evTrigger != null) {
                foreach(var entry in entryRemoveList) {
                    if(evTrigger.triggers.Contains(entry)) {
                        evTrigger.triggers.Remove( entry );
                    }
                }
                entryRemoveList.Clear();
            }
        }

        private void OnEnable()
        {
            if (selectable == null)
                selectable = GetComponentInParent<Selectable>();

            EventTrigger evTrigger = selectable.GetComponent<EventTrigger>();
            if (evTrigger == null)
                evTrigger = selectable.gameObject.AddComponent<EventTrigger>();

            if (trigger == Trigger.Hover)
            {
                startEv = AddOrCreateTrigger( evTrigger, EventTriggerType.PointerEnter, HandleStart );
                endEv = AddOrCreateTrigger( evTrigger, EventTriggerType.PointerExit, HandleEnd );
            }
            if( trigger == Trigger.Press )
            {
                startEv = AddOrCreateTrigger( evTrigger, EventTriggerType.PointerDown, HandleStart );
                endEv = AddOrCreateTrigger( evTrigger, EventTriggerType.PointerUp, HandleEnd );
            }

            on.ForEach(g => g.SetActive(false));
            off.ForEach(g => g.SetActive(true));
        }

        private void OnDisable() {
            EventTrigger evTrigger = selectable.GetComponent<EventTrigger>();
            RemoveTriggerEntry( evTrigger, startEv, HandleStart );
            RemoveTriggerEntry( evTrigger, endEv, HandleEnd );
            
            on.ForEach(g => g.SetActive(false));
            off.ForEach(g => g.SetActive(true));
        }

        private void HandleStart(BaseEventData evData)
        {
            on.ForEach(g => g.SetActive(true));
            off.ForEach(g => g.SetActive(false));
        }
        private void HandleEnd(BaseEventData evData)
        {
            on.ForEach(g => g.SetActive(false));
            off.ForEach(g => g.SetActive(true));
        }
    }
}
