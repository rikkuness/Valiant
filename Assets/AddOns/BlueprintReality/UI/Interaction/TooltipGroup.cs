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
	public class TooltipGroup : MonoBehaviour {
        public static List<TooltipGroup> ActiveGroups { get; protected set; }
        static TooltipGroup()
        {
            ActiveGroups = new List<TooltipGroup>();
        }

        public string groupId = "tooltip";

        public GameObject tooltipGroup;
        public Text.SetTextFromLocalization tooltipLabel;

        private TooltipEntry currentEntry;
        public TooltipEntry CurrentEntry
        {
            get
            {
                return currentEntry;
            }
            set
            {
                currentEntry = value;
                tooltipGroup.SetActive(currentEntry != null);
                if( currentEntry != null ) {
                    tooltipLabel.id = currentEntry.tooltipTextId;
                    tooltipLabel.autoLocalize = !string.IsNullOrEmpty( currentEntry.tooltipTextId );
                    if(!tooltipLabel.autoLocalize && !string.IsNullOrEmpty(currentEntry.nonLocTextString)) {
                        tooltipLabel.id = currentEntry.nonLocTextString;
                    }
                    tooltipLabel.RefreshText();
                }
            }
        }

        private void OnEnable()
        {
            CurrentEntry = null;
            ActiveGroups.Add(this);
        }
        private void OnDisable()
        {
            ActiveGroups.Remove(this);
        }
    }
}
