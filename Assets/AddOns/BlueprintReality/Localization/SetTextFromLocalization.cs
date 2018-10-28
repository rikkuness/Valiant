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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintReality.Text
{
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class SetTextFromLocalization : MonoBehaviour
    {
        public string id;
        public bool toUpper = false;
        public bool useNewlines = false;
        [HideInInspector]
        public bool autoLocalize = true;

        protected void OnEnable()
        {
            Localization.LanguageChanged += RefreshText;
            RefreshText();
        }
        protected void OnDisable()
        {
            Localization.LanguageChanged -= RefreshText;
        }

        public void RefreshText()
        {
            string text = autoLocalize ? Localization.Get(id) : id;
            if (toUpper)
                text = text.ToUpper();
            if(useNewlines)
                text = text.Replace( "\\n", "\n" ).Replace( "<br>", "\n" );
            
            GetComponent<UnityEngine.UI.Text>().text = text;
        }
    }
}
