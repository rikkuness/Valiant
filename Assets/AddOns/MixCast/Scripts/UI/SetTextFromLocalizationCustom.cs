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

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using UnityEngine;

namespace BlueprintReality.Text
{
    public class SetTextFromLocalizationCustom : MonoBehaviour
    {
        public UnityEngine.UI.Text textCustom;
        public string id;
        public bool toUpper = false;

        protected void OnEnable()
        {
            if (textCustom == null)
                textCustom = GetComponent<UnityEngine.UI.Text>();
            
            Localization.LanguageChanged += RefreshText;
            RefreshText();
        }
        protected void OnDisable()
        {
            Localization.LanguageChanged -= RefreshText;
        }

        void RefreshText()
        {
            string text = Localization.Get(id);

            if (toUpper)
                text = text.ToUpper();

            if (textCustom != null)
                textCustom.text = text;
        }
    }
}
#endif
