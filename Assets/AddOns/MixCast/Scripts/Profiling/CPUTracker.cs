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
using System.Diagnostics;
using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;
using ComType = System.Runtime.InteropServices.ComTypes;

namespace BlueprintReality.MixCast.Profiling
{
    public class CPUTracker : StatsTracker
    {

        [DllImport("kernel32.dll")]
        static extern bool GetProcessTimes(IntPtr processHandle, out ComType.FILETIME creationTime,
            out ComType.FILETIME exitTime, out ComType.FILETIME kernelTime, out ComType.FILETIME userTime);

        [DllImport("kernel32.dll")]
        static extern bool GetSystemTimes(out ComType.FILETIME idleTime, out ComType.FILETIME kernelTime, out ComType.FILETIME userTime);

        public const string KeyName = "CPU";

        public override string Key { get { return KeyName; } }

        IntPtr currentProcessHandle;

        WaitForSeconds timeInterval;

        float intervalInSeconds = 1f;

        private void Start()
        {
            currentProcessHandle = Process.GetCurrentProcess().Handle;
            timeInterval = new WaitForSeconds(intervalInSeconds);
        }

        public override void StartTracking()
        {
            base.StartTracking();
            StartCoroutine("KeepTracking");
        }

        public override void StopTracking()
        {
            base.StopTracking();
            StopCoroutine("KeepTracking");
        }

        private ComType.FILETIME oldKernelTime;
        private ComType.FILETIME oldUserTime;
        private ComType.FILETIME oldProcessKernelTime;
        private ComType.FILETIME oldProcessUserTime;

        IEnumerator KeepTracking()
        {
            ComType.FILETIME idleTime, kernelTime, userTime, processCreationTime, processExitTime, processKernelTime, processUserTime;
            while (!GetSystemTimes(out idleTime, out kernelTime, out userTime) ||
                !GetProcessTimes(currentProcessHandle, out processCreationTime, out processExitTime, out processKernelTime, out processUserTime))
            {
                yield return null;
            }
            oldKernelTime = kernelTime;
            oldUserTime = userTime;
            oldProcessKernelTime = processKernelTime;
            oldProcessUserTime = processUserTime;

            while (true)
            {
                if (IsTracking)
                {
                    yield return timeInterval;

                    if (GetSystemTimes(out idleTime, out kernelTime, out userTime) &&
                        GetProcessTimes(currentProcessHandle, out processCreationTime, out processExitTime, out processKernelTime, out processUserTime))
                    {
                        int value = CpuUsage(kernelTime, userTime, processKernelTime, processUserTime);

                        SetCurrent(value);
                    }
                }
                else
                {
                    yield return null;
                }
            }
        }

        private int CpuUsage(ComType.FILETIME newKernelTime, ComType.FILETIME newUserTime, ComType.FILETIME newProcessKernelTime, ComType.FILETIME newProcessUserTime)
        {
            int rawUsage;

            ulong processKernelTime = SubtractTimes(newProcessKernelTime, oldProcessKernelTime);
            ulong processUserTime = SubtractTimes(newProcessUserTime, oldProcessUserTime);


            ulong kernelTime = SubtractTimes(newKernelTime, oldKernelTime);
            ulong userTime = SubtractTimes(newUserTime, oldUserTime);

            double processTotal = processUserTime + processKernelTime;
            double systemTotal = userTime + kernelTime;
            if (systemTotal <= double.Epsilon)
                return -1;
            double division = processTotal / systemTotal;
            rawUsage = Mathf.RoundToInt((float)division * 100f);

            oldUserTime = newUserTime;
            oldKernelTime = newKernelTime;
            oldProcessKernelTime = newProcessKernelTime;
            oldProcessUserTime = newProcessUserTime;

            return rawUsage;
        }

        private ulong SubtractTimes(ComType.FILETIME a, ComType.FILETIME b)
        {
            DateTime adate = ToDateTime(a);
            DateTime bdate = ToDateTime(b);
            return (ulong)(adate.Ticks - bdate.Ticks);
        }

        private DateTime ToDateTime(ComType.FILETIME time)
        {
            ulong high = (ulong)time.dwHighDateTime;
            uint low = (uint)time.dwLowDateTime;
            long fileTime = (long)((high << 32) + low);
            try
            {
                return DateTime.FromFileTimeUtc(fileTime);
            }
            catch
            {
                return DateTime.FromFileTimeUtc(0xFFFFFFFF);
            }
        }
    }
}
#endif
