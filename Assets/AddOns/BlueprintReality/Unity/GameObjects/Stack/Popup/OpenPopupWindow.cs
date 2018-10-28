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
using UnityEngine.UI;

namespace BlueprintReality.GameObjects
{
    public class OpenPopupWindow : PushToGameObjectStack
    {
        public string titleText;
        public bool titleUseLoc = true;
        public bool titleToUpper = false;

        public string contentText;
        public bool contentUseLoc = true;
        public bool contentToUpper = false;

        public bool showCloseButton = true;
        public Button.ButtonClickedEvent onCloseButtonClicked = new Button.ButtonClickedEvent();

        public List<BtnData> btnData = new List<BtnData>();

        PopupWindow popupWindow;
        GameObjectStack popupStack;        

        public bool isShowing { get { return popupWindow != null && popupWindow.gameObject.activeInHierarchy; } }
   
        public float PopupAlpha {
            get {
                return popupWindow == null ? 0f : popupWindow.GetComponent<CanvasGroup>().alpha;
            }
            set {
                if(popupWindow != null) {
                    popupWindow.GetComponent<CanvasGroup>().alpha = value;
                }
            }
        }

        [System.Serializable]
        public class BtnData
        {
            public string btnText;
            public bool btnUseLoc = true;
            public bool btnToUpper = false;
            public Button.ButtonClickedEvent btnClick;
        }

        public void Open()
        {            

            PushElement();

            if (popupStack == null)
            {
                popupStack = base.FindStack();
                if( popupStack == null )
                {
                    Debug.LogError("Couldn't find Game Object Stack to push to!");
                    return;
                }
            }
            popupWindow = popupStack.GetComponentInChildren<PopupWindow>();            

            popupWindow.CompleteTitle(titleText, titleUseLoc, titleToUpper);

            popupWindow.SetCloseButtonActive(showCloseButton, showCloseButton ? onCloseButtonClicked : null);

            popupWindow.CompleteContent(contentText, contentUseLoc, contentToUpper);
            
            foreach (var data in btnData)
            {
                popupWindow.CompleteButton(data.btnText, data.btnUseLoc, data.btnToUpper, data.btnClick);
            }
        }

        public void Close() {
            if (popupWindow == null)
                return;
            base.FindStack().RemoveElement( popupWindow.gameObject );
        }

#if UNITY_EDITOR
        void Reset()
        {
            if (prefab == null)
            {
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MixCast/Prefabs/UI/Popup/Popup Window.prefab");
            }

            if (string.IsNullOrEmpty(stackId))
            {
                stackId = "screens";
            }
        }
#endif
    }
}
