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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class MixCastLight : MonoBehaviour
    {
        public static HashSet<Light> ActiveDirectionalLights = new HashSet<Light>();
        public static HashSet<Light> ActivePointLights = new HashSet<Light>();

        void OnEnable()
        {
            var lights = gameObject.GetComponents<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                switch(lights[i].type)
                {
                    case LightType.Directional:
                        ActiveDirectionalLights.Add(lights[i]);
                        break;
                    case LightType.Point:
                        ActivePointLights.Add(lights[i]);
                        break;
                    default:
                        Debug.LogError(string.Format("{0}: Lights of type {1} aren't yet supported!", gameObject.name, lights[i].type));
                        break;
                }
            }
        }

        void OnDisable()
        {
            var lights = gameObject.GetComponents<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                switch(lights[i].type)
                {
                    case LightType.Directional:
                        ActiveDirectionalLights.Remove(lights[i]);
                        break;
                    case LightType.Point:
                        ActivePointLights.Remove(lights[i]);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
#endif
