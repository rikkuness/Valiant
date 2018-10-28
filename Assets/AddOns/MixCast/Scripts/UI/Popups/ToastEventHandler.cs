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
using BlueprintReality.GameObjects;

namespace BlueprintReality.MixCast
{
    public class ToastEventHandler : MonoBehaviour
    {
        protected virtual string Category { get { return "GenericEvent"; } }

        protected virtual string ResultStartedTitle { get { return "GenericStart"; } }
        protected virtual string ResultStoppedTitle { get { return "GenericStop"; } }
        protected virtual string ResultSuccessTitle { get { return "GenericSuccess"; } }
        protected virtual string ResultErrorTitle { get { return "GenericError"; } }

        protected virtual string ResultStartedMsg { get { return "GenericStartMsg"; } }
        protected virtual string ResultStoppedMsg { get { return "GenericStopMsg"; } }
        protected virtual string ResultSuccessMsg { get { return "GenericSuccessMsg"; } }
        protected virtual string ResultErrorMsg { get { return "GenericErrorMsg"; } }

        [SerializeField] EventCenter.Result[] deactivateAfterPopupTypesDisplayed = { };

        private void HandleToastEvent(EventCenter.Result result, string locMsg)
        {
            bool displayed = false;

            if (locMsg == "##")
            {
                locMsg = null;
            }

            if (result == EventCenter.Result.Error)
            {
                displayed = HandleError(locMsg);
            }
            else if (result == EventCenter.Result.Started)
            {
                displayed = HandleStarted(locMsg);
            }
            else if (result == EventCenter.Result.Stopped)
            {
                displayed = HandleStopped(locMsg);
            }
            else if (result == EventCenter.Result.Success)
            {
                displayed = HandleSuccess(locMsg);
            }
            if (displayed && System.Array.Exists(deactivateAfterPopupTypesDisplayed, t => t == result))
            {
                gameObject.SetActive(false);
            }
        }

        // returns whether or not it was handled
        protected virtual bool HandleError(string msg, float duration = 1.5f)
        {
            if (string.IsNullOrEmpty(ResultErrorMsg))
            {
                return false;
            }
            ToastCenter.ShowToast(ResultErrorTitle, ResultErrorMsg, msg, duration);
            return true;
        }

        protected virtual bool HandleStarted(string msg, float duration = 1.5f)
        {
            if (string.IsNullOrEmpty(ResultStartedMsg))
            {
                return false;
            }
            ToastCenter.ShowToast(ResultStartedTitle, ResultStartedMsg, msg, duration);
            return true;
        }

        protected virtual bool HandleStopped(string msg, float duration = 1.5f)
        {
            if (string.IsNullOrEmpty(ResultStoppedMsg))
            {
                return false;
            }
            ToastCenter.ShowToast(ResultStoppedTitle, ResultStoppedMsg, msg, duration);
            return true;
        }

        protected virtual bool HandleSuccess(string msg, float duration = 1.5f)
        {
            if (string.IsNullOrEmpty(ResultSuccessMsg))
            {
                return false;
            }
            ToastCenter.ShowToast(ResultSuccessTitle, ResultSuccessMsg, msg, duration);
            return true;
        }

        void OnEnable()
        {
            EventCenter.AddListener(Category, HandleToastEvent);
        }
        void OnDisable()
        {
            EventCenter.RemoveListener(Category, HandleToastEvent);
        }
    }
}
#endif
