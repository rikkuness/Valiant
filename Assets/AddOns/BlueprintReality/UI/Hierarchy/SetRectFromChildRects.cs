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
	public class SetRectFromChildRects : MonoBehaviour {
        public Transform group;

        public bool setWidth;
        public float widthPadding = 0;
        public bool setHeight;
        public float heightPadding = 0;
        public bool setPosX;
        public bool setPosY;

        public bool updateEveryFrame = false;
        public bool leaveIfNoChildren = false;

        private Vector3[] worldCoords = new Vector3[4];

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
            Transform actualGroup = group != null ? group : transform;

            float xMin = 0, yMin = 0, xMax = 0, yMax = 0;

            bool foundChild = false;
            for( int i = 0; i < actualGroup.childCount; i++)
            {
                Transform child = actualGroup.GetChild(i);
                if (child.gameObject.activeSelf && child.transform is RectTransform)
                {
                    (child as RectTransform).GetWorldCorners(worldCoords);
                    for (int j = 0; j < worldCoords.Length; j++)
                        worldCoords[j] = actualGroup.InverseTransformPoint(worldCoords[j]);

                    float newXMin = Mathf.Min(Mathf.Min(worldCoords[0].x, worldCoords[1].x), Mathf.Min(worldCoords[2].x, worldCoords[3].x));
                    float newXMax = Mathf.Max(Mathf.Max(worldCoords[0].x, worldCoords[1].x), Mathf.Max(worldCoords[2].x, worldCoords[3].x));
                    float newYMin = Mathf.Min(Mathf.Min(worldCoords[0].y, worldCoords[1].y), Mathf.Min(worldCoords[2].y, worldCoords[3].y));
                    float newYMax = Mathf.Max(Mathf.Max(worldCoords[0].y, worldCoords[1].y), Mathf.Max(worldCoords[2].y, worldCoords[3].y));

                    if (!foundChild)
                    {
                        xMin = newXMin;
                        xMax = newXMax;
                        yMin = newYMin;
                        yMax = newYMax; 
                        foundChild = true;
                    }
                    else
                    {
                        xMin = Mathf.Min(xMin, newXMin);
                        xMax = Mathf.Max(xMax, newXMax);
                        yMin = Mathf.Min(yMin, newYMin);
                        yMax = Mathf.Max(yMax, newYMax);
                    }
                }
            }
            if (leaveIfNoChildren && !foundChild)
                return;

            RectTransform rect = transform as RectTransform;
            if (rect == null)
                return;

            float width = xMax - xMin + widthPadding;
            float height = yMax - yMin + heightPadding;

            bool changedRect = false;
            if (setWidth && !Mathf.Approximately(rect.rect.width, width))
            {
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                changedRect = true;
            }
            if (setHeight && !Mathf.Approximately(rect.rect.height, height))
            {
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                changedRect = true;
            }

            Vector3 pos = rect.localPosition;
            if (setPosX)
                pos.x = Mathf.Lerp(xMin, xMax, rect.pivot.x);
            if (setPosY)
                pos.y = Mathf.Lerp(yMin, yMax, rect.pivot.y);
            if ((setPosX || setPosY) && rect.position != pos)
            {
                rect.localPosition = pos;
                changedRect = true;
            }

            if (changedRect && transform.parent != null)
            {
                LayoutGroup group = transform.parent.GetComponent<LayoutGroup>();
                if (group != null)
                {
                    group.enabled = false;
                    group.enabled = true;
                }
            }
        }
    }
}
