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
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlueprintReality.MixCast
{
	public class AudioCallbackContext : MonoBehaviour
	{
		private int callCount = 0;

        public int Call
		{
			get { return callCount; }
			set
			{
				callCount++;
			}
		}
		
        public void Callback(int dec) {
            if(Called != null) {
                Called( dec );
            }
            Call++;
        }
        public event System.Action<int> Called;
	}
}
#endif
