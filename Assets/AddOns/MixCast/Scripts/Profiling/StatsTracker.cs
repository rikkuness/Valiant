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
using UnityEngine;

namespace BlueprintReality.MixCast.Profiling
{
    public class StatsTracker : MonoBehaviour
    {
        public virtual string Key { get; set; }

        public int Current { get; protected set; }
        public int Lowest { get; protected set; }
        public int Highest { get; protected set; }
        public int Avg { get; protected set; }
        public int Target { get; protected set; }
        public int SampleCount { get; protected set; }

        public bool IsReady { get { return SampleCount > 0; } }
        public bool IsTracking { get; protected set; }

        private int avgSum = 0;

        private Coroutine printCoroutine;

        protected void SetCurrent(int newCurrent)
        {
            ++SampleCount;

            Current = newCurrent;

            avgSum += Current;
            Avg = avgSum / SampleCount;

            if (Current < Lowest)
            {
                Lowest = Current;
            }

            if (Current > Highest)
            {
                Highest = Current;
            }
        }

        public void ResetStats()
        {
            Lowest = int.MaxValue;
            Highest = int.MinValue;
            Avg = 0;
            SampleCount = 0;
            avgSum = 0;
        }

        public virtual void StartTracking()
        {
            IsTracking = true;
        }

        public virtual void StopTracking()
        {
            IsTracking = false;
        }

        public void StartPrinting()
        {
            if (printCoroutine != null)
            {
                return;
            }

            printCoroutine = StartCoroutine(Print());
        }

        public void StopPrinting()
        {
            StopCoroutine(printCoroutine);

            printCoroutine = null;
        }

        private IEnumerator Print()
        {
            while (true)
            {
                Debug.Log(string.Format("KEY:{4} CUR:{0} AVG:{1} MIN:{2} MAX:{3}", Current, Avg, Lowest, Highest, Key));

                yield return new WaitForSeconds(1);
            }
        }
    }
}
#endif
