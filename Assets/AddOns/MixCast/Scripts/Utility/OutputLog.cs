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
using System.IO;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class OutputLog : MonoBehaviour
    {
        public string logFileName = "MixCastLog.txt";
        private StreamWriter sw;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            if (!File.Exists(Path.Combine(Application.persistentDataPath, logFileName)))
            {
                File.Create(Path.Combine(Application.persistentDataPath, logFileName));
            }

        }

        private void OnEnable()
        {
            /*
             * There is also a Application.logMessageReceivedThreaded event
             * */

            Application.logMessageReceived += HandleLog;
            sw = new StreamWriter(Path.Combine(Application.persistentDataPath, logFileName));
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
            sw.Close();
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {

            sw.WriteLine(logString + " " + stackTrace);

        }
    }
}
#endif
