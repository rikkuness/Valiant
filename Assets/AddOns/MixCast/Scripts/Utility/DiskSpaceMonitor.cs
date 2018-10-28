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
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    /// <summary>
    /// Allows disk space to be monitored by polling it periodically.
    /// 
    /// The repeating check is a coroutine invoked via `MonoBehaviour.StartCoroutine`.
    /// </summary>
    public class DiskSpaceMonitor
    {
        public event Action OnLowDiskSpace;

        public float alertIfBelowMegabytes = 10; 
        public float checkInterval = 3; // Seconds
        public string directory = @"C:\";

        /// <summary>
        /// Coroutine that periodically checks whether the directory is low on free space.
        /// </summary>
        public IEnumerator MonitorDiskSpace()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkInterval);
                AlertIfBelowThreshold();
            }
        }

        void AlertIfBelowThreshold()
        {
            var freeDiskSpace = GetFreeDiskSpace(directory);
            var belowThreshold = freeDiskSpace < alertIfBelowMegabytes;

            if (belowThreshold && OnLowDiskSpace != null)
            {
                OnLowDiskSpace();
            }
        }

        /// <summary>
        /// Checks how much free space is available to the user.
        /// </summary>
        /// <param name="directory">Directory to check.</param>
        /// <returns>Amount of free space in MB.</returns>
        public static float GetFreeDiskSpace(string directory)
        {
            // We only care about the free bytes available, but we still need to pass the other arguments.
            ulong freeBytesAvailable;
            ulong totalNumberOfBytes;
            ulong totalNumberOfFreeBytes;
            GetDiskFreeSpaceEx(directory, out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes);
            return BytesToMegabytes(freeBytesAvailable);
        }

        static float BytesToMegabytes(ulong bytes)
        {
            return Convert.ToSingle(bytes) / 1000000;
        }

        #region DLL Imports

        [DllImport("kernel32.dll")]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes); 

        #endregion
    }
}
#endif
