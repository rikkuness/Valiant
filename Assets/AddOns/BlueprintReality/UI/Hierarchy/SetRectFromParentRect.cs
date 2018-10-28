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
using UnityEngine.UI;

namespace BlueprintReality.UI {
    [ExecuteInEditMode]
	public class SetRectFromParentRect : MonoBehaviour {
        public bool setWidth;
        public bool setHeight;
        public bool setPosX;
        public bool setPosY;

        public float sizeYOffset = 0;
        public float posYOffset = 0;

        public bool updateEveryFrame = false;

        private void OnEnable()
        {
            UpdateRect();
        }

        private void Update()
        {
            if (updateEveryFrame)
                UpdateRect();
        }

        void UpdateRect()
        {
            Transform parentTrans = transform.parent;
            while (parentTrans != null && !(parentTrans is RectTransform))
                parentTrans = parentTrans.parent;
            if (parentTrans == null)
                return;

            RectTransform parentRect = parentTrans as RectTransform;
            RectTransform myRect = transform as RectTransform;
            if (setWidth)
                myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentRect.rect.width);
            if (setHeight)
                myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentRect.rect.height + sizeYOffset);
            Vector3 localPos = myRect.localPosition;
            if (setPosX)
                localPos.x = 0;
            if (setPosY)
                localPos.y = 0 + posYOffset;
            myRect.localPosition = localPos;
        }
    }
}
