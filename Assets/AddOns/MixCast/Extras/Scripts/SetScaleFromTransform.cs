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
    public class SetScaleFromTransform : MonoBehaviour
    {
        public Transform source;
        public bool applyLocally = true;

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

            Vector3 relative = multiplier;
            if( source is RectTransform )
            {
                RectTransform sourceTransform = source as RectTransform;
                relative.x *= sourceTransform.rect.width;
                relative.y *= sourceTransform.rect.height;
            }

            if (applyLocally)
                transform.localScale = offset + Vector3.Scale(relative, source.localScale);
            else
            {
                transform.localScale = offset + Vector3.Scale(relative, source.lossyScale);
            }
        }
    }
}
