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

namespace BlueprintReality.MixCast
{
    public class SetBufferDelayFromShortcutKey : CameraComponent
    {
        [SerializeField] float minVal = 0.0f;
        [SerializeField] float maxVal = 5000f;

        [SerializeField] float step = 25f;
        
        [SerializeField] KeyCode goUpKey = KeyCode.RightBracket;
        [SerializeField] KeyCode goUpModifier = KeyCode.None;

        [SerializeField] KeyCode goDownKey = KeyCode.LeftBracket;
        [SerializeField] KeyCode goDownModifier = KeyCode.None;

        [SerializeField] KeyCode excludedKey1 = KeyCode.LeftControl;
        [SerializeField] KeyCode excludedKey2 = KeyCode.RightControl;

        void Update()
        {
            if (MixCast.IsRecordingOrStreaming() || context == null || context.Data == null || context.Data.unplugged || SelectableUtility.isInputFieldFocused)
            {
                return;
            }

            bool goUp = Input.GetKeyDown(goUpKey);
            if (goUpModifier != KeyCode.None)
                goUp &= Input.GetKey(goUpModifier);

            bool goDown = Input.GetKeyDown(goDownKey);
            if (goDownModifier != KeyCode.None)
                goDown &= Input.GetKey(goDownModifier);
            
            if (goUp && !Input.GetKey(excludedKey1) && !Input.GetKey(excludedKey2))
            {
                UpdateValue(step);
                Debug.Log("Increase buffer timing to " + context.Data.bufferTime);
            }
            else if (goDown && !Input.GetKey(excludedKey1) && !Input.GetKey(excludedKey2))
            {
                UpdateValue(-step);
                Debug.Log("Decrease buffer timing " + context.Data.bufferTime);
            }
        }

        private void UpdateValue(float delta)
        {
            context.Data.bufferTime = Mathf.Clamp( context.Data.bufferTime + delta, minVal, maxVal );
        }
    }
}
#endif
