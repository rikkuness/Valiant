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
using System.Threading;
using UnityEngine;

namespace BlueprintReality.MixCast.Profiling
{
    public class CPUPerformanceSpoiler : MonoBehaviour
    {
        public int iterations = 1000000;
        public int threadCount = 5;

        private VainCalculator[] vains;
        private bool isRunning;

        private class VainCalculator
        {
            public int iterations = 1000000;
            public bool isFinished = false;
            private Thread thread;

            public void Start()
            {
                thread = new Thread(new ThreadStart(Run));
                thread.Start();
            }

            public void Stop()
            {
                isFinished = true;
            }

            public void Run()
            {
                while (true)
                {
                    float nothing = 1f;

                    for (int i = 0; i < iterations; ++i)
                    {
                        nothing = Mathf.Pow(nothing, 1.1f);
                    }

                    if (isFinished)
                    {
                        break;
                    }
                }
            }
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.S))
            {
                if (threadCount > 0)
                {
                    //Debug.Log("Spinning threads! count: " + threadCount);

                    if (!isRunning)
                    {
                        isRunning = true;

                        vains = new VainCalculator[threadCount];

                        for (int i = 0; i < threadCount; ++i)
                        {
                            var calc = new VainCalculator();
                            calc.iterations = iterations;
                            calc.Start();

                            vains[i] = calc;
                        }
                    }
                }
                else
                {
                    Spoil();
                }
            }
            else
            {
                StopThreads();
            }
        }

        private void Spoil()
        {
            float nothing = 1f;

            for (int i = 0; i < iterations; ++i)
            {
                nothing = Mathf.Pow(nothing, 1.1f);
            }
        }

        private void StopThreads()
        {
            if (isRunning)
            {
                isRunning = false;

                for (int i = 0; i < threadCount; ++i)
                {
                    vains[i].Stop();
                }
            }
        }

        void OnDestroy()
        {
            StopThreads();
        }
    }
}
#endif
