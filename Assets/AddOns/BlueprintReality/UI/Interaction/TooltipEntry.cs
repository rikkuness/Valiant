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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.UI {
	public class TooltipEntry : MonoBehaviour {
        public string groupId = "tooltip";
        public string tooltipTextId = "";
        [HideInInspector]
        public string nonLocTextString = "";

        public TooltipGroup AttachedTo { get; protected set; }

        private void OnEnable()
        {
            AttachToGroup();
        }
        private void OnDisable()
        {
            RemoveFromGroup();
        }

        void AttachToGroup()
        {
            AttachedTo = FindClosestEntry();
            if (AttachedTo != null)
            {
                AttachedTo.CurrentEntry = this;
            }
        }
        void RemoveFromGroup()
        {
            if (AttachedTo != null)
            {
                if (AttachedTo.CurrentEntry == this)
                    AttachedTo.CurrentEntry = null;
                AttachedTo = null;
            }
        }

        TooltipGroup FindClosestEntry()
        {
            List<TooltipGroup> groups = TooltipGroup.ActiveGroups.FindAll(e => e.groupId == groupId);
            int leastHops = int.MaxValue;
            TooltipGroup closestGroup = null;
            for (int i = 0; i < groups.Count; i++)
            {
                bool foundMatch = false;
                int x = 0;
                Transform myTrackedObj = transform;
                while (myTrackedObj != null && !foundMatch)
                {
                    Transform theirTrackedObj = groups[i].transform;
                    int y = 0;
                    while (theirTrackedObj != null && !foundMatch)
                    {
                        if (myTrackedObj == theirTrackedObj)
                        {
                            if (leastHops > x + y)
                            {
                                leastHops = x + y;
                                closestGroup = groups[i];
                            }
                            foundMatch = true;
                        }
                        theirTrackedObj = theirTrackedObj.parent;
                        y++;
                    }
                    myTrackedObj = myTrackedObj.parent;
                    x++;
                }
            }
            return closestGroup;
        }
    }
}
