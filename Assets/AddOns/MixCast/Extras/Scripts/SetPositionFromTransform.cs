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

namespace BlueprintReality.MixCast
{
    [ExecuteInEditMode]
    public class SetPositionFromTransform : MonoBehaviour
    {
        public Transform source;
        public bool applyLocally = false;

        public Vector3 offset = Vector3.zero;
        public Vector3 multiplier = Vector3.one;

        void OnEnable()
        {
            Update();
        }

        void Update()
        {
            if (source == null)
                return;

            Vector3 relativePos = multiplier;
            if( source is RectTransform )
            {
                RectTransform sourceTransform = source as RectTransform;
                relativePos.x *= sourceTransform.rect.width;
                relativePos.y *= sourceTransform.rect.height;
            }

            Vector3 result = source.TransformPoint(relativePos) + source.rotation * offset;
            if (applyLocally)
                transform.localPosition = source.InverseTransformDirection(result - source.position);
            else
                transform.position = result;
        }
    }
}
