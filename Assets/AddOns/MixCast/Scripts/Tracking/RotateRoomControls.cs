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
    public class RotateRoomControls : MonoBehaviour
    {
        public float angleStep = 90f;
        public KeyCode[] clockwiseKeys = { KeyCode.LeftArrow, KeyCode.A };
        public KeyCode[] counterClockwiseKeys = { KeyCode.RightArrow, KeyCode.D };
        public KeyCode[] modifiers = {};

        bool shouldGoClockwise
        {
            get { return IsKeyPressed(clockwiseKeys); }
        }

        bool shouldGoCounterClockwise
        {
            get { return IsKeyPressed(counterClockwiseKeys); }
        }

        void Update()
        {
            if (SelectableUtility.isInputFieldFocused)
            {
                return;
            }

            if (shouldGoClockwise)
            {
                MixCastCameras.Instance.RoomTransform.Rotate(0, angleStep, 0);
            }  
            else if (shouldGoCounterClockwise)
            {
                MixCastCameras.Instance.RoomTransform.Rotate(0, -angleStep, 0);
            } 
        }

        bool IsKeyPressed(KeyCode[] keys)
        {
            var isPressed = false;
            for( int i = 0; i < keys.Length && !isPressed; i++ )
                isPressed |= Input.GetKeyDown(keys[i]);

            if (modifiers.Length > 0)
            {
                bool modPressed = false;
                for (int i = 0; i < modifiers.Length && !modPressed; i++)
                    modPressed |= Input.GetKey(modifiers[i]);
                isPressed &= modPressed;
            }

            return isPressed;
        }
    }
}
#endif
