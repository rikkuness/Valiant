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

namespace BlueprintReality.GameObjects
{
    public class PopupWindow : MonoBehaviour {
        public UnityEngine.UI.Text title;

        public GameObject closeButton;

        public UnityEngine.UI.Text content;

        public Transform buttons;

        public GameObject btnPrefab;

        private Button.ButtonClickedEvent onCloseClicked;

        void CompleteText( UnityEngine.UI.Text uitext, string text, bool useLoc, bool toUpper ) {
            var loc = uitext.gameObject.GetComponent<Text.SetTextFromLocalization>();
            if(useLoc) {
                loc.id = text;
                loc.toUpper = toUpper;
                loc.RefreshText();
            } else {
                loc.enabled = false;
                uitext.text = (toUpper) ? (text.ToUpper()) : (text);
            }
        }

        public void CompleteTitle( string titleText, bool titleUseLoc, bool titleToUpper ) {
            CompleteText( title, titleText, titleUseLoc, titleToUpper );
        }

        public void CompleteContent( string contentText, bool contentUseLoc, bool contentToUpper ) {
            CompleteText( content, contentText, contentUseLoc, contentToUpper );
        }

        public void CompleteButton( string btnText, bool btnUseLoc, bool btnToUpper, Button.ButtonClickedEvent btnClick ) {
            var btnObj = AddPrefab( buttons, btnPrefab );
            var btn = btnObj.GetComponent<Button>();
            var btnUIText = btnObj.GetComponentInChildren<UnityEngine.UI.Text>();

            CompleteText( btnUIText, btnText, btnUseLoc, btnToUpper );

            btn.onClick = btnClick;
            buttons.gameObject.SetActive( true );
        }

        public void SetCloseButtonActive(bool active, Button.ButtonClickedEvent onClicked)
        {
            if (closeButton == null)
                return;
            closeButton.SetActive(active);
            onCloseClicked = onClicked;
        }

        GameObject AddPrefab( Transform parent, GameObject prefab ) {
            GameObject instance = null;

            instance = Instantiate( prefab, parent );

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            instance.gameObject.layer = parent.gameObject.layer;
            instance.name = prefab.name;

            return instance;
        }

        public void HandleCloseButtonClicked()
        {
            GameObjectStackElement element = GetComponentInParent<GameObjectStackElement>();

            GameObjectStack stack = element.GetComponentInParent<GameObjectStack>();
            if (stack.stack.Count > 0 && stack.stack[stack.stack.Count - 1] == element)
            {
                stack.PopTopElement();
                if (onCloseClicked != null)
                    onCloseClicked.Invoke();
            }
        }

        public float Alpha {
            get {
                return GetComponent<CanvasGroup>().alpha;
            }
            set {
                GetComponent<CanvasGroup>().alpha = value;
            }
        }
    }
}
