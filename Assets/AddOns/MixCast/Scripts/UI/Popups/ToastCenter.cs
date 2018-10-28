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
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.GameObjects
{
    public class ToastCenter : MonoBehaviour
    {
        const float TIME_BETWEEN_TOASTS          = 0.1f;
        const float TIME_TOAST_DISPLAYED_DEFAULT = 3f;
        const float TIME_TWEEN                   = 0.25f;

        public class ToastData {
            public string locTitle;
            public string locMsg;
            public float  duration;
            public string plainMsg; // takes precedence over locMsg, assuming the user will localize it manually when needed.

            public ToastData( string title, string msg, float timeDisplayed, string nonLocMsg = null ) {
                locTitle = title;
                locMsg   = msg;
                duration = timeDisplayed;
                plainMsg = nonLocMsg;
            }
        }
        GameObject  popupWindowPrefab;
        PopupWindow popupWindow;

        List<ToastData> toasts;
        ToastData       currentToast;

        float timeElapsed   = 0f;

        static ToastCenter _instance = null;

        // nonLocMsg --> If this is not null/empty, will override loc_msg.
        public static void ShowToast( string loc_title, string loc_msg, string nonLocMsg = null, float duration = TIME_TOAST_DISPLAYED_DEFAULT ) {
            if(nonLocMsg == "##") {
                nonLocMsg = null;
            }
            foreach(var toast in Instance.toasts) {
                if(toast.locTitle == loc_title && toast.locMsg == loc_msg && toast.plainMsg == nonLocMsg) {
                    return;
                }
            }            
            Instance.toasts.Add( new ToastData( loc_title, loc_msg, duration, nonLocMsg ) );
        }

        // call Initialize() when the app loads.  This will create the singleton game object.
        public static void Initialize() {
            if(_instance == null) {
                GameObject go = new GameObject( "ToastCenter" );
                _instance = go.AddComponent<ToastCenter>(); // will finish initializing in OnEnable()
            }
        }

        public static void Disable() {
            if(_instance != null) {
                GameObject.Destroy( _instance.gameObject );
                _instance = null;
            }
        }

        /// <summary>
        ///  PRIVATE FUNCTIONS
        /// </summary>

        private static ToastCenter Instance {
            get {
                if(_instance == null) {
                    Debug.LogError( "ToastCenter::Instance -- This script must be attached to a global game object" );
                }
                return _instance;
            }
        }
        
        void OnEnable() {
            if(_instance != null && _instance != this) {
                Destroy( gameObject );
                return;
            }
            if(_instance == null) {
                _instance = this;
                _instance.InitInternal();
                DontDestroyOnLoad( _instance.gameObject );
            }
        }

        void OnDestroy() {
            if(this == _instance) {
                _instance = null;
                toasts.Clear();
            }
        }

        void CreatePopup( string title, bool locTitle, string content, bool locContent, float startingAlpha ) {
            if(popupWindow != null) {
                GameObject.Destroy( popupWindow.gameObject );
                popupWindow = null;
            }

            popupWindow = BlueprintReality.Utility.ScreenUtility.InstantiateInTopScene( popupWindowPrefab ).GetComponent<PopupWindow>();
            popupWindow.transform.SetAsLastSibling();
            popupWindow.CompleteTitle( title, locTitle, false );
            popupWindow.CompleteContent( content, locContent, false );
            popupWindow.Alpha = startingAlpha;
        }

        void InitInternal() {
            toasts = new List<ToastData>();
            popupWindowPrefab = (GameObject)Resources.Load( "Toast Popup" );
        }

        // Update is called once per frame
        void Update() {
            if(!BlueprintReality.Utility.ScreenUtility.AreAllScenesLoaded()) {
                return;
            }
            Instance.timeElapsed += Time.deltaTime;
            if(currentToast != null) {
                if(Instance.timeElapsed >= currentToast.duration + TIME_TWEEN) {
                    Instance.timeElapsed = 0f;
                    GameObject.Destroy(Instance.popupWindow.gameObject);
                    Instance.popupWindow = null;
                    currentToast = null;
                } else {
                    if(Instance.timeElapsed < TIME_TWEEN) {
                        PopupAlphaTween( Instance.timeElapsed, 0f, 1f ); // fade in
                    } else if(timeElapsed >= currentToast.duration) {
                        PopupAlphaTween( Instance.timeElapsed - currentToast.duration, 1f, 0f ); // fade out
                    }
                }
            } else if(Instance.toasts.Count > 0 && Instance.timeElapsed >= TIME_BETWEEN_TOASTS) {
                Instance.timeElapsed = 0f;
                currentToast = Instance.toasts[0];
                Instance.toasts.RemoveAt( 0 );

                string titleText   = currentToast.locTitle;
                string contentText = string.IsNullOrEmpty( currentToast.plainMsg ) ? currentToast.locMsg : currentToast.plainMsg;
                bool   contentLoc  = string.IsNullOrEmpty( currentToast.plainMsg );
                bool   titleLoc    = true;
                
                CreatePopup( titleText, titleLoc, contentText, contentLoc, 0f );
            }
        }

        void PopupAlphaTween(float timeElapsed, float alphaStart, float alphaEnd) {
            float percent = timeElapsed / TIME_TWEEN;

            float alpha = Mathf.Clamp( percent * (alphaEnd - alphaStart) + alphaStart,
                Mathf.Min( alphaStart, alphaEnd ),
                Mathf.Max( alphaStart, alphaEnd ) );

            Instance.popupWindow.Alpha = alpha;
        }
    }
}
#endif
