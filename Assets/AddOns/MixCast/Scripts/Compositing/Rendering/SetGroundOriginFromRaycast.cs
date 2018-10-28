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
    [ExecuteInEditMode]
    public class SetGroundOriginFromRaycast : MonoBehaviour
    {
        public InputFeed feed;

        public string groundPositionParameter = "_GroundScreenHeight"; //0 is bottom of screen, 1 is top

        public LayerMask groundLayers = 0;
        public float maxRayLength = 10;

        private void Update()
        {
            if (feed == null || !feed.isActiveAndEnabled)
                return;

            Vector3 groundOrigin = Vector3.zero;
            if (groundLayers == 0)
            {
                groundOrigin = Camera.main.transform.position;
                groundOrigin.y = 0;
            }
            else
            {
                RaycastHit hitInfo;
                if (UnityEngine.Physics.Raycast(Camera.main.transform.position, Vector3.down, out hitInfo, maxRayLength, groundLayers, QueryTriggerInteraction.Ignore))
                    groundOrigin = hitInfo.point;
                else
                    groundOrigin = Camera.main.transform.position + Vector3.down * maxRayLength;
            }

            if( feed.blitMaterial != null && feed.blitMaterial.HasProperty(groundPositionParameter) )
            {
                MixCastCamera cam = MixCastCamera.FindCamera(feed.context);

                float output = 0;
                if (cam.gameCamera.transform.InverseTransformPoint(groundOrigin).z > 0)
                    output = Mathf.Clamp01(cam.gameCamera.WorldToViewportPoint(groundOrigin).y);
                feed.blitMaterial.SetFloat(groundPositionParameter, output);
            }
        }
    }
}
#endif
